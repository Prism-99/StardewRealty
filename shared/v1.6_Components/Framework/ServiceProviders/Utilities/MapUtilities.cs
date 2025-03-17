using System;
using System.Collections.Generic;
using Prism99_Core.Utilities;
using xTile;
using xTile.Layers;
using xTile.Tiles;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System.Linq;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System.IO;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;


namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class MapUtilities : IMapUtilities
    {
        //
        //  path blocking objects
        //
        // size = 2
        private const int boulder_TopLeft = 898;
        private const int boulder_TopRight = 899;
        private const int boulder_BottomLeft = 923;
        private const int boulder_BottomRight = 924;
        // size 3 and above
        // horizontal
        private const int fence_Horizontal_LeftEndTop = 358;
        private const int fence_Horizontal_MiddleTop = 359;
        private const int fence_Horizontal_RightEndTop = 360;
        private const int fence_Horizontal_LeftEndBottom = 383;
        private const int fence_Horizontal_MiddleBottom = 384;
        private const int fence_Horizontal_RightEndBottom = 385;
        //vertical
        private const int fence_Vertical_Bottom = 436;
        private const int fence_Vertical_BottomTop = 411;
        private const int fence_Vertical_Middle = 386;
        private const int fence_Vertical_Top = 361;

        private ICustomEventsService customEventsService;
        public override List<string> CustomServiceEventTriggers
        { get => new List<string> { }; }
        public override Type ServiceType => typeof(IMapUtilities);

        public override Type[] InitArgs => new Type[]
        {
            typeof(ICustomEventsService)
        };

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;

            customEventsService = (ICustomEventsService)args[0];

            //contentManager = ((IContentManagerService)args[0]).contentManager;
        }
        internal override string GetSignPostMessagePrefix(EntranceDirection side)
        {
            return side switch
            {
                EntranceDirection.West => "WestMessage",
                EntranceDirection.East => "EastMessage",
                EntranceDirection.North => "NorthMessage",
                EntranceDirection.South => "SouthMessage",
                _ => ""
            };
        }
        internal override void RemovePatchWarps(EntrancePatch oPatch, GameLocation glExp)
        {
            //
            //  remove all of the exits warps for a given side
            //
            if (oPatch.WarpOrientation == WarpOrientations.Vertical)
            {
                for (int y = oPatch.WarpOut.Y; y < oPatch.WarpOut.Y + oPatch.WarpOut.NumberofPoints; y++)
                {
                    var warp = glExp.warps.Where(p => p.X == oPatch.WarpOut.X && p.Y == y);
                    foreach (var w in warp)
                    {
                        glExp.warps.Remove(w);
                    }
                }
            }
            else
            {
                for (int x = oPatch.WarpOut.X; x < oPatch.WarpOut.X + oPatch.WarpOut.NumberofPoints; x++)
                {
                    var warp = glExp.warps.Where(p => p.Y == oPatch.WarpOut.Y && p.X == x);
                    foreach (var w in warp)
                    {
                        glExp.warps.Remove(w);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all elements of an expansion sign post
        /// </summary>
        /// <param name="oExp"></param>
        /// <param name="oExpPatch"></param>
        /// <param name="messageKeySuffix"></param>
        internal override void RemoveSignPost(GameLocation oExp, EntrancePatch oExpPatch, string messageKeySuffix)
        {
            if (oExpPatch.Sign != null)
            {
                string layerName = oExpPatch.Sign.UseFront ? "Front" : "Buildings";
                RemoveTile(oExp, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y, layerName);
                string messageKey = oExp.Name + "." + messageKeySuffix;

                customEventsService.TriggerCustomEvent("RemoveStringFromMap", new object[] { messageKey });
                RemoveTileProperty(oExp, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y, "Buildings", "Action");
                RemoveTile(oExp, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y - 1, "Front");
            }
        }
        internal override string GetSignPostMessagePrefix(string side)
        {
            return side switch
            {
                westSidePatch => "WestMessage",
                eastSidePatch => "EastMessage",
                northSidePatch => "NorthMessage",
                southSidePatch => "SouthMessage",
                _ => ""
            };
        }
        internal override void RemovePathBlock(Vector2 oBlockPos, GameLocation gl, WarpOrientations eBlockOrientation, int iBlockWidth)
        {
            logger.Log($"   Removing path block {gl.Name} {oBlockPos}, {eBlockOrientation} for {iBlockWidth}", LogLevel.Debug);

            try
            {
                Layer oBuilding = gl.map.GetLayer("Buildings");
                switch (iBlockWidth)
                {
                    case 2:
                        //
                        //  remove boulder
                        //
                        if (oBuilding.Tiles[(int)oBlockPos.X, (int)oBlockPos.Y] != null) gl.removeTile((int)oBlockPos.X, (int)oBlockPos.Y, "Buildings");
                        if (oBuilding.Tiles[(int)oBlockPos.X + 1, (int)oBlockPos.Y] != null) gl.removeTile((int)oBlockPos.X + 1, (int)oBlockPos.Y, "Buildings");
                        if (oBuilding.Tiles[(int)oBlockPos.X, (int)oBlockPos.Y + 1] != null) gl.removeTile((int)oBlockPos.X, (int)oBlockPos.Y + 1, "Buildings");
                        if (oBuilding.Tiles[(int)oBlockPos.X + 1, (int)oBlockPos.Y + 1] != null) gl.removeTile((int)oBlockPos.X + 1, (int)oBlockPos.Y + 1, "Buildings");
                        break;
                    default:
                        //
                        //  remove fence
                        //
                        if (eBlockOrientation == WarpOrientations.Horizontal)
                        {
                            RemoveTile(gl, (int)oBlockPos.X, (int)oBlockPos.Y - 1, "Buildings");
                            RemoveTile(gl, (int)oBlockPos.X, (int)oBlockPos.Y, "Buildings");
                            int iMid;
                            for (iMid = 1; iMid < iBlockWidth - 1; iMid++)
                            {
                                RemoveTile(gl, (int)oBlockPos.X + iMid, (int)oBlockPos.Y - 1, "Buildings");
                                RemoveTile(gl, (int)oBlockPos.X + iMid, (int)oBlockPos.Y, "Buildings");
                            }
                            RemoveTile(gl, (int)oBlockPos.X + iMid, (int)oBlockPos.Y - 1, "Buildings");
                            RemoveTile(gl, (int)oBlockPos.X + iMid, (int)oBlockPos.Y, "Buildings");
                        }
                        else
                        {
                            RemoveTile(gl, (int)oBlockPos.X, (int)oBlockPos.Y - 1, "AlwaysFront");
                            //RemoveTile(gl, (int)oBlockPos.X, (int)oBlockPos.Y -  1, "Buildings", 361);
                            RemoveTile(gl, (int)oBlockPos.X, (int)oBlockPos.Y, "Buildings");
                            int iMid;
                            for (iMid = 1; iMid < iBlockWidth; iMid++)
                            {
                                RemoveTile(gl, (int)oBlockPos.X, (int)oBlockPos.Y + iMid, "Buildings");
                            }

                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"MapUtilities.RemovePathBlock", ex); ;
            }
        }
        internal override void RemoveTile(GameLocation gl, int x, int y, string sLayer, int tileIndex)
        {
            Layer oLayer = gl.map.GetLayer(sLayer);
            if (tileIndex == -1)
            {
                if (oLayer.Tiles[x, y] != null) gl.removeTile(x, y, sLayer);
            }
            else
            {
                if (oLayer.Tiles[x, y] != null && oLayer.Tiles[x, y].TileIndex == tileIndex) gl.removeTile(x, y, sLayer);
            }
        }
        internal override void RemoveTile(GameLocation gl, int x, int y, string sLayer)
        {
            RemoveTile(gl, x, y, sLayer, -1);
        }
        internal void SetMapTileIndex(Map map, int tileX, int tileY, int index, string layer, int whichTileSheet = 0)
        {
            if (map.GetLayer(layer).Tiles[tileX, tileY] != null)
            {
                if (index == -1)
                {
                    map.GetLayer(layer).Tiles[tileX, tileY] = null;
                }
                else
                {
                    map.GetLayer(layer).Tiles[tileX, tileY].TileIndex = index;
                }
            }
            else if (index != -1)
            {
                map.GetLayer(layer).Tiles[tileX, tileY] = new StaticTile(map.GetLayer(layer), map.TileSheets[whichTileSheet], BlendMode.Alpha, index);
            }
        }
        internal override void PatchInMap(GameLocation gl, Map oMap, Point vOffset, bool overlay = false)
        {
            PatchInMap(gl.Map, oMap, vOffset, overlay);
        }
        internal override void AddPathBlock(Vector2 oBlockPos, Map map, WarpOrientations eBlockOrientation, int iBlockWidth)
        {
            //
            //  add building layer tiles to block the exit path
            //

            //
            //  find the name of the outdoor tilesheet
            //  the blocking tiles in the outdoor tilesheet
            //
            string season = Game1.currentSeason.ToLower();
            int tilesheetId = GetTileSheetId(map, $"Maps\\*_outdoorstilesheet");
            if (tilesheetId == -1)
            {
                tilesheetId = GetTileSheetId(map, $"Maps\\*_outdoorstilesheet.png");
            }
            if (tilesheetId == -1)
            {
                tilesheetId = GetTileSheetId(map, $"Maps\\{season}_outdoorstilesheet.png");
            }
            if (tilesheetId == -1)
            {
                tilesheetId = GetTileSheetId(map, $"Maps\\{season}_outdoorstilesheet");
            }
            if (tilesheetId == -1)
            {
                logger?.LogOnce($"Could not find tilesheet '{season}'", LogLevel.Error);
            }
            else
            {
                //
                //  add either a boulder or fence depending upon the exit width
                //
                //  should add support for width of x
                //
                //  currently only supports widths of 2,3,4
                //
                switch (iBlockWidth)
                {
                    case 2:
                        //  patch in a boulder
                        SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, boulder_TopLeft, "Buildings");
                        SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y, tilesheetId, boulder_TopRight, "Buildings");
                        SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y + 1, tilesheetId, boulder_BottomLeft, "Buildings");
                        SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y + 1, tilesheetId, boulder_BottomRight, "Buildings");
                        break;
                    case 3:
                        if (eBlockOrientation == WarpOrientations.Horizontal)
                        {
                            //
                            //  patch in a horizontal fence
                            //
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, fence_Horizontal_LeftEndTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y - 1, tilesheetId, fence_Horizontal_MiddleTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 2, (int)oBlockPos.Y - 1, tilesheetId, fence_Horizontal_RightEndTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, fence_Horizontal_LeftEndBottom, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y, tilesheetId, fence_Horizontal_MiddleBottom, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 2, (int)oBlockPos.Y, tilesheetId, fence_Horizontal_RightEndBottom, "Buildings");
                        }
                        else
                        {
                            //
                            //  patch in a vertical fence
                            //
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y + 2, tilesheetId, fence_Vertical_Bottom, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y + 1, tilesheetId, fence_Vertical_BottomTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, fence_Vertical_BottomTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, fence_Vertical_Middle, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, fence_Vertical_Top, "Buildings", false);
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, fence_Vertical_Top, "AlwaysFront");
                        }
                        break;
                    case 4:
                        if (eBlockOrientation == WarpOrientations.Horizontal)
                        {
                            //
                            //  patch in a horizontal fence
                            //
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, fence_Horizontal_LeftEndTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y - 1, tilesheetId, fence_Horizontal_MiddleTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 2, (int)oBlockPos.Y - 1, tilesheetId, fence_Horizontal_MiddleTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 3, (int)oBlockPos.Y - 1, tilesheetId, fence_Horizontal_RightEndTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, fence_Horizontal_LeftEndBottom, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y, tilesheetId, fence_Horizontal_MiddleBottom, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 2, (int)oBlockPos.Y, tilesheetId, fence_Horizontal_MiddleBottom, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 3, (int)oBlockPos.Y, tilesheetId, fence_Horizontal_RightEndBottom, "Buildings");
                        }
                        else
                        {
                            //
                            //  patch in a vertical fence
                            //
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y + 3, tilesheetId, fence_Vertical_Bottom, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y + 2, tilesheetId, fence_Vertical_BottomTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y + 1, tilesheetId, fence_Vertical_BottomTop, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, fence_Vertical_Middle, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, fence_Vertical_Middle, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 2, tilesheetId, fence_Vertical_Top, "Buildings", false);
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 2, tilesheetId, fence_Vertical_Top, "AlwaysFront");


                            //SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, fence_Vertical_Bottom, "Buildings");
                            //SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, fence_Vertical_BottomTop, "Buildings");
                            //SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 2, tilesheetId, fence_Vertical_BottomTop, "Buildings");
                            //SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 3, tilesheetId, fence_Vertical_Middle, "Buildings");
                            //SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 4, tilesheetId, fence_Vertical_Top, "Buildings", false);
                            //SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 4, tilesheetId, fence_Vertical_Top, "AlwaysFront");
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// Resets the game location to default
        /// called after selling a location
        /// </summary>
        /// <param name="location">Location to be reset</param>
        internal override void ResetGameLocation(GameLocation location)
        {
            location.terrainFeatures.Clear();
            location.buildings.Clear();
            location.animals.Clear();
            location.furniture.Clear();
            location.critters.Clear();
            location.characters.Clear();

            // remove fences
            List<Vector2> toDelete = new();
            var fences = DataLoader.Fences(Game1.content);
            foreach (KeyValuePair<Vector2, SDObject> pair in location.objects.Pairs)
            {
                if (fences.ContainsKey(pair.Value.ItemId))
                    toDelete.Add(pair.Key);
                else if (Game1.objectData.TryGetValue(pair.Value.ItemId, out var data))
                {
                    if (data.Type != "Litter")
                        toDelete.Add(pair.Key);
                }                
            }
            foreach(Vector2 del in toDelete)
                location.objects.Remove(del);

            //
            //  reset weeds and debris
            //
            location.loadWeeds();
        }
        /// <summary>
        /// Formats sign post text with appropriate direction icon (if available)
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="text"></param>
        /// <returns>String with direction symbol and sign text</returns>
        internal override string FormatDirectionText(string direction, string text)
        {
            return direction switch
            {
                eastSidePatch => "> " + text,
                westSidePatch => "@ " + text,
                northSidePatch => "` " + text,
                southSidePatch => "" + text,
                _ => text
            };
        }
        internal override string GetNeighbourSide(string side)
        {
            return side switch
            {
                westSidePatch => eastSidePatch,
                eastSidePatch => westSidePatch,
                northSidePatch => southSidePatch,
                southSidePatch => northSidePatch,
                _ => ""
            };
        }
        internal override void OverlayMap(GameLocation gl, Map sourceMap, Point offset)
        {
            PatchInMap(gl.map, sourceMap, offset, true);
        }
        internal override void OverlayMap(Map targetMap, Map sourceMap, Point offset)
        {
            PatchInMap(targetMap, sourceMap, offset, true);
        }
        internal override void PatchInMap(Map targetMap, Map sourceMap, Vector2 vOffset, bool overlay = false)
        {
            PatchInMap(targetMap,sourceMap,new Point((int)vOffset.X,(int)vOffset.Y));
        }
        private void DumpProperties(Layer sourceLayer)
        {
            for (int x = 0; x < sourceLayer.LayerWidth; x++)
            {
                for (int y = 0; y < sourceLayer.LayerHeight; y++)
                {
                    if (sourceLayer.Tiles[x, y]?.Properties.Count > 0)
                    {
                        Console.WriteLine($"({x},{y})");
                        foreach(var prop in sourceLayer.Tiles[x, y].Properties)
                        {
                            Console.WriteLine($"{prop.Key}: {prop.Value}");
                        }
                    }
                }
            }
        }
        internal override void PatchInMap(Map targetMap, Map sourceMap, Point vOffset, bool overlay = false)
        {
            logger.Log($"PatchInMap Dest map: {targetMap.Id}, Src map: {sourceMap.Id}", LogLevel.Debug);

            foreach (Layer sourceLayer in sourceMap.Layers)
            {
                Layer targetLayer = targetMap.GetLayer(sourceLayer.Id);
                if (sourceLayer != null && targetLayer != null)
                {
                    for (int x = 0; x < sourceLayer.LayerWidth; x++)
                    {
                        for (int y = 0; y < sourceLayer.LayerHeight; y++)
                        {
                            int iIndex = (sourceLayer.Tiles[x, y] == null ? -1 : sourceLayer.Tiles[x, y].TileIndex);
                            if (sourceLayer.Tiles[x, y] != null && sourceLayer.Tiles[x, y].TileIndex > 0)
                            {

                                Tile tile = sourceLayer.Tiles[x, y];
                                Tile tile2 = null;
                                if (!(tile is StaticTile))
                                {
                                    if (tile is AnimatedTile animatedTile)
                                    {
                                        StaticTile[] array = new StaticTile[animatedTile.TileFrames.Length];
                                        for (int num5 = 0; num5 < animatedTile.TileFrames.Length; num5++)
                                        {
                                            StaticTile staticTile = animatedTile.TileFrames[num5];
                                            
                                            array[num5] = new StaticTile(targetLayer, targetMap.TileSheets[GetTileSheetId(targetMap, staticTile.TileSheet.ImageSource)], staticTile.BlendMode, staticTile.TileIndex);
                                        }

                                        tile2 = new AnimatedTile(targetLayer, array, animatedTile.FrameInterval);
                                    }
                                }
                                else
                                {
                                    //targetMap.TileSheets[GetTileSheetId(targetMap, tile.TileSheet.ImageSource)]
                                    tile2 = new StaticTile(targetLayer, targetMap.TileSheets[GetTileSheetId(targetMap, tile.TileSheet.ImageSource)], tile.BlendMode, tile.TileIndex);
                                }

                                tile2?.Properties.CopyFrom(tile.Properties);
                                targetLayer.Tiles[x+vOffset.X, y+vOffset.Y] = tile2;



                                //if (targetLayer.Tiles[x + (int)vOffset.X, y + (int)vOffset.Y] == null || targetLayer.Tiles[x + (int)vOffset.X, y + (int)vOffset.Y].TileSheet.ImageSource != sourceLayer.Tiles[x, y].TileSheet.ImageSource)
                                //{
                                //    targetLayer.Tiles[x + (int)vOffset.X, y + (int)vOffset.Y] = new StaticTile(targetLayer, targetMap.TileSheets[GetTileSheetId(targetMap, sourceLayer.Tiles[x, y].TileSheet.ImageSource)], BlendMode.Alpha, sourceLayer.Tiles[x, y].TileIndex);
                                //}
                                //else
                                //    SetMapTileIndex(targetMap, x + (int)vOffset.X, y + (int)vOffset.Y, sourceLayer.Tiles[x, y].TileIndex, sourceLayer.Id, GetTileSheetId(targetMap, sourceLayer.Tiles[x, y].TileSheet.ImageSource));

                                ////  add tile properties
                                //targetLayer.Tiles[x + (int)vOffset.X, y + (int)vOffset.Y].Properties.Clear();
                                //foreach (var prop in sourceLayer.Tiles[x, y].Properties)
                                //{
                                //    targetLayer.Tiles[x + (int)vOffset.X, y + (int)vOffset.Y].Properties.Add(prop.Key, prop.Value);
                                //}
                            }
                            else
                            {
                                if (!overlay && targetLayer.Tiles[x + (int)vOffset.X, y + (int)vOffset.Y] != null)
                                {
                                    targetMap.GetLayer(sourceLayer.Id).Tiles[x + (int)vOffset.X, y + (int)vOffset.Y] = null;
                                }
                            }
                        }
                    }

                }
                else
                {
                    logger.Log($"Layer is null. Id= {sourceLayer?.Id ?? "-1"}", LogLevel.Error);
                }
            }
        }
        internal override int GetTileSheetId(Map map, string tileSheetSourceName)
        {
            //tileSheetSourceName = SDVPathUtilities.NormalizePath(tileSheetSourceName);

            var tileSheet = map.TileSheets.Where(p => p.ImageSource!=null&& Path.GetFileNameWithoutExtension(p.ImageSource) == Path.GetFileNameWithoutExtension(tileSheetSourceName));

            //if(tileSheet.Any())
            //{
            //    string id = tileSheet.First().Id;
            //}
            string[] arParts = tileSheetSourceName.Split('*');

            for (int iTileSheet = 0; iTileSheet < map.TileSheets.Count; iTileSheet++)
            {
                if(Path.GetFileNameWithoutExtension(map.TileSheets[iTileSheet].ImageSource) == Path.GetFileNameWithoutExtension(tileSheetSourceName))
                {
                    return iTileSheet;
                }
                if (tileSheetSourceName.Contains("*"))
                {
                    if (arParts.Length == 2)
                    {
                        string normalized = SDVPathUtilities.NormalizePath(map.TileSheets[iTileSheet].ImageSource).ToLower();
                        if (normalized.StartsWith(arParts[0].ToLower()) && normalized.EndsWith(arParts[1].ToLower()))
                            return iTileSheet;
                    }
                    else
                    {
                        bool allParts = true;
                        foreach (string part in arParts)
                        {
                            if (!SDVPathUtilities.NormalizePath(map.TileSheets[iTileSheet].ImageSource).ToLower().Contains(part.ToLower()))
                            {
                                allParts = false;
                                break;
                            }
                        }
                        if (allParts) return iTileSheet;
                    }
                }
                else
                {
                    if (SDVPathUtilities.NormalizePath(map.TileSheets[iTileSheet].ImageSource).ToLower() == tileSheetSourceName.ToLower())
                    {
                        return iTileSheet;
                    }
                }
            }

            if (IsSeasonalTileSheet(tileSheetSourceName))
            {
                return GetTileSheetId(map, WildCardSeason(tileSheetSourceName));
            }

            logger.Log($"GetTileSheetId. Could not find tilesheet: {map.assetPath} '{tileSheetSourceName}'", LogLevel.Error);

            return -1;
        }
        internal override bool IsSeasonalTileSheet(string tileSheetSourceName)
        {
            string lowerName = tileSheetSourceName.ToLower();

            return lowerName.Contains("summer") || lowerName.Contains("spring") || lowerName.Contains("fall") || lowerName.Contains("winter");
        }
        internal override string WildCardSeason(string tileSheetSourceName)
        {
            int iDelim = tileSheetSourceName.IndexOf('_');

            if (iDelim == -1) return tileSheetSourceName;

            return "*" + tileSheetSourceName.Substring(iDelim);
        }
        internal override void SetTile(Map map, int x, int y, int iTileSheetId, int iTileId, string sLayerId, bool overwriteTile)
        {
            //
            //  sets a tile to a new tileid, if the tile does not exist
            //  a new tile is added to the layer
            //
            try
            {
                Layer oLayer = map.GetLayer(sLayerId);

                if (oLayer.IsValidTileLocation(x, y))
                {
                    if (oLayer.Tiles[x, y] == null)
                    {
                        oLayer.Tiles[x, y] = new StaticTile(oLayer, map.TileSheets[iTileSheetId], BlendMode.Alpha, iTileId);
                    }
                    else if (overwriteTile)
                    {
                        oLayer.Tiles[x, y] = null;
                        oLayer.Tiles[x, y] = new StaticTile(oLayer, map.TileSheets[iTileSheetId], BlendMode.Alpha, iTileId);
                    }
                }
                else
                {
                    logger.Log($"SetTile error: Invalid tile {x},{y}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Log($"SetTile error: {ex}", LogLevel.Error);
                logger.Log($"Tile params: Location: {map.Id}, ({x}, {y}), TileSheetId={iTileSheetId}, TileId={iTileId}, LayerId='{sLayerId}'", LogLevel.Error);
            }
        }
        internal override void SetTile(Map map, int x, int y, int iTilesheetId, int iTileId, string sLayerId)
        {
            SetTile(map, x, y, iTilesheetId, iTileId, sLayerId, true);
        }
        internal override void RemoveTileProperty(GameLocation gl, int x, int y, string sLayerId, string propName)
        {
            Layer oLayer = gl.map.GetLayer(sLayerId);
            if (oLayer.Tiles[x, y] != null)
            {
                oLayer.Tiles[x, y].Properties.Remove(propName);
            }
        }
        internal override void SetTileProperty(GameLocation gl, int x, int y, string sLayerId, string propName, string propValue)
        {
            SetTileProperty(gl.Map,x,y,sLayerId,propName,propValue);            
        }
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }
    }
}
