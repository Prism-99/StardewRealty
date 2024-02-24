using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using xTile;
using SkiaSharp;
using StardewModHelpers;
using Microsoft.Xna.Framework.Graphics;
using xTile.Tiles;
using System.IO;

namespace SDV_Realty_Core.Framework.ServiceProviders.GUI
{
    internal class MapRendererService : IMapRendererService
    {
        private IGameEnvironmentService _gameEnvironmentService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEnvironmentService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            _gameEnvironmentService= (IGameEnvironmentService)args[0];
        }
        internal override StardewBitmap RenderMap(Map map, List<string> layers = null)
        {
            StardewBitmap bitmap = new StardewBitmap(map.DisplayWidth, map.DisplayHeight);
            Dictionary<string, TileSheet> sourceTileSheets = new();
            Dictionary<string, StardewBitmap> tsBitMaps = new();
            foreach (var tileSheet in map.TileSheets)
            {
                if (Path.GetFileName(tileSheet.ImageSource) != Path.GetFileNameWithoutExtension(tileSheet.ImageSource))
                {
                    logger.Log($"Map ({Path.GetFileName(map.assetPath)}) has tilesheet with extension {tileSheet.ImageSource}", LogLevel.Error);
                }
                if (tsBitMaps.ContainsKey(tileSheet.ImageSource))
                {
                    logger.Log($"Map ({Path.GetFileName(map.assetPath)}) has duplicate tilesheet references [{tileSheet.ImageSource}].", LogLevel.Warn);
                }
                else
                {
                    tsBitMaps.Add(tileSheet.ImageSource, new StardewBitmap(Game1.content.Load<Texture2D>($"{tileSheet.ImageSource}")));
                    //tsBitMaps.Add(tileSheet.ImageSource, new StardewBitmap(Game1.content.Load<Texture2D>($"{Path.GetDirectoryName(tileSheet.ImageSource)}/{Path.GetFileNameWithoutExtension(tileSheet.ImageSource)}" )));
                    sourceTileSheets.Add(tileSheet.ImageSource, tileSheet);
                }
            }
            foreach (var layer in map.Layers)
            {
                if (layers == null || layers.Contains(layer.Id))
                {
                    for (int left = 0; left < layer.DisplayWidth / 64; left++)
                    {
                        for (int top = 0; top < layer.DisplayHeight / 64; top++)
                        {
                            if (layer.Tiles.Array[left, top] != null)
                            {
                                int rotation = 0;
                                Tile layerTile = layer.Tiles.Array[left, top];
                                if (layerTile.Properties != null && layerTile.Properties.ContainsKey("@Rotation"))
                                {
                                    int.TryParse(layerTile.Properties["@Rotation"].ToString(), out rotation);
                                }
                                if (tsBitMaps.ContainsKey(layerTile.TileSheet.ImageSource))
                                {
                                    //int columns = tsBitMaps[layerTile.TileSheet.ImageSource].Width / 16;
                                    //if(sourceTileSheets[layerTile.TileSheet.ImageSource].SheetWidth!=columns)
                                    //{
                                    //    int xx = 1;
                                    //}
                                    int tileWidth = 16;// tsBitMaps[layerTile.TileSheet.ImageSource].Width / sourceTileSheets[layerTile.TileSheet.ImageSource].SheetWidth;
                                    int tileHeight = 16;// tsBitMaps[layerTile.TileSheet.ImageSource].Height / sourceTileSheets[layerTile.TileSheet.ImageSource].SheetHeight;
                                    int tileX = layerTile.TileIndex % sourceTileSheets[layerTile.TileSheet.ImageSource].SheetWidth;
                                    int tileY = (int)Math.Floor((double)layerTile.TileIndex / sourceTileSheets[layerTile.TileSheet.ImageSource].SheetWidth);
                                    int tsLeft = tileWidth * tileX;
                                    int tsTop = tileHeight * tileY;

                                    //int srcLeft = tile.TileSheet.TileWidth * tile;
                                    if (rotation == 0)
                                    {
                                        bitmap.DrawImage(tsBitMaps[layerTile.TileSheet.ImageSource], new Rectangle(left * 64, top * 64, 64, 64), new Rectangle(tsLeft, tsTop, tileWidth, tileHeight));
                                    }
                                    else
                                    {
                                        StardewBitmap sprite = new StardewBitmap(16, 16);
                                        sprite.DrawImage(tsBitMaps[layerTile.TileSheet.ImageSource], new Rectangle(0, 0, 16, 16), new Rectangle(tsLeft, tsTop, tileWidth, tileHeight));
                                        SKBitmap rotated = Rotate(sprite.SourceImage, rotation);
                                        sprite = new StardewBitmap(rotated);
                                        bitmap.DrawImage(sprite, new Rectangle(left * 64, top * 64, 64, 64), new Rectangle(0, 0, tileWidth, tileHeight));
                                    }
                                }
                            }
                        }
                    }
                }
                //foreach (var tile in layer.Tiles.Array)
                //{

                //    if (tile != null)
                //    {
                //        if (tsBitMaps.ContainsKey(tile.TileSheet.ImageSource))
                //        {
                //            int tileX = tile.TileIndex % tsBitMaps[tile.TileSheet.ImageSource].Width / tile.TileSheet.TileWidth;
                //            int tileY = tile.TileIndex / tsBitMaps[tile.TileSheet.ImageSource].Width / tile.TileSheet.TileWidth;
                //            int tsLeft = tile.TileSheet.TileWidth * tileX;
                //            int tsTop = tile.TileSheet.TileHeight * tileY;

                //            //int srcLeft = tile.TileSheet.TileWidth * tile;
                //            bitmap.DrawImage(tsBitMaps[tile.TileSheet.ImageSource], new Rectangle(tsLeft, tsTop, tile.TileSheet.TileWidth, tile.TileSheet.TileHeight), new Rectangle(0,0,16,16));
                //        }
                //    }
                //}
            }
            return bitmap;
        }
        public SKBitmap Rotate(SKBitmap bitmap, double angle)
        {
            double radians = Math.PI * angle / 180;
            float sine = (float)Math.Abs(Math.Sin(radians));
            float cosine = (float)Math.Abs(Math.Cos(radians));
            int originalWidth = bitmap.Width;
            int originalHeight = bitmap.Height;
            int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
            int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

            var rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);

