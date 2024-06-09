using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using xTile.Tiles;
using xTile.Layers;
using xTile;
using System.IO;
using StardewModHelpers;
using Prism99_Core.Utilities;
using SkiaSharp;
using StardewValley.GameData.Locations;


namespace SDV_MapRenderer
{
    internal class SDVMapRenderer
    {
        public struct MapOptions
        {
            public bool ShowFishAreas;
            public bool ShowFishAreaName;
            public bool IsWinter;
            public bool DrawBuildings;
            public bool DrawPropertyIndicator;
            public bool SkipPathsLayer;
        }
        internal static StardewBitmap RenderMap(string mapPath,string gamePath, SDVLogger logger, MapOptions mapOptions, List<string> layers = null)
        {
            return null;
        }
        internal static StardewBitmap RenderMap(GameLocation location, LocationData locationData, SDVLogger logger, MapOptions mapOptions, List<string> layers = null)
        {
            return RenderMap(location.Map, locationData, location, logger, mapOptions, layers);
        }
        internal static StardewBitmap RenderMap(Map map, LocationData locationData, GameLocation location, SDVLogger logger, MapOptions mapOptions, List<string> layers = null)
        {
            StardewBitmap bitmap = new StardewBitmap(map.DisplayWidth, map.DisplayHeight);
            Dictionary<string, TileSheet> sourceTileSheets = new();
            Dictionary<string, StardewBitmap> tsBitMaps = new();
            foreach (TileSheet tileSheet in map.TileSheets)
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
            
            foreach (Layer layer in map.Layers)
            {
                if (mapOptions.SkipPathsLayer && layer.Id == "Paths")
                    continue;

                if (layers == null || layers.Contains(layer.Id))
                {
                    for (int left = 0; left < layer.LayerWidth; left++)
                    {
                        for (int top = 0; top < layer.LayerHeight; top++)
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
                                    int tileWidth =  16;// tsBitMaps[layerTile.TileSheet.ImageSource].Width / sourceTileSheets[layerTile.TileSheet.ImageSource].SheetWidth;
                                    int tileHeight =  16;// tsBitMaps[layerTile.TileSheet.ImageSource].Height / sourceTileSheets[layerTile.TileSheet.ImageSource].SheetHeight;
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
                                    //if(mapOptions.DrawPropertyIndicator && layer.Tiles.Array[left, top].Properties!=null && layer.Tiles.Array[left, top].Properties.Count > 0)
                                    //{
                                    //    bitmap.DrawRectangle(Color.Black, left * 64, top * 64, 64, 64,3);
                                    //}
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

            if (location != null)
            {
                if(mapOptions.DrawBuildings && location.IsBuildableLocation() && location.buildings.Count>0)
                {
                    foreach(var building in location.buildings)
                    {
                        if (building.texture != null)
                        {
                            bitmap.DrawImage(new StardewBitmap( building.texture.Value), new Rectangle(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value), new Rectangle(0,0,building.texture.Value.Width,building.texture.Value.Height));
                        }
                    }
                }
            }


            //
            //  draw fishareas
            //
            if (mapOptions.ShowFishAreas)
            {
                if (locationData != null && locationData.FishAreas != null)
                {
                    int borderWidth = bitmap.Width switch
                    {
                        > 4000 => 6,
                        _ => 3
                    };
                    Color textColor = mapOptions.IsWinter ? Color.Black : Color.White;

                    foreach (var area in locationData.FishAreas)
                    {
                        if (area.Value.Position != null)
                        {
                            Rectangle areaPosition = area.Value.Position.GetValueOrDefault();

                            bitmap.DrawRectangle(Color.Red, areaPosition.X * Game1.tileSize, areaPosition.Y * Game1.tileSize, areaPosition.Width * Game1.tileSize, areaPosition.Height * Game1.tileSize, borderWidth);
                            if (mapOptions.ShowFishAreaName)
                            {
                                bitmap.DrawString(area.Value.DisplayName ?? area.Key, SKTextAlign.Left, "Arial", 64.0f, textColor, areaPosition.X * Game1.tileSize + 20, areaPosition.Y * Game1.tileSize + 60);
                            }
                        }
                    }
                }
            }
            return bitmap;
        }
        private static SKBitmap Rotate(SKBitmap bitmap, double angle)
        {
            double radians = Math.PI * angle / 180;
            float sine = (float)Math.Abs(Math.Sin(radians));
            float cosine = (float)Math.Abs(Math.Cos(radians));
            int originalWidth = bitmap.Width;
            int originalHeight = bitmap.Height;
            int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
            int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

            var rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);

            using (SKCanvas surface = new SKCanvas(rotatedBitmap))
            {
                surface.Clear();
                surface.Translate(rotatedWidth / 2, rotatedHeight / 2);
                surface.RotateDegrees((float)angle);
                surface.Translate(-originalWidth / 2, -originalHeight / 2);
                surface.DrawBitmap(bitmap, new SKPoint());
            }
            return rotatedBitmap;
        }

    }
}
