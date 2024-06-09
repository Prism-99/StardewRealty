using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using System.Runtime.CompilerServices;



namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    /// <summary>
    /// Version 1.6
    /// </summary>
    internal class GridManager : IGridManager
    {
        public struct FarmProfile
        {
            //
            //  flags for farm Exits
            //

            public bool UseFarmExit1;
            public bool UseFarmExit2;
            public bool PatchFarmExits;
            public bool PatchBackwoodExits;
            public bool SDEInstalled;
        }

        private IMapUtilities mapUtilities;
        private IContentManagerService contentManager;
        private IContentPackService contentPackService;
        private IUtilitiesService utilitiesService;
        private IModHelperService modHelperService;
        private IMineCartMenuService mineCartMenuService;
        private IGameEnvironmentService gameEnvironmentService;

        public override Type ServiceType => typeof(IGridManager);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IModDataService), typeof(IMapUtilities),
            typeof(IContentManagerService) , typeof(IContentPackService),
            typeof(IUtilitiesService),typeof(IModHelperService),
            typeof(IMineCartMenuService),typeof(IGameEnvironmentService)
        };
        public override List<string> CustomServiceEventTriggers => new List<string>
        {

        };
        internal override void Initialize(ILoggerService logger, object[] args)
        {
            modDataService = (IModDataService)args[0];
            mapUtilities = (IMapUtilities)args[1];
            contentManager = (IContentManagerService)args[2];
            contentPackService = (IContentPackService)args[3];
            utilitiesService = (IUtilitiesService)args[4];
            modHelperService = (IModHelperService)args[5];
            mineCartMenuService = (IMineCartMenuService)args[6];
            gameEnvironmentService = (IGameEnvironmentService)args[7];

            this.logger = logger;

            MapGrid = modDataService.MapGrid;

        }
        #region "Public Methods"

        internal override string SwapGridLocations(int GridIdA, int GridIdB)
        {
            string result = "";

            if (!IsMapGridOccupied(GridIdA))
            {
                result = $"{GridIdA} is not occupied.";
            }
            else if (!IsMapGridOccupied(GridIdB))
            {
                result = $"{GridIdB} is not occupied.";
            }
            else
            {
                string farmA = MapGrid[GridIdA];
                string farmB = MapGrid[GridIdB];
                //
                //  re-block each map
                //
                AddAllMapExitBlockers(farmA);
                AddAllMapExitBlockers(farmB);
                //
                //  update modData
                //
                modDataService.farmExpansions[farmA].GridId = GridIdB;
                modDataService.farmExpansions[farmA].modData["prism99.advize.stardewrealty.FEGridId"] = GridIdB.ToString();
                modDataService.farmExpansions[farmB].GridId = GridIdA;
                modDataService.farmExpansions[farmB].modData["prism99.advize.stardewrealty.FEGridId"] = GridIdA.ToString();
                //
                //  swap references
                //
                MapGrid[GridIdA] = farmB;
                MapGrid[GridIdB] = farmA;
                //
                //  re-patch maps
                //
                PatchInMap(GridIdA);
                PatchInMap(GridIdB);
                //
                //  invalidate world map
                //

                utilitiesService.InvalidateCache("Data/WorldMap");

                //
                //  invalidate map strings
                //
                utilitiesService.InvalidateCache("Strings/StringsFromMaps");
                result = "Expansions swapped.";
            }
            return result;
        }
        internal override void PatchInMap(int iGridId)
        {
            string sExpansionName = MapGrid[iGridId];
#if DEBUG_LOG
            logger.Log($"     PatchInMap {iGridId}:{sExpansionName}", LogLevel.Debug);
#endif
            try
            {
                GameLocation oExp = modDataService.farmExpansions[sExpansionName];
                bool bAutoAdd = modDataService.farmExpansions[sExpansionName].AutoAdd;
                ExpansionPack oPackToActivate = modDataService.validContents[sExpansionName];
                //AddSign(ContentPacks.ValidContents[sExpansionName]);
                if (iGridId > -1)
                {
                    //
                    //  based upon the grid location, remove path blocks and add warps
                    //  to join new Expansion to its neighbours
                    //
                    FarmDetails oFarm = null;
                    ExpansionPack oNewPack = modDataService.validContents[sExpansionName];
                    EntrancePatch oExpPatch;
                    int iGridRow;
                    int iGridCol;
                    int iPatchIndex;

                    switch (iGridId)
                    {
                        case 0:
                            //
                            //  initial grid spot
                            //  linked off of the Backwoods
                            //
                            oFarm = GetFarmDetails(999999);

                            if (oFarm != null)
                            {
                                GameLocation glBackwoods = Game1.getLocationFromName(oFarm.MapName);
                                EntrancePatch oBackWoodsPatch = oFarm.PathPatches["0"];
                                //RemovePathBlock(new Vector2(oFarm.PathPatches["0"].PathBlockX, oFarm.PathPatches["0"].PathBlockY), glBackwoods, oBackWoodsPatch.WarpOrientation, oBackWoodsPatch.WarpOut.NumberofPoints);
                                iGridRow = iGridId / oFarm.PathPatches.Count;
                                iGridCol = iGridId % oFarm.PathPatches.Count;
                                //
                                //    add basic left side warp ins/outs
                                //
                                oExpPatch = oNewPack.EntrancePatches[eastSidePatch];
                                AddExpansionIntersection(oBackWoodsPatch, oFarm.MapName, oExpPatch, sExpansionName);

                                if (oExpPatch.Sign?.UseSign ?? false)
                                {
                                    AddSignPost(oExp, oExpPatch, "EastMessage", FormatDirectionText(eastSidePatch, glBackwoods.Name));
                                }
                                if (MapGrid.ContainsKey(1))
                                {
                                    JoinMaps(iGridId, iGridId + 1, westSidePatch);
                                }
                            }
                            break;
                        case 1:
                        case 4:
                        case 7:
                            //
                            //  top row 
                            //
                            //
                            //  get expansion to right details
                            //
                            int iRightGridId = iGridId == 1 ? 0 : iGridId - 3;

                            JoinMaps(iGridId, iRightGridId, eastSidePatch);

                            //
                            //  check for other neighbours from swapping
                            //
                            //  check for neighbour to the south
                            //
                            if (MapGrid.ContainsKey(iGridId + 1))
                            {
                                JoinMaps(iGridId, iGridId + 1, southSidePatch);
                            }
                            //
                            //  check for neighbour to the west
                            //
                            if (MapGrid.ContainsKey(iGridId + 3))
                            {
                                JoinMaps(iGridId, iGridId + 3, westSidePatch);
                            }
                            break;
                        case 2:
                        case 5:
                        case 8:
                            //
                            //  middle row
                            //
                            //  remove patch block to the right
                            //
                            iPatchIndex = iGridId == 2 ? -1 : iGridId - 3;
                            string sRightKey = iPatchIndex == -1 ? "Farm" : MapGrid[iPatchIndex];
                            string sMessageKey = iPatchIndex == -1 ? "WestMessage.1" : "WestMessage";

                            GameLocation glMidRight = Game1.getLocationFromName(sRightKey);
                            EntrancePatch oMidRightPatch = null;
                            ExpansionPack oMidRightPack = null;

                            if (iPatchIndex == -1)
                            {
                                oFarm = GetFarmDetails(Game1.whichFarm);
                                oMidRightPatch = oFarm?.PathPatches["0"];
                            }
                            else
                            {
                                oMidRightPack = modDataService.validContents[MapGrid[iPatchIndex]];
                                oMidRightPatch = oMidRightPack.EntrancePatches[westSidePatch];
                            }

                            if (oMidRightPatch != null && (iPatchIndex == -1 && utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit1) || iPatchIndex > -1)
                            {
                                //RemovePathBlock(new Vector2(oMidRightPatch?.PathBlockX ?? 0, oMidRightPatch?.PathBlockY ?? 0), glMidRight, oMidRightPatch?.WarpOrientation ?? 0, oMidRightPatch?.WarpOut.NumberofPoints ?? 0);

                                //
                                //    add basic right/left side warp ins/outs
                                //
                                oExpPatch = oNewPack.EntrancePatches[eastSidePatch];
                                if (oMidRightPatch != null)
                                    AddExpansionIntersection(oMidRightPatch, sRightKey, oExpPatch, sExpansionName);

                                if (oExpPatch.Sign?.UseSign ?? false)
                                {
                                    AddSignPost(oExp, oExpPatch, "EastMessage", FormatDirectionText(eastSidePatch, iPatchIndex == -1 ? "Farm" : oMidRightPack.DisplayName));
                                }
                                if (oMidRightPatch.Sign != null && (oMidRightPatch.Sign?.UseSign ?? false))
                                {
                                    AddSignPost(Game1.getLocationFromName(sRightKey), oMidRightPatch, sMessageKey, FormatDirectionText(westSidePatch, oPackToActivate.DisplayName));
                                }

                            }
                            //
                            //  add northside exit
                            //
                            JoinMaps(iGridId, iGridId - 1, northSidePatch);
                            //
                            //  check for southside exit
                            //
                            if (MapGrid.ContainsKey(iGridId + 1))
                            {
                                JoinMaps(iGridId, iGridId + 1, southSidePatch);
                            }
                            //
                            //  check for westside exit
                            //
                            if (MapGrid.ContainsKey(iGridId + 3))
                            {
                                JoinMaps(iGridId, iGridId + 3, westSidePatch);
                            }
                            break;
                        case 3:
                        case 6:
                        case 9:
                            //
                            // bottom row
                            //
                            //
                            //  remove patch block to the right
                            //
                            iPatchIndex = iGridId == 3 ? -1 : iGridId - 3;
                            string sBottomRightKey = iPatchIndex == -1 ? "Farm" : MapGrid[iPatchIndex];

                            GameLocation glBottomRight = Game1.getLocationFromName(sBottomRightKey);
                            EntrancePatch oBottomRightPatch;
                            ExpansionPack oBottomRightPack = null;

                            if (iPatchIndex == -1)
                            {
                                oFarm = GetFarmDetails(Game1.whichFarm);
                                oBottomRightPatch = oFarm?.PathPatches["1"];
                            }
                            else
                            {
                                oBottomRightPack = modDataService.validContents[MapGrid[iPatchIndex]];
                                oBottomRightPatch = oBottomRightPack.EntrancePatches[westSidePatch];
                            }

                            if (oBottomRightPatch != null && (iPatchIndex == -1 && utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit2) || iPatchIndex > -1)
                            {
                                RemovePathBlock(new Vector2(oBottomRightPatch.PathBlockX, oBottomRightPatch.PathBlockY), glBottomRight, oBottomRightPatch.WarpOrientation, oBottomRightPatch.WarpOut.NumberofPoints);

                                //
                                //    add basic right/left side warp ins/outs
                                //
                                oExpPatch = oNewPack.EntrancePatches[eastSidePatch];
                                AddExpansionIntersection(oBottomRightPatch, sBottomRightKey, oExpPatch, sExpansionName);
                                if (oExpPatch.Sign?.UseSign ?? false)
                                {
                                    AddSignPost(oExp, oExpPatch, "EastMessage", FormatDirectionText(eastSidePatch, (iPatchIndex == -1 ? "Farm" : oBottomRightPack?.DisplayName ?? "")));
                                }
                                if (oBottomRightPatch.Sign?.UseSign ?? false)
                                {
                                    AddSignPost(Game1.getLocationFromName(sBottomRightKey), oBottomRightPatch, "WestMessage", FormatDirectionText(westSidePatch, oPackToActivate.DisplayName));
                                }
                            }
                            //
                            //  add northside patch
                            //
                            JoinMaps(iGridId, iGridId - 1, northSidePatch);
                            //
                            //  check for westside exit
                            //
                            if (MapGrid.ContainsKey(iGridId + 3))
                            {
                                JoinMaps(iGridId, iGridId + 3, westSidePatch);
                            }
                            break;
                    }

                }
                else if (!bAutoAdd)
                {
                    //
                    //  expansion has explicit location details
                    //
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error patching in map {iGridId}:{sExpansionName}", LogLevel.Error);
                logger.LogError($"PatchInMap", ex);
            }
        }

        #endregion
        /// <summary>
        /// Gets the world map location for an expansion in the Auto Grid
        /// </summary>
        /// <param name="sExpansionName"></param>
        /// <returns>A bounding Rectangle with the map placement for the specified expansion</returns>mm
        internal override Rectangle GetExpansionWorldMapLocation(string sExpansionName)
        {
            List<KeyValuePair<int, string>> gridEntry = MapGrid.Where(p => p.Value == sExpansionName).ToList();
            if (gridEntry.Any())
            {
                return GetExpansionWorldMapLocation(gridEntry.First().Key);
            }
            else
            {
                return Rectangle.Empty;
            }
        }
        internal override Rectangle GetExpansionMainMapLocation(int iGridId, int left, int top)
        {
            int iCol = (iGridId - 1) / 3;
            int iRow = (iGridId - 1) % 3;

            int iPadding = 1;
            int iBoxWidth = 20;
            int iBoxHeight = 10;
            int iImageWidth = iBoxWidth - iPadding * 2;
            int iImageHeight = iBoxHeight - iPadding * 2;

            switch (iGridId)
            {
                case 0: //by the Backwoods
                    return new Rectangle(left + iPadding + 70, top + iPadding - 5, iImageWidth, iImageHeight);
                //case 1:
                //    return new Rectangle(63, 64 + iMapIndex * 14, iImageWidth, iImageHeight);
                default:
                    return new Rectangle(left + (3 - iCol) * iBoxWidth + iPadding - 10, top + iRow * iBoxHeight + iPadding - 5, iImageWidth, iImageHeight);
            }
        }
        /// <summary>
        /// Gets the world map location for an expansion in the Auto Grid
        /// </summary>
        /// <param name="iGridId"></param>
        /// <returns>A bounding Rectangle with the map placement for the specified expansion</returns>mm
        internal override Rectangle GetExpansionWorldMapLocation(int iGridId)
        {
            int iRow = (iGridId - 1) / 3;
            int iCol = (iGridId - 1) % 3;

            int iPadding = 4;
            int iBoxWidth = 64;
            int iBoxHeight = 44;
            int iImageWidth = iBoxWidth - iPadding * 2;
            int iImageHeight = iBoxHeight - iPadding * 2;
            int iTop = 5;
            int iLeft = 210;

            switch (iGridId)
            {
                case 0: //by the Backwoods
                    return new Rectangle(iLeft + iPadding, iTop + iPadding, iImageWidth, iImageHeight);
                //case 1:
                //    return new Rectangle(63, 64 + iMapIndex * 14, iImageWidth, iImageHeight);
                default:
                    return new Rectangle(iLeft - iRow * iBoxWidth - iBoxWidth + iPadding, iTop + iCol * iBoxHeight + iPadding, iImageWidth, iImageHeight);
            }

        }

        internal override int AddMapToGrid(string expansionName, int gridId)
        {
            //
            //  adds an expansion map to the
            //  farm grid
            //
            //  returns the gridid on the 
            //  newly added expansion
            //
            int returnId = -1;
            if (modDataService.farmExpansions.TryGetValue(expansionName, out FarmExpansionLocation oBase))
            {
                try
                {
                    if (gridId > -1)
                    {
                        //addition from MP Server, not always, called  by load
                        //
                        if (MapGrid.Where(p => p.Value == expansionName).Any())
                        {
                            int localId = MapGrid.Where(p => p.Value == expansionName).First().Key;
                            if (localId == gridId)
                            {
                                //
                                //  already added, return existing id
                                //
                                // skip caves, assume they were added during first additon
                                return gridId;
                            }
                            else
                            {
                                // duplicate add, should not happen
                                // might need extra check for multiplayer
                                // skip caves, assume they were added during first additon
                                return localId;
                            }
                        }
                        else
                        {
                            //  expansion key not found, supplied id
                            //  is invalid
                            if (!MapGrid.ContainsKey(gridId))
                                returnId = gridId;
                            else
                                returnId = GetMapGridId();
                        }
                        if (!MapGrid.TryAdd(returnId, expansionName))
                        {
                            returnId = -1;
                            //  still screwed up, fix it up
                            //
                            //  try and fix it
                            //
                            //var addedExpansion = MapGrid.Where(p => p.Value == expansionName);
                            //if (addedExpansion.Any())
                            //{
                            //    if (addedExpansion.Count() > 1)
                            //        logger.Log($"Multiple instaces {addedExpansion.Count()} of '{expansionName}'", LogLevel.Warn);
                            //    else
                            //    {
                            //        if (modDataService.farmExpansions.TryGetValue(addedExpansion.First().Value, out var expansion))
                            //        {
                            //            if (expansion.GridId != gridId)
                            //            {
                            //                logger.Log($"Expansion mapped to different gridId: {expansion.GridId}", LogLevel.Warn);
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                    else
                    {
                        if (MapGrid.Where(p => p.Value == expansionName).Any())
                        {
                            //
                            //  already added, return existing id
                            //  skip caves addition
                            return MapGrid.Where(p => p.Value == expansionName).First().Key;
                        }
                        else
                        {
                            //
                            //  verify grid availability
                            //
                            if (oBase.GridId > -1)
                            {
                                if (MapGrid.ContainsKey(oBase.GridId))
                                {
                                    //
                                    //  conflict grid allocations, should not happen
                                    //
                                    oBase.GridId = GetMapGridId();
                                    if (oBase.GridId > -1)
                                    {
                                        if (MapGrid.TryAdd(oBase.GridId, expansionName))
                                            returnId = oBase.GridId;
                                    }
                                }
                                else
                                {
                                    if (MapGrid.TryAdd(oBase.GridId, expansionName))
                                        returnId = oBase.GridId;
                                }
                            }
                            else
                            {
                                oBase.GridId = GetMapGridId();
                                if (oBase.GridId > -1)
                                {
                                    if (MapGrid.TryAdd(oBase.GridId, expansionName))
                                        returnId = oBase.GridId;
                                }
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    logger.LogError("AddMapToGrid", ex);
                    logger.Log($"Location Name='{expansionName}', gridId={gridId}, returnId={returnId}", LogLevel.Debug);
                    returnId = -1;
                }
                if (returnId > -1)
                {
                    //
                    //  add cave details
                    //
                    //
                    //  check for Cave entrance
                    //             
                    ExpansionPack oPackToActivate = contentPackService.contentPackLoader.ValidContents[expansionName];

                    if (oPackToActivate.CaveEntrance != null && oPackToActivate.CaveEntrance.WarpIn != null && oPackToActivate.CaveEntrance.WarpOut != null)
                    {
                        modDataService.farmExpansions[expansionName].warps.Add(new Warp(oPackToActivate.CaveEntrance.WarpOut.X, oPackToActivate.CaveEntrance.WarpOut.Y, WarproomManager.WarpRoomLoacationName, (int)WarproomManager.WarpRoomEntrancePoint.X, (int)WarproomManager.WarpRoomEntrancePoint.Y, false));
                        utilitiesService.CustomEventsService.TriggerCustomEvent("AddCaveEntrance", new object[] { expansionName, Tuple.Create(oPackToActivate.DisplayName, oPackToActivate.CaveEntrance) });
                        //warproomService.warproomManager.AddCaveEntrance(sExpName, Tuple.Create(oPackToActivate.DisplayName, oPackToActivate.CaveEntrance));
                        //CaveEntrances.Add(sExpName, Tuple.Create(oPackToActivate.DisplayName, oPackToActivate.CaveEntrance));
                    }
                    //
                    //  update grid id in moddata
                    //
                    oBase.modData[IModDataKeysService.FEGridId] = returnId.ToString();
                }
            }
            return returnId;
        }
        private int GetMapGridId()
        {
            for (int i = 0; i < 10; i++)
            {
                if (!IsMapGridOccupied(i))
                {
                    return i;
                }
            }

#if DEBUG_LOG
            logger.LogDebug($"GetMapGridId: MapGrid is full.");
#endif
            return -1;
        }
        /// <summary>
        /// Formats sign post text with appropriate direction icon (if available)
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="text"></param>
        /// <returns>String with direction symbol and sign text</returns>
        private string FormatDirectionText(string direction, string text)
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
        //internal override void AddSign(ExpansionPack oPack)
        //{
        //    //
        //    //  need to update logic
        //    //
        //    return;


        //    PatchingDetails oPatch = oPack.InsertDetails[oPack.ActiveRule];

        //    foreach (MapEdit oEdit in oPatch.MapEdits)
        //    {
        //        if (oEdit.SignPostLocation != Point.Zero && !string.IsNullOrEmpty(oEdit.SignLocationName))
        //        {
        //            GameLocation gl = Game1.getLocationFromName(oEdit.SignLocationName);
        //            if (gl == null)
        //            {
        //                logger?.Log($"Invalid sign location '{oEdit.SignLocationName}'", LogLevel.Debug);
        //            }
        //            else
        //            {
        //                Vector2 oVector = new Vector2(oEdit.SignPostLocation.X, oEdit.SignPostLocation.Y);
        //                if (gl.objects.ContainsKey(oVector))
        //                {
        //                    gl.objects.Remove(oVector);
        //                }
        //                FESign oSign = new FESign(oVector, oPack.DisplayName);
        //                gl.objects.Add(oVector, oSign);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Add a Sign Post to the map with the name of the expansion the exit leads to
        /// </summary>
        /// <param name="oExp"></param>
        /// <param name="oExpPatch"></param>
        /// <param name="messageKeySuffix"></param>
        /// <param name="signMessage"></param>
        internal void AddSignPost(GameLocation oExp, EntrancePatch oExpPatch, string messageKeySuffix, string signMessage)
        {
            //
            //  the sign post can either be on the building layer to block,
            //  or added to the Front of an existing map with a building layer tile
            //
            try
            {
                string layerName = oExpPatch.Sign.UseFront ? "Front" : "Buildings";
                int iTileSheetId = mapUtilities.GetTileSheetId(oExp.map, "*_outdoorsTileSheet");
                mapUtilities.SetTile(oExp.map, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y, iTileSheetId, 435, layerName);
                //
                //  add name of expansion the the MapStrings
                //
                string messageKey = oExp.Name + "." + messageKeySuffix;

                contentManager.UpsertStringFromMap(messageKey, signMessage);
                //ContentHandler?.UpsertStringFromMap(messageKey, signMessage);
                //
                //  add tile action to view the message
                //
                mapUtilities.SetTileProperty(oExp, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y, "Buildings", "Action", "Message \"" + messageKey + "\"");
                mapUtilities.SetTile(oExp.map, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y - 1, iTileSheetId, 410, "Front");
            }
            catch (Exception ex)
            {
                logger.Log($"Error AddingSignPost. {ex}", LogLevel.Error);
            }
        }

        /// <summary>
        ///  Joins the entrance/exit warps of two maps
        ///  removes the path blocks for the entrance from the 2 maps
        /// </summary>
        /// <param name="oExitPatch"></param>
        /// <param name="sLocationNameA"></param>
        /// <param name="oEntrancePath"></param>
        /// <param name="sLocationNameB"></param>
        private void AddExpansionIntersection(EntrancePatch oExitPatch, string sLocationNameA, EntrancePatch oEntrancePath, string sLocationNameB)
        {
            //
            //  get Gameloactions
            //
            GameLocation glA = Game1.getLocationFromName(sLocationNameA);
            if (glA is null)
            {
                logger.Log($"AddExpansionIntersection could not location: {sLocationNameA}", LogLevel.Debug);
                return;
            }
            GameLocation glB = Game1.getLocationFromName(sLocationNameB);
            if (glB is null)
            {
                logger.Log($"AddExpansionIntersection could not location: {sLocationNameB}", LogLevel.Debug);
                return;
            }
            //
            //  remove path blocks from each expansion
            //
            RemovePathBlock(new Vector2(oExitPatch.PathBlockX, oExitPatch.PathBlockY), glA, oExitPatch.WarpOrientation, oExitPatch.WarpOut.NumberofPoints);
            RemovePathBlock(new Vector2(oEntrancePath.PathBlockX, oEntrancePath.PathBlockY), glB, oEntrancePath.WarpOrientation, oEntrancePath.WarpOut.NumberofPoints);
            //
            //  add the warps between the expansions
            //
            AddExpansionWarps(oExitPatch, glA, oEntrancePath, glB);
        }
        /// <summary>
        /// add warps between 2 expansions
        /// Compensates for mis-matches in the size of each destination
        /// </summary>
        /// <param name="oExitPatch"></param>
        /// <param name="glA"></param>
        /// <param name="oEntrancePath"></param>
        /// <param name="glB"></param>
        private void AddExpansionWarps(EntrancePatch oExitPatch, GameLocation glA, EntrancePatch oEntrancePath, GameLocation glB)
        {
            int numWarps = Math.Max(oExitPatch.WarpOut.NumberofPoints, oEntrancePath.WarpIn.NumberofPoints);

#if DEBUG
            logger.Log($"       AddExpansionWarps. {glA.NameOrUniqueName} to {glB.NameOrUniqueName}", LogLevel.Debug);
            logger.Log($"       numwarps: {numWarps}", LogLevel.Debug);
            logger.Log($"       exit points: {oExitPatch.WarpOut.NumberofPoints} {oExitPatch.WarpOrientation}", LogLevel.Debug);
            logger.Log($"   entrance points: {oEntrancePath.WarpIn.NumberofPoints} {oEntrancePath.WarpOrientation}", LogLevel.Debug);
#endif

            for (int iWarp = 0; iWarp < numWarps; iWarp++)
            {
                try
                {
                    //
                    //  check for patch point underflows
                    //
                    int iExitCell = iWarp < oExitPatch.WarpOut.NumberofPoints ? iWarp : oExitPatch.WarpOut.NumberofPoints - 1;
                    int iEntCell = iWarp < oEntrancePath.WarpIn.NumberofPoints - 1 ? iWarp : oEntrancePath.WarpIn.NumberofPoints - 1;

                    if (oExitPatch.WarpOrientation == WarpOrientations.Horizontal)
                    {
                        //
                        //  remove existing warp to support swapping expansions
                        //
                        IEnumerable<Warp> existingWarp = glA.warps.Where(p => p.X == oExitPatch.WarpOut.X + iExitCell && p.Y == oExitPatch.WarpOut.Y);
                        if (existingWarp.Any())
                        {
                            glA.warps.Remove(glA.warps.Where(p => p.X == oExitPatch.WarpOut.X + iExitCell && p.Y == oExitPatch.WarpOut.Y).First());
                        }
                        glA.warps.Add(new Warp(oExitPatch.WarpOut.X + iExitCell, oExitPatch.WarpOut.Y, glB.Name, oEntrancePath.WarpIn.X + iEntCell, oEntrancePath.WarpIn.Y, false));
                        //
                        //  remove existing warp to support swapping expansions
                        //
                        existingWarp = glB.warps.Where(p => p.X == oEntrancePath.WarpOut.X + iExitCell && p.Y == oEntrancePath.WarpOut.Y);
                        if (existingWarp.Any())
                        {
                            glB.warps.Remove(glB.warps.Where(p => p.X == oEntrancePath.WarpOut.X + iExitCell && p.Y == oEntrancePath.WarpOut.Y).First());
                        }
                        glB.warps.Add(new Warp(oEntrancePath.WarpOut.X + iEntCell, oEntrancePath.WarpOut.Y, glA.Name, oExitPatch.WarpIn.X + iExitCell, oExitPatch.WarpIn.Y, false));
                    }
                    else
                    {
                        IEnumerable<Warp> existingWarp = glA.warps.Where(p => p.X == oExitPatch.WarpOut.X && p.Y == oExitPatch.WarpOut.Y + iExitCell);
                        if (existingWarp.Any())
                        {
                            glA.warps.Remove(glA.warps.Where(p => p.X == oExitPatch.WarpOut.X && p.Y == oExitPatch.WarpOut.Y + iExitCell).First());
                        }
                        glA.warps.Add(new Warp(oExitPatch.WarpOut.X, oExitPatch.WarpOut.Y + iExitCell, glB.Name, oEntrancePath.WarpIn.X, oEntrancePath.WarpIn.Y + iEntCell, false));
                        existingWarp = glB.warps.Where(p => p.X == oEntrancePath.WarpOut.X && p.Y == oEntrancePath.WarpOut.Y + iEntCell);
                        if (existingWarp.Any())
                        {
                            glB.warps.Remove(glB.warps.Where(p => p.X == oEntrancePath.WarpOut.X && p.Y == oEntrancePath.WarpOut.Y + iEntCell).First());
                        }
                        glB.warps.Add(new Warp(oEntrancePath.WarpOut.X, oEntrancePath.WarpOut.Y + iEntCell, glA.Name, oExitPatch.WarpIn.X, oExitPatch.WarpIn.Y + iExitCell, false));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"AddExpansionWarps", ex);
                }
            }
        }

        /// <summary>
        /// Get the details of the currently selected FarmType
        /// </summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        private FarmDetails GetFarmDetails(int farmId)
        {
            IEnumerable<FarmDetails> olist = gameEnvironmentService.GameFarms?.Where(p => p.FarmType == farmId);

            if (olist != null && olist.Any())
            {
                return olist.First();
            }

            return null;
        }
        private void RemovePathBlock(Vector2 oBlockPos, GameLocation gl, WarpOrientations eBlockOrientation, int iBlockWidth)
        {
#if DEBUG_LOG
            logger.Log($"   Removing path block {gl.Name} {oBlockPos}, {eBlockOrientation} for {iBlockWidth}", LogLevel.Debug);
#endif

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
                logger.LogError($"GridManager.RemovePathBlock", ex); ;
            }
        }
        private void RemoveTile(GameLocation gl, int x, int y, string sLayer, int tileIndex)
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
        private void RemoveTile(GameLocation gl, int x, int y, string sLayer)
        {
            RemoveTile(gl, x, y, sLayer, -1);
        }
        private void JoinMaps(int sourceGrid, int targetGrid, string sourceSide)
        {
            //
            //  Join intersecting expansions
            //
            //  - remove path blocks
            //  - add warps
            //  - add signs
            //
            string targetSide = GetNeighbourSide(sourceSide);
            string sourceMessageName = GetSignPostMessagePrefix(sourceSide);
            string targetMessageName = GetSignPostMessagePrefix(targetSide);

            ExpansionPack targetPack = modDataService.validContents[MapGrid[targetGrid]];
            EntrancePatch targetEntPatch = targetPack.EntrancePatches[targetSide];

            ExpansionPack sourcePack = modDataService.validContents[MapGrid[sourceGrid]];
            EntrancePatch sourceEntPatch = sourcePack.EntrancePatches[sourceSide];

            AddExpansionIntersection(sourceEntPatch, sourcePack.LocationName, targetEntPatch, targetPack.LocationName);

            if (sourceEntPatch.Sign?.UseSign ?? false)
            {
                AddSignPost(modDataService.farmExpansions[sourcePack.LocationName], sourceEntPatch, sourceMessageName, FormatDirectionText(sourceSide, targetPack.DisplayName));
            }
            if (targetEntPatch.Sign?.UseSign ?? false)
            {
                AddSignPost(modDataService.farmExpansions[targetPack.LocationName], targetEntPatch, targetMessageName, FormatDirectionText(targetSide, sourcePack.DisplayName));
            }

        }
        private string GetSignPostMessagePrefix(EntranceDirection side)
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
        private string GetSignPostMessagePrefix(string side)
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
        private static string GetNeighbourSide(EntranceDirection side)
        {
            return side switch
            {
                EntranceDirection.West => eastSidePatch,
                EntranceDirection.East => westSidePatch,
                EntranceDirection.North => southSidePatch,
                EntranceDirection.South => northSidePatch,
                _ => ""
            };
        }
        private string GetNeighbourSide(string side)
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
        private bool IsMapGridOccupied(int iGridId)
        {
            return MapGrid.ContainsKey(iGridId);
        }
        internal override string GetNeighbourExpansionName(int iGridId, EntranceDirection side)
        {
            //
            //  returns the neighbouring expasion in the direction supplied
            //
            int nGridId;
            switch (side)
            {
                case EntranceDirection.North:
                    //  top row has no northern neighbour
                    if (iGridId == 0 || (iGridId - 1) % 3 == 0) return null;
                    nGridId = iGridId - 1;
                    if (MapGrid.ContainsKey(nGridId))
                        return MapGrid[nGridId];
                    break;
                case EntranceDirection.East:
                    nGridId = iGridId == 1 ? 0 : iGridId - 3;
                    if (MapGrid.ContainsKey(nGridId))
                        return MapGrid[nGridId];
                    break;
                case EntranceDirection.South:
                    //  bottom row has no southern neighbour
                    if (iGridId == 0 || iGridId % 3 == 0) return null;
                    nGridId = iGridId + 1;
                    if (MapGrid.ContainsKey(nGridId))
                        return MapGrid[nGridId];
                    break;
                case EntranceDirection.West:
                    nGridId = iGridId == 0 ? 1 : iGridId + 3;
                    if (MapGrid.ContainsKey(nGridId))
                        return MapGrid[nGridId];
                    break;

            }

            return null;
        }


        private void AddAllMapExitBlockers(string sExpansionName)
        {
            //
            //  An Expansion should have an exit on each of the 4 map sides
            //  add path blocks to each of the 4 exits
            //
            GameLocation glNewExp = Game1.getLocationFromName(sExpansionName);
            int iGridId = modDataService.farmExpansions[sExpansionName].GridId;

            foreach (EntrancePatch oPatch in modDataService.validContents[sExpansionName].EntrancePatches.Values)
            {
                //if ((oPatch.PatchSide != EntranceDirection.East) || (iGridId == 2 && !ActiveFarmProfile.UseFarmExit1) || (iGridId == 3 && !ActiveFarmProfile.UseFarmExit2))
                //{
                utilitiesService.MapUtilities.AddPathBlock(new Vector2(oPatch.PathBlockX, oPatch.PathBlockY), glNewExp.map, oPatch.WarpOrientation, oPatch.WarpOut.NumberofPoints);

                //
                // check for neighbour
                //
                if ((iGridId == 2 || iGridId == 3) && oPatch.PatchSide == EntranceDirection.East)
                {
                    //
                    //  check for farm patch
                }
                else
                {
                    EntrancePatch patchNeighbour = null;
                    string neighbour = GetNeighbourExpansionName(iGridId, oPatch.PatchSide);
                    GameLocation glNeighbour = null;
                    if (!string.IsNullOrEmpty(neighbour))
                    {
                        string opside = GetNeighbourSide(oPatch.PatchSide);
                        patchNeighbour = modDataService.validContents[neighbour].EntrancePatches[opside];
                        glNeighbour = Game1.getLocationFromName(neighbour);
                    }
                    //
                    //
                    //  remove any sign in patch side
                    //
                    if (oPatch.Sign?.UseSign ?? false)
                    {
                        //
                        //  need to remove local sign post map pieces
                        //  and remove action tile properties
                        RemoveSignPost(glNewExp, oPatch, GetSignPostMessagePrefix(oPatch.PatchSide));
                        //
                        //  remove side neighbours sign
                        //
                        if (patchNeighbour != null)
                        {
                            RemoveSignPost(glNeighbour, patchNeighbour, GetSignPostMessagePrefix(patchNeighbour.PatchSide));
                        }
                    }
                    //
                    //  remove local warps
                    //
                    utilitiesService.MapUtilities.RemovePatchWarps(oPatch, glNewExp);
                    if (patchNeighbour != null)
                    {
                        //
                        //  need to remove warps
                        //  from neighbouring expansions
                        //
                        utilitiesService.MapUtilities.RemovePatchWarps(patchNeighbour, glNeighbour);
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
        private void RemoveSignPost(GameLocation oExp, EntrancePatch oExpPatch, string messageKeySuffix)
        {
            if (oExpPatch.Sign != null)
            {
                string layerName = oExpPatch.Sign.UseFront ? "Front" : "Buildings";
                RemoveTile(oExp, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y, layerName);
                string messageKey = oExp.Name + "." + messageKeySuffix;

                contentManager.RemoveStringFromMap(messageKey);
                //ContentHandler.RemoveStringFromMap(messageKey);
                mapUtilities.RemoveTileProperty(oExp, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y, "Buildings", "Action");
                RemoveTile(oExp, oExpPatch.Sign.Position.X, oExpPatch.Sign.Position.Y - 1, "Front");
            }
        }
        internal override void FixGrid()
        {
            //
            //  called after save to fix any holes in the grid
            //
            for (int iCounter = 0; iCounter < 10; iCounter++)
            {
                if (!MapGrid.ContainsKey(iCounter))
                {
                    bool found = false;
                    for (int replace = iCounter + 1; replace < 10; replace++)
                    {
                        if (MapGrid.ContainsKey(replace))
                        {
                            found = true;
                            MapGrid[iCounter] = MapGrid[replace];
                            modDataService.farmExpansions[MapGrid[iCounter]].modData.Remove("prism99.advize.stardewrealty.FEGridId");
                            modDataService.farmExpansions[MapGrid[iCounter]].modData.Add("prism99.advize.stardewrealty.FEGridId", iCounter.ToString());
                            modDataService.farmExpansions[MapGrid[iCounter]].GridId = iCounter;
                            MapGrid.Remove(replace);
                            break;
                        }
                    }
                    if (!found)
                        break;
                }
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
