using Prism99_Core.Utilities;
using xTile;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System.Collections.Generic;
using System;
using ContentPackFramework.MapUtililities;
using xTile.Layers;
using xTile.Tiles;
using HarmonyLib;
using SDV_Realty_Core.Framework.Objects;
using System.Reflection;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class IMapUtilities : IService
    {
        public const string northSidePatch = "0";
        public const string eastSidePatch = "1";
        public const string southSidePatch = "2";
        public const string westSidePatch = "3";

        public struct MapLightSource
        {
            public int x;
            public int y;
            public int lightType;
            public int offset;
        }
        internal abstract int GetTileSheetId(Map map, string tileSheetSourceName);
        internal abstract bool IsSeasonalTileSheet(string tileSheetSourceName);
        internal abstract string WildCardSeason(string tileSheetSourceName);
        internal abstract void SetTile(Map map, int x, int y, int iTileSheetId, int iTileId, string sLayerId, bool overwriteTile);
        internal abstract void SetTile(Map map, int x, int y, int iTilesheetId, int iTileId, string sLayerId);
        internal abstract void SetTileProperty(GameLocation gl, int x, int y, string sLayerId, string propName, string propValue);
        internal void SetTileProperty(Map map, int x, int y, string layerId, string propName, string propValue)
        {
            try
            {
                Layer oLayer = map.GetLayer(layerId);
                if (oLayer.Tiles[x, y] == null)
                {
                    SetTile(map, x, y, 0, 0, layerId);
                }
                oLayer.Tiles[x, y].Properties[propName] = propValue;
                //if (oLayer.Tiles[x, y].Properties.ContainsKey(propName))
                //{
                //    oLayer.Tiles[x, y].Properties[propName] = propValue;
                //}
                //else
                //{
                //    oLayer.Tiles[x, y].Properties.Add(propName, propValue);
                //}
            }
            catch (Exception ex)
            {
                logger.Log($"SetTileProperty error: {ex}", LogLevel.Error);
                logger.Log($"Tile params: Location: {map.Id}, ({x}, {y}), LayerId='{layerId}', PropName='{propName}', PropValue='{propValue}'", LogLevel.Error);
            }
        }
        internal abstract void RemoveTileProperty(GameLocation gl, int x, int y, string sLayerId, string propName);
        internal abstract void PatchInMap(Map gl, Map oMap, Point vOffset, bool overlay = false);
        internal abstract void PatchInMap(Map gl, Map oMap, Vector2 offset, bool overlay = false);
        internal abstract void PatchInMap(GameLocation gl, Map oMap, Point vOffset, bool overlay = false);
        internal abstract void RemovePathBlock(Vector2 oBlockPos, GameLocation gl, WarpOrientations eBlockOrientation, int iBlockWidth);
        internal abstract void RemoveTile(GameLocation gl, int x, int y, string sLayer, int tileIndex);
        internal abstract void RemoveTile(GameLocation gl, int x, int y, string sLayer);
        internal abstract string GetNeighbourSide(string side);
        internal abstract string FormatDirectionText(string direction, string text);
        internal abstract void AddPathBlock(Vector2 oBlockPos, Map map, WarpOrientations eBlockOrientation, int iBlockWidth);
        internal abstract string GetSignPostMessagePrefix(string side);
        internal abstract string GetSignPostMessagePrefix(EntranceDirection side);
        internal abstract void OverlayMap(GameLocation gl, Map sourceMap, Point offset);
        internal abstract void OverlayMap(Map targetMap, Map sourceMap, Point offset);
        internal abstract void RemoveSignPost(GameLocation oExp, EntrancePatch oExpPatch, string messageKeySuffix);
        internal abstract void RemovePatchWarps(EntrancePatch oPatch, GameLocation glExp);
        internal abstract void ResetGameLocation(GameLocation map);
        internal List<MapLightSource> ReadLightSources(Map sourceMap)
        {
            List<MapLightSource> results = new();

            Layer backLayer = sourceMap.GetLayer("Back");

            if (backLayer != null)
            {
                //
                //  Scan for all valid tokens
                //
                for (int x = 0; x < backLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < backLayer.LayerHeight; y++)
                    {
                        if (backLayer.IsValidTileLocation(x, y))
                        {
                            Tile tile = backLayer.Tiles[x, y];
                            if (tile != null && tile.Properties != null)
                            {
                                foreach (var prop in tile.Properties)
                                {
                                    if (prop.Key == "DayNightLight")
                                    {
                                        if (string.IsNullOrEmpty(prop.Value))
                                        {
                                            results.Add(new MapLightSource { x = x, y = y, offset = 3, lightType = 4 });
                                        }
                                        else
                                        {
                                            string[] arValues = prop.Value.ToString().Split(' ');
                                            switch (arValues.Length)
                                            {
                                                case 1:
                                                    if (int.TryParse(prop.Value, out int lightType))
                                                    {
                                                        results.Add(new MapLightSource { x = x, y = y, offset = 3, lightType = lightType });
                                                    }
                                                    else
                                                        results.Add(new MapLightSource { x = x, y = y, offset = 3, lightType = 4 });
                                                    break;
                                                case 2:
                                                    if (int.TryParse(prop.Value, out int lightType1) && int.TryParse(arValues[1], out int offset))
                                                    {
                                                        results.Add(new MapLightSource { x = x, y = y, offset = offset, lightType = lightType1 });
                                                    }
                                                    else
                                                        results.Add(new MapLightSource { x = x, y = y, offset = 0, lightType = 4 });
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }
        internal void AdjustMapActions(Map map, string baseLocationName)
        {
            Layer back = map.GetLayer("Back");
            Layer buildings = map.GetLayer("Buildings");
            if (back != null && buildings != null)
            {
                for (int x = 0; x < back.LayerWidth; x++)
                {
                    for (int y = 0; y < back.LayerHeight; y++)
                    {
                        if (back.Tiles[x, y] != null && back.Tiles[x, y].Properties.TryGetValue("TouchAction", out var value))
                        {

                        }

                        if (buildings.Tiles[x, y] != null && buildings.Tiles[x, y].Properties.TryGetValue("Action", out var value2))
                        {
                            if (value2.ToString().StartsWith("Warp") && value2.ToString().EndsWith("Farm"))
                            {
                                buildings.Tiles[x, y].Properties["Action"] = value2.ToString().Replace("Farm", baseLocationName);
                            }
                        }
                    }
                }
            }
        }
        internal void AddTouchAction(Map map, Point tile, string action)
        {
            SetTileProperty(map, tile.X, tile.Y, "Back", "TouchAction", action);
        }
    }
}