            using (var surface = new SKCanvas(rotatedBitmap))
            {
                surface.Clear();
                surface.Translate(rotatedWidth / 2, rotatedHeight / 2);
                surface.RotateDegrees((float)angle);
                surface.Translate(-originalWidth / 2, -originalHeight / 2);
                surface.DrawBitmap(bitmap, new SKPoint());
            }
            return rotatedBitmap;
        }

        internal override StardewBitmap RenderLocationMap(string locationName)
        {
            GameLocation gl = Game1.getLocationFromName(locationName);

            if (gl != null)
            {
                return RenderMap(gl.map);
            }

            return null;
        }

        internal override StardewBitmap RenderIndoorLocation(string buildingName)
        {
            if (Game1.buildingData.TryGetValue(buildingName, out var building))
            {
                if (!string.IsNullOrEmpty(building.IndoorMap))
                {
                    return RenderMap(Game1.content.Load<Map>(building.IndoorMap));
                }
            }

            return null;
        }

        internal override StardewBitmap RenderLocationMap(string locationName, List<string> layers)
        {
            GameLocation gl = Game1.getLocationFromName(locationName);

            if (gl != null)
            {
                return RenderMap(gl.map, layers);
            }

            return null;
        }

        internal override StardewBitmap RenderMap(string assetPat, List<string> layers = null)
        {
            throw new NotImplementedException();
        }
    }
}
