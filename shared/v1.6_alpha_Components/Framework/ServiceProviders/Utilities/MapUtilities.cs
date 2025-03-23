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

namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class MapUtilities : IMapUtilities
    {
        //private SDRContentManager contentManager;

        private ICustomEventsService customEventsService;
        public override List<string> CustomServiceEventTriggers
        { get => new List<string> {  }; }
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
                logger.LogError($"FEFramework.RemovePathBlock", ex); ;
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

        internal void setMapTileIndex(Map map, int tileX, int tileY, int index, string layer, int whichTileSheet = 0)
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
        internal override void PatchInMap(GameLocation gl, Map oMap, Vector2 vOffset)
        {
            PatchInMap(gl.Map, oMap, vOffset);
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
                        SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, 898, "Buildings");
                        SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y, tilesheetId, 899, "Buildings");
                        SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y + 1, tilesheetId, 923, "Buildings");
                        SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y + 1, tilesheetId, 924, "Buildings");
                        break;
                    case 3:
                        if (eBlockOrientation == WarpOrientations.Horizontal)
                        {
                            //
                            //  patch in a horizontal fence
                            //
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, 358, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y - 1, tilesheetId, 359, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 2, (int)oBlockPos.Y - 1, tilesheetId, 360, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, 383, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y, tilesheetId, 384, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 2, (int)oBlockPos.Y, tilesheetId, 385, "Buildings");
                        }
                        else
                        {
                            //
                            //  patch in a vertical fence
                            //
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y + 2, tilesheetId, 436, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y + 1, tilesheetId, 411, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, 411, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, 386, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, 361, "Buildings", false);
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, 361, "AlwaysFront");
                        }
                        break;
                    case 4:
                        if (eBlockOrientation == WarpOrientations.Horizontal)
                        {
                            //
                            //  patch in a horizontal fence
                            //
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, 358, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y - 1, tilesheetId, 359, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 2, (int)oBlockPos.Y - 1, tilesheetId, 359, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 3, (int)oBlockPos.Y - 1, tilesheetId, 360, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, 383, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 1, (int)oBlockPos.Y, tilesheetId, 384, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 2, (int)oBlockPos.Y, tilesheetId, 384, "Buildings");
                            SetTile(map, (int)oBlockPos.X + 3, (int)oBlockPos.Y, tilesheetId, 385, "Buildings");
                        }
                        else
                        {
                            //
                            //  patch in a vertical fence
                            //
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y, tilesheetId, 436, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 1, tilesheetId, 411, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 2, tilesheetId, 411, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 3, tilesheetId, 386, "Buildings");
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 4, tilesheetId, 361, "Buildings", false);
                            SetTile(map, (int)oBlockPos.X, (int)oBlockPos.Y - 4, tilesheetId, 361, "AlwaysFront");
                        }
                        break;
                }
            }
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
        internal override void PatchInMap(Map gl, Map oMap, Vector2 vOffset)
        {
            logger.Log($"PatchInMap Dest map: {gl.Description}, Src map: {oMap.Description}", LogLevel.Debug);
            foreach (Layer olayer in oMap.Layers)
            {
                Layer layer = gl.GetLayer(olayer.Id);
                if (olayer != null)
                {
                    for (int iCol = 0; iCol < olayer.LayerHeight; iCol++)
                    {
                        for (int iRow = 0; iRow < olayer.LayerWidth; iRow++)
                        {
                            int iIndex = (olayer.Tiles[iRow, iCol] == null ? -1 : olayer.Tiles[iRow, iCol].TileIndex);
                            if (olayer.Tiles[iRow, iCol] != null && olayer.Tiles[iRow, iCol].TileIndex > 0)
                            {
                                if (layer.Tiles[iRow + (int)vOffset.X, iCol + (int)vOffset.Y] == null || layer.Tiles[iRow + (int)vOffset.X, iCol + (int)vOffset.Y].TileSheet.ImageSource != olayer.Tiles[iRow, iCol].TileSheet.ImageSource)
                                {
                                    layer.Tiles[iRow + (int)vOffset.X, iCol + (int)vOffset.Y] = new StaticTile(layer, gl.TileSheets[GetTileSheetId(gl, olayer.Tiles[iRow, iCol].TileSheet.ImageSource)], BlendMode.Alpha, olayer.Tiles[iRow, iCol].TileIndex);
                                }
                                else
                                    setMapTileIndex(gl, iRow + (int)vOffset.X, iCol + (int)vOffset.Y, olayer.Tiles[iRow, iCol].TileIndex, olayer.Id, GetTileSheetId(gl, olayer.Tiles[iRow, iCol].TileSheet.ImageSource));
                            }
                            else
                            {
                                if (layer.Tiles[iRow + (int)vOffset.X, iCol + (int)vOffset.Y] != null)
                                {
                                    gl.GetLayer(olayer.Id).Tiles[iRow + (int)vOffset.X, iCol + (int)vOffset.Y] = null;
                                }
                            }
                        }
                    }

                }
                else
                {
                    logger.Log($"Layer is null. Id= {olayer?.Id ?? "-1"}", LogLevel.Error);
                }
            }
        }

        internal override int GetTileSheetId(Map map, string tileSheetSourceName)
        {
            tileSheetSourceName = SDVPathUtilities.NormalizePath(tileSheetSourceName);

            string[] arParts = tileSheetSourceName.Split('*');

            for (int iTileSheet = 0; iTileSheet < map.TileSheets.Count; iTileSheet++)
            {
                if (tileSheetSourceName.Contains("*"))
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

            logger.Log($"GetTileSheetId. Could not find tilesheet: {map.Id} '{tileSheetSourceName}'", LogLevel.Error);

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
            try
            {
                Layer oLayer = gl.map.GetLayer(sLayerId);
                if (oLayer.Tiles[x, y] != null)
                {
                    if (oLayer.Tiles[x, y].Properties.ContainsKey(propName))
                    {
                        oLayer.Tiles[x, y].Properties[propName] = propValue;
                    }
                    else
                    {
                        oLayer.Tiles[x, y].Properties.Add(propName, propValue);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log($"SetTileProperty error: {ex}", LogLevel.Error);
                logger.Log($"Tile params: Location: {gl.Name}, ({x}, {y}), LayerId='{sLayerId}', PropName='{propName}', PropValue='{propValue}'", LogLevel.Error);
            }
        }

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }
    }
}
