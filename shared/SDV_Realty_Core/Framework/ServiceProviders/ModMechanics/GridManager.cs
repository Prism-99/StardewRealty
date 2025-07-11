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
using CustomMenuFramework.Menus;
using xTile;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceProviders.ModData;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    /// <summary>
    /// Version 1.6
    /// </summary>
    internal class GridManager : IGridManager
    {


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
            this.logger = logger;
            modDataService = (IModDataService)args[0];
            mapUtilities = (IMapUtilities)args[1];
            contentManager = (IContentManagerService)args[2];
            contentPackService = (IContentPackService)args[3];
            utilitiesService = (IUtilitiesService)args[4];
            modHelperService = (IModHelperService)args[5];
            mineCartMenuService = (IMineCartMenuService)args[6];
            gameEnvironmentService = (IGameEnvironmentService)args[7];

            GameLocation.RegisterTouchAction(ModKeyStrings.TAction_PickDestination, HandlePickDestination);
        }
        #region "Public Methods"

        public override void AddMapExitBlockers(string expansionName)
        {
            int gridId = GetLocationGridId(expansionName);

            foreach (EntrancePatch patch in modDataService.validContents[expansionName].EntrancePatches.Values)
            {
                if ((patch.PatchSide != EntranceDirection.East) || (gridId == 2 && !utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit1) || (gridId == 3 && !utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit2))
                {
                    GameLocation targetLocation;
                    if (string.IsNullOrEmpty(patch.WarpOut.LocationName))
                    {
                        targetLocation = Game1.getLocationFromName(expansionName);
                    }
                    else
                    {
                        targetLocation = Game1.getLocationFromName(patch.WarpOut.LocationName);
                    }
                    utilitiesService.MapUtilities.AddPathBlock(new Vector2(patch.PathBlockX, patch.PathBlockY), targetLocation.map, patch.WarpOrientation, patch.WarpOut.NumberofPoints);
                }
            }
        }
        public override void AddMapExitBlockers(int iGridId)
        {
            //
            //  An Expansion should have an exit on each of the 4 map sides
            //  add path blocks to each of the 4 exits
            //
            string expansionName = modDataService.MapGrid[iGridId];
            AddMapExitBlockers(expansionName);
        }
        internal override string? GetNeighbourExpansionTTId(int gridId, EntranceDirection side)
        {
            return gridId switch
            {
                0 => side switch
                {
                    EntranceDirection.West => "1/Default",
                    EntranceDirection.South => "-1/Default",
                    _ => ""
                },
                1 => side switch
                {
                    EntranceDirection.West => "4/Default",
                    EntranceDirection.East => "0/Default",
                    EntranceDirection.South => "2/Default",
                    _ => ""
                },
                2 => side switch
                {
                    EntranceDirection.North => "1/Default",
                    EntranceDirection.East => "-3/Default",
                    EntranceDirection.South => "3/Default",
                    EntranceDirection.West => gridId + 3 < modDataService.MaximumExpansions ? "5/Default" :"",
                    _ => ""
                },
                3 => side switch
                {
                    EntranceDirection.North => "2/Default",
                    EntranceDirection.East => "-5/Default",
                    EntranceDirection.West => gridId + 3 < modDataService.MaximumExpansions ? "6/Default" :"",
                    _ => ""
                },
                _ => new Func<string>(() =>
                {
                    if ((gridId - 1) % 3 == 0)
                    {
                        return side switch
                        {
                            EntranceDirection.West =>gridId+3< modDataService.MaximumExpansions? $"{gridId + 3}/Default":"",
                            EntranceDirection.East => $"{gridId - 3}/Default",
                            EntranceDirection.South => gridId + 1 < modDataService.MaximumExpansions ? $"{gridId + 1}/Default":"",
                            _ => ""
                        };
                    }
                    else if (gridId % 3 == 0)
                    {
                        return side switch
                        {
                            EntranceDirection.West => gridId + 3 < modDataService.MaximumExpansions ? $"{gridId + 3}/Default":"",
                            EntranceDirection.East => $"{gridId - 3}/Default",
                            EntranceDirection.North => gridId + 1 < modDataService.MaximumExpansions ? $"{gridId - 1}/Default":"",
                            _ => ""
                        };
                    }
                    return side switch
                    {
                        EntranceDirection.West => gridId + 3 < modDataService.MaximumExpansions ? $"{gridId + 3}/Default":"",
                        EntranceDirection.East => $"{gridId - 3}/Default",
                        EntranceDirection.North => $"{gridId - 1}/Default",
                        EntranceDirection.South => gridId + 1 < modDataService.MaximumExpansions ? $"{gridId + 1}/Default":"",
                        _=>""
                    };

                })()
            };
            //string? expansionName = GetNeighbourExpansionName(gridId, side);
            //if (expansionName == null)
            //    return null;
            //return $"{expansionName}/{expansionName}.tt";
            //return $"{modDataService.validContents[expName].Owner.Manifest.UniqueID}.{expName}/{modDataService.validContents[expName].Owner.Manifest.UniqueID}.{expName}.tt";
            //return $"{expName}.tt";
            //return $"{expName}.tt";
        }
        /// <summary>
        /// Swap Farm Expansions grid locations
        /// </summary>
        /// <param name="gridIdA">Source Grid Id</param>
        /// <param name="gridIdB">Target Grid Id</param>
        /// <returns></returns>
        internal override string SwapGridLocations(int gridIdA, int gridIdB)
        {
            string result = "";

            if (!IsMapGridOccupied(gridIdA))
            {
                result = $"{gridIdA} is not occupied.";
            }
            else if (!IsMapGridOccupied(gridIdB))
            {
                result = $"{gridIdB} is not occupied.";
            }
            else
            {
                string farmA = modDataService.MapGrid[gridIdA];
                string farmB = modDataService.MapGrid[gridIdB];
                //
                //  re-block each map
                //
                AddAllMapExitBlockers(farmA);
                AddAllMapExitBlockers(farmB);
                //
                //  update modData
                //
                modDataService.farmExpansions[farmA].GridId = gridIdB;
                modDataService.farmExpansions[farmA].modData[IModDataKeysService.FEGridId] = gridIdB.ToString();
                modDataService.farmExpansions[farmB].GridId = gridIdA;
                modDataService.farmExpansions[farmB].modData[IModDataKeysService.FEGridId] = gridIdA.ToString();
                //
                //  swap references
                //
                modDataService.MapGrid[gridIdA] = farmB;
                modDataService.MapGrid[gridIdB] = farmA;
                //
                //  re-patch maps
                //
                PatchInMap(gridIdA);
                PatchInMap(gridIdB);
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
        /// <summary>
        /// Patch in a Farm Expansion map
        /// -remove required exit blockers
        /// -add required warps between Farm Expansion
        /// </summary>
        /// <param name="targetGridId">Grid Id to patch in</param>
        internal override void PatchInMap(int targetGridId)
        {
            string expansionName = modDataService.MapGrid[targetGridId];
#if DEBUG_LOG
            logger.Log($"     PatchInMap {targetGridId}:{expansionName}", LogLevel.Debug);
#endif
            try
            {
                GameLocation expansionToAdd = modDataService.farmExpansions[expansionName];
                bool autoAdd = modDataService.farmExpansions[expansionName].AutoAdd;
                ExpansionPack expansionPackToAdd = modDataService.validContents[expansionName];

                if (targetGridId > -1)
                {
                    //
                    //  based upon the grid location, remove path blocks and add warps
                    //  to join new Expansion to its neighbours
                    //
                    FarmDetails farmDetails = null;
                    EntrancePatch entrancePatch;
                    int patchIndex;

                    switch (targetGridId)
                    {
                        case 0:
                            //
                            //  initial grid spot
                            //  linked off of the Backwoods
                            //
                            farmDetails = GetFarmDetails(999999);

                            if (farmDetails != null)
                            {
                                GameLocation glBackwoods = Game1.getLocationFromName(farmDetails.MapName);
                                EntrancePatch backWoodsPatch = farmDetails.PathPatches[((int)EntranceDirection.North).ToString()];
                                //RemovePathBlock(new Vector2(oFarm.PathPatches["0"].PathBlockX, oFarm.PathPatches["0"].PathBlockY), glBackwoods, oBackWoodsPatch.WarpOrientation, oBackWoodsPatch.WarpOut.NumberofPoints);
                                //
                                //    add basic left side warp ins/outs
                                //
                                entrancePatch = expansionPackToAdd.EntrancePatches[((int)EntranceDirection.East).ToString()];
                                AddExpansionIntersection(backWoodsPatch, farmDetails.MapName, entrancePatch, expansionName);

                                if (entrancePatch.Sign?.UseSign ?? false)
                                {
                                    AddSignPost(expansionToAdd, entrancePatch, "EastMessage", FormatDirectionText(EntranceDirection.East, glBackwoods.Name));
                                }
                                if (modDataService.MapGrid.ContainsKey(1))
                                {
                                    JoinMaps(targetGridId, targetGridId + 1, EntranceDirection.West);
                                }
                            }
                            break;
                        case 1:
                        case 4:
                        case 7:
                        case 10:
                            //
                            //  top row 
                            //
                            //
                            //  get expansion to right details
                            //
                            int rightGridId = targetGridId == 1 ? 0 : targetGridId - 3;

                            JoinMaps(targetGridId, rightGridId, EntranceDirection.East);

                            //
                            //  check for other neighbours from swapping
                            //
                            //  check for neighbour to the south
                            //
                            if (modDataService.MapGrid.ContainsKey(targetGridId + 1))
                            {
                                JoinMaps(targetGridId, targetGridId + 1, EntranceDirection.South);
                            }
                            //
                            //  check for neighbour to the west
                            //
                            if (modDataService.MapGrid.ContainsKey(targetGridId + 3))
                            {
                                JoinMaps(targetGridId, targetGridId + 3, EntranceDirection.West);
                            }
                            break;
                        case 5:
                        case 8:
                        case 11:
                        case 2:
                            //
                            //  middle row
                            //
                            //  remove patch block to the right
                            //
                            patchIndex = targetGridId == 2 ? -1 : targetGridId - 3;
                            //string sRightKey = iPatchIndex == -1 ? "Farm" : MapGrid[iPatchIndex];
                            //string sMessageKey = iPatchIndex == -1 ? "WestMessage.1" : "WestMessage";

                            //GameLocation glMidRight = Game1.getLocationFromName(sRightKey);
                            EntrancePatch midRightPatch = null;
                            //ExpansionPack oMidRightPack = null;

                            if (patchIndex == -1)
                            {
                                farmDetails = GetFarmDetails(Game1.whichFarm);
                                if (utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit1)
                                {
                                    midRightPatch = farmDetails?.PathPatches[((int)EntranceDirection.North).ToString()];
                                    if (midRightPatch != null)
                                    {
                                        //RemovePathBlock(new Vector2(oMidRightPatch?.PathBlockX ?? 0, oMidRightPatch?.PathBlockY ?? 0), glMidRight, oMidRightPatch?.WarpOrientation ?? 0, oMidRightPatch?.WarpOut.NumberofPoints ?? 0);

                                        //
                                        //    add basic right/left side warp ins/outs
                                        //
                                        entrancePatch = expansionPackToAdd.EntrancePatches[((int)EntranceDirection.East).ToString()];
                                        if (midRightPatch != null)
                                            AddExpansionIntersection(midRightPatch, "Farm", entrancePatch, expansionName);

                                        if (entrancePatch.Sign?.UseSign ?? false)
                                        {
                                            AddSignPost(expansionToAdd, entrancePatch, "EastMessage", FormatDirectionText(EntranceDirection.East, "Farm"));
                                        }
                                        if (midRightPatch.Sign != null && (midRightPatch.Sign?.UseSign ?? false))
                                        {
                                            AddSignPost(GetLocation("Farm"), midRightPatch, "WestMessage.1", FormatDirectionText(EntranceDirection.West, expansionPackToAdd.DisplayName));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //
                                //  check for eastside exit
                                //
                                if (modDataService.MapGrid.ContainsKey(targetGridId - 3))
                                {
                                    JoinMaps(targetGridId, targetGridId - 3, EntranceDirection.East);
                                }
                                //oMidRightPack = modDataService.validContents[MapGrid[iPatchIndex]];
                                //oMidRightPatch = oMidRightPack.EntrancePatches[westSidePatch];
                            }


                            //
                            //  add northside exit
                            //
                            JoinMaps(targetGridId, targetGridId - 1, EntranceDirection.North);
                            //
                            //  check for southside exit
                            //
                            if (modDataService.MapGrid.ContainsKey(targetGridId + 1))
                            {
                                JoinMaps(targetGridId, targetGridId + 1, EntranceDirection.South);
                            }
                            //
                            //  check for westside exit
                            //
                            if (modDataService.MapGrid.ContainsKey(targetGridId + 3))
                            {
                                JoinMaps(targetGridId, targetGridId + 3, EntranceDirection.West);
                            }
                            break;
                        case 3:
                        case 6:
                        case 9:
                        case 12:
                            //
                            // bottom row
                            //
                            //
                            //  remove patch block to the right
                            //
                            patchIndex = targetGridId == 3 ? -1 : targetGridId - 3;
                            string bottomRightKey = patchIndex == -1 ? "Farm" : modDataService.MapGrid[patchIndex];

                            GameLocation glBottomRight = GetLocation(bottomRightKey);
                            EntrancePatch bottomRightPatch;
                            ExpansionPack bottomRightPack = null;

                            if (patchIndex == -1)
                            {
                                farmDetails = GetFarmDetails(Game1.whichFarm);
                                bottomRightPatch = farmDetails?.PathPatches[((int)EntranceDirection.East).ToString()];
                                if (bottomRightPatch != null && utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit2)
                                {
                                    RemovePathBlock(new Vector2(bottomRightPatch.PathBlockX, bottomRightPatch.PathBlockY), glBottomRight, bottomRightPatch.WarpOrientation, bottomRightPatch.WarpOut.NumberofPoints);

                                    //
                                    //    add basic right/left side warp ins/outs
                                    //
                                    entrancePatch = expansionPackToAdd.EntrancePatches[((int)EntranceDirection.East).ToString()];
                                    AddExpansionIntersection(bottomRightPatch, bottomRightKey, entrancePatch, expansionName);
                                    if (entrancePatch.Sign?.UseSign ?? false)
                                    {
                                        AddSignPost(expansionToAdd, entrancePatch, "EastMessage", FormatDirectionText(EntranceDirection.East, (patchIndex == -1 ? "Farm" : bottomRightPack?.DisplayName ?? "")));
                                    }
                                    if (bottomRightPatch.Sign?.UseSign ?? false)
                                    {
                                        AddSignPost(Game1.getLocationFromName(bottomRightKey), bottomRightPatch, "WestMessage", FormatDirectionText(EntranceDirection.West, expansionPackToAdd.DisplayName));
                                    }
                                }
                            }
                            else
                            {
                                //oBottomRightPack = modDataService.validContents[MapGrid[iPatchIndex]];
                                //oBottomRightPatch = oBottomRightPack.EntrancePatches[westSidePatch];
                                JoinMaps(targetGridId, targetGridId - 3, EntranceDirection.East);
                            }

                            //if (oBottomRightPatch != null && (iPatchIndex == -1 && utilitiesService.GameEnvironment.ActiveFarmProfile.UseFarmExit2) || iPatchIndex > -1)
                            //{
                            //    RemovePathBlock(new Vector2(oBottomRightPatch.PathBlockX, oBottomRightPatch.PathBlockY), glBottomRight, oBottomRightPatch.WarpOrientation, oBottomRightPatch.WarpOut.NumberofPoints);

                            //    //
                            //    //    add basic right/left side warp ins/outs
                            //    //
                            //    oExpPatch = oNewPack.EntrancePatches[eastSidePatch];
                            //    AddExpansionIntersection(oBottomRightPatch, sBottomRightKey, oExpPatch, sExpansionName);
                            //    if (oExpPatch.Sign?.UseSign ?? false)
                            //    {
                            //        AddSignPost(oExp, oExpPatch, "EastMessage", FormatDirectionText(EntranceDirection.East, (iPatchIndex == -1 ? "Farm" : oBottomRightPack?.DisplayName ?? "")));
                            //    }
                            //    if (oBottomRightPatch.Sign?.UseSign ?? false)
                            //    {
                            //        AddSignPost(Game1.getLocationFromName(sBottomRightKey), oBottomRightPatch, "WestMessage", FormatDirectionText(EntranceDirection.West, oPackToActivate.DisplayName));
                            //    }
                            //}
                            //
                            //  add northside patch
                            //
                            JoinMaps(targetGridId, targetGridId - 1, EntranceDirection.North);
                            //
                            //  check for westside exit
                            //
                            if (modDataService.MapGrid.ContainsKey(targetGridId + 3))
                            {
                                JoinMaps(targetGridId, targetGridId + 3, EntranceDirection.West);
                            }
                            break;
                    }

                }
                else if (!autoAdd)
                {
                    //
                    //  expansion has explicit location details
                    //
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error patching in map {targetGridId}:{expansionName}", LogLevel.Error);
                logger.LogError($"PatchInMap", ex);
            }
        }

        #endregion

        private void HandlePickDestination(GameLocation location, string[] args, Farmer who, Vector2 position)
        {
            if (modDataService.TryGetExpansionPack(location.Name, out ExpansionPack expansionPack))

                if (TryGetLocationNeighbours(expansionPack.LocationName, out Neighbours neighbours))
                {
                    if (expansionPack.isAdditionalFarm)
                    {
                        if (neighbours.West != null)
                        {
                            List<KeyValuePair<string, string>> locations = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("west",neighbours.West.DisplayName),
                            new KeyValuePair<string, string>("north",neighbours.North.DisplayName)
                        };
                            GenericPickListMenu pickLocationResponse = new GenericPickListMenu();
                            pickLocationResponse.ShowPagedResponses(I18n.GridManagerSelectDestination(), locations, delegate (string value)
                            {
                                if (value == "west")
                                {
                                    Point warp = new Point(neighbours.West.GetEntrancePatch(EntranceDirection.East).WarpIn.X, neighbours.West.GetEntrancePatch(EntranceDirection.East).WarpIn.Y);
                                    Game1.warpFarmer(neighbours.West.LocationName, warp.X, warp.Y, false);
                                }
                                else
                                {
                                    Point warp = new Point(neighbours.North.GetEntrancePatch(EntranceDirection.South).WarpIn.X, neighbours.North.GetEntrancePatch(EntranceDirection.South).WarpIn.Y);
                                    Game1.warpFarmer(neighbours.North.LocationName, warp.X, warp.Y, false);
                                }

                            }, auto_select_single_choice: true);
                        }

                    }
                }

            //return true;
        }

        /// <summary>
        /// Gets the world map location for an expansion in the Auto Grid
        /// </summary>
        /// <param name="expansionName"></param>
        /// <returns>A bounding Rectangle with the map placement for the specified expansion</returns>mm
        internal override Rectangle GetExpansionWorldMapLocation(string expansionName)
        {
            //if (expansionName == "Home") 
            //{
            //    // return home square locaation
            //    //return new Rectangle(left - iRow * imageBoxWidth - imageBoxWidth + boxPadding, top + topMargin + iCol * imageBoxHeight + boxPadding, imageWidth, imageHeight);
            //    return GetExpansionWorldMapLocation(-1);
            //}

            List<KeyValuePair<int, string>> gridEntry = modDataService.MapGrid.Where(p => p.Value == expansionName).ToList();
            if (gridEntry.Any())
            {
                return GetExpansionWorldMapLocation(gridEntry.First().Key);
            }
            else
            {
                return Rectangle.Empty;
            }
        }
        //internal override Rectangle GetExpansionMainMapLocation(int iGridId, int left, int top)
        //{
        //    //return utilitiesService.GetGridLocationCoordinates(iGridId, new Rectangle(left, top, 765, 525));
        //    int iCol = (iGridId - 1) / 3;
        //    int iRow = (iGridId - 1) % 3;

        //    int iPadding = 1;
        //    int iBoxWidth = 20;
        //    int iBoxHeight = 10;
        //    int iImageWidth = iBoxWidth - iPadding * 2;
        //    int iImageHeight = iBoxHeight - iPadding * 2;

        //    switch (iGridId)
        //    {
        //        case 0: //by the Backwoods
        //            return new Rectangle(left + iPadding + 70, top + iPadding - 5, iImageWidth, iImageHeight);
        //        //case 1:
        //        //    return new Rectangle(63, 64 + iMapIndex * 14, iImageWidth, iImageHeight);
        //        default:
        //            return new Rectangle(left + (3 - iCol) * iBoxWidth + iPadding - 10, top + iRow * iBoxHeight + iPadding - 5, iImageWidth, iImageHeight);
        //    }
        //}
        /// <summary>
        /// Gets the world map location for an expansion in the Auto Grid
        /// </summary>
        /// <param name="iGridId"></param>
        /// <returns>A bounding Rectangle with the map placement for the specified expansion</returns>
        internal override Rectangle GetExpansionWorldMapLocation(int iGridId)
        {
            return utilitiesService.GetGridLocationCoordinates(iGridId, new Rectangle(0, -20, 300, 190));
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
                        if (modDataService.MapGrid.Where(p => p.Value == expansionName).Any())
                        {
                            int localId = modDataService.MapGrid.Where(p => p.Value == expansionName).First().Key;
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
                            if (!modDataService.MapGrid.ContainsKey(gridId))
                                returnId = gridId;
                            else
                                returnId = GetMapGridId();
                        }
                        if (!modDataService.MapGrid.TryAdd(returnId, expansionName))
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
                        if (modDataService.MapGrid.Where(p => p.Value == expansionName).Any())
                        {
                            //
                            //  already added, return existing id
                            //  skip caves addition
                            return modDataService.MapGrid.Where(p => p.Value == expansionName).First().Key;
                        }
                        else
                        {
                            //
                            //  verify grid availability
                            //
                            if (oBase.GridId > -1)
                            {
                                if (modDataService.MapGrid.ContainsKey(oBase.GridId))
                                {
                                    //
                                    //  conflict grid allocations, should not happen
                                    //
                                    oBase.GridId = GetMapGridId();
                                    if (oBase.GridId > -1)
                                    {
                                        if (modDataService.MapGrid.TryAdd(oBase.GridId, expansionName))
                                            returnId = oBase.GridId;
                                    }
                                }
                                else
                                {
                                    if (modDataService.MapGrid.TryAdd(oBase.GridId, expansionName))
                                        returnId = oBase.GridId;
                                }
                            }
                            else
                            {
                                oBase.GridId = GetMapGridId();
                                if (oBase.GridId > -1)
                                {
                                    if (modDataService.MapGrid.TryAdd(oBase.GridId, expansionName))
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
                    ExpansionPack oPackToActivate = modDataService.validContents[expansionName];

                    if (oPackToActivate.CaveEntrance != null && oPackToActivate.CaveEntrance.WarpIn != null && oPackToActivate.CaveEntrance.WarpOut != null)
                    {
                        mapUtilities.AddTouchAction(modDataService.farmExpansions[expansionName].Map, new Point(oPackToActivate.CaveEntrance.WarpOut.X, oPackToActivate.CaveEntrance.WarpOut.Y), ModKeyStrings.TAction_ToMeadows);
                        //modDataService.farmExpansions[expansionName].warps.Add(new Warp(oPackToActivate.CaveEntrance.WarpOut.X, oPackToActivate.CaveEntrance.WarpOut.Y, WarproomManager.StardewMeadowsLoacationName, (int)WarproomManager.WarpRoomEntrancePoint.X, (int)WarproomManager.WarpRoomEntrancePoint.Y, false));
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
            for (int i = 0; i < modDataService.MaximumExpansions; i++)
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
        private string FormatDirectionText(EntranceDirection direction, string text)
        {
            return direction switch
            {
                EntranceDirection.East => "> " + text,
                EntranceDirection.West => "@ " + text,
                EntranceDirection.North => "` " + text,
                EntranceDirection.South => "" + text,
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
        internal override void AddSignPost(GameLocation oExp, EntrancePatch oExpPatch, string messageKeySuffix, string signMessage)
        {
            //
            //  the sign post can either be on the building layer to block,
            //  or added to the Front of an existing map with a building layer tile
            //
            if (modDataService.Config.UseSignPosts)
            {
                try
                {
                    if (oExp == null)
                    {
                        logger.Log($"Attempt to add sign to non-loaded expansion {messageKeySuffix}", LogLevel.Debug);
                    }
                    else
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
                }
                catch (Exception ex)
                {
                    logger.Log($"Error AddingSignPost to {oExp.Name}", LogLevel.Error);
                    logger.LogError(ex);
                }
            }
        }

        /// <summary>
        ///  Joins the entrance/exit warps of two maps
        ///  removes the path blocks for the entrance from the 2 maps
        /// </summary>
        /// <param name="exitPatch"></param>
        /// <param name="exitLocationName"></param>
        /// <param name="entrancePatch"></param>
        /// <param name="entranceLocationName"></param>
        private void AddExpansionIntersection(EntrancePatch exitPatch, string exitLocationName, EntrancePatch entrancePatch, string entranceLocationName)
        {
            //
            //  get Gameloactions
            //
            string sourceLocationName = exitLocationName;
            if (!string.IsNullOrEmpty(exitPatch.WarpOut.LocationName))
                sourceLocationName = exitPatch.WarpOut.LocationName;

            GameLocation glA = Game1.getLocationFromName(sourceLocationName);
            if (glA is null)
            {
                logger.Log($"AddExpansionIntersection could not find location: {exitLocationName}", LogLevel.Debug);
                return;
            }
            string targetLocationName = entranceLocationName;
            if (!string.IsNullOrEmpty(entrancePatch.WarpIn.LocationName))
                targetLocationName = entrancePatch.WarpIn.LocationName;

            GameLocation glB = Game1.getLocationFromName(targetLocationName);
            if (glB is null)
            {
                logger.Log($"AddExpansionIntersection could not find location: {entranceLocationName}", LogLevel.Debug);
                return;
            }
            //
            //  remove path blocks from each expansion
            //
            RemovePathBlock(new Vector2(exitPatch.PathBlockX, exitPatch.PathBlockY), glA, exitPatch.WarpOrientation, exitPatch.WarpOut.NumberofPoints);
            RemovePathBlock(new Vector2(entrancePatch.PathBlockX, entrancePatch.PathBlockY), glB, entrancePatch.WarpOrientation, entrancePatch.WarpOut.NumberofPoints);
            //
            //  add the warps between the expansions
            //
            AddExpansionWarps(exitPatch, glA, entrancePatch, glB);
        }
        /// <summary>
        /// add warps between 2 expansions
        /// Compensates for mis-matches in the size of each destination
        /// </summary>
        /// <param name="exitPatch"></param>
        /// <param name="glA"></param>
        /// <param name="entrancePatch"></param>
        /// <param name="glB"></param>
        private void AddExpansionWarps(EntrancePatch exitPatch, GameLocation glA, EntrancePatch entrancePatch, GameLocation glB, bool addWarpBack = true)
        {
            int numberOfWarps = Math.Max(exitPatch.WarpOut.NumberofPoints, entrancePatch.WarpIn.NumberofPoints);

#if DEBUG
            logger.Log($"       AddExpansionWarps. {glA.NameOrUniqueName} to {glB.NameOrUniqueName}", LogLevel.Debug);
            logger.Log($"       numwarps: {numberOfWarps}", LogLevel.Debug);
            logger.Log($"       exit points: {exitPatch.WarpOut.NumberofPoints} {exitPatch.WarpOrientation}", LogLevel.Debug);
            logger.Log($"   entrance points: {entrancePatch.WarpIn.NumberofPoints} {entrancePatch.WarpOrientation}", LogLevel.Debug);
#endif

            for (int warpIndex = 0; warpIndex < numberOfWarps; warpIndex++)
            {
                try
                {
                    //
                    //  check for patch point underflows
                    //
                    int exitCellIndex = warpIndex < exitPatch.WarpOut.NumberofPoints ? warpIndex : exitPatch.WarpOut.NumberofPoints - 1;
                    int entranceCellIndex = warpIndex < entrancePatch.WarpIn.NumberofPoints ? warpIndex : entrancePatch.WarpIn.NumberofPoints - 1;

                    logger.Log($"   exitIndex: {exitCellIndex}, entIndex: {entranceCellIndex} ", LogLevel.Debug);
                    int exitWarpOutX;
                    int exitWarpOutY;
                    int exitWarpInX;
                    int exitWarpInY;
                    if (exitPatch.WarpOrientation == WarpOrientations.Horizontal)
                    {
                        exitWarpOutX = exitPatch.WarpOut.X + exitCellIndex;
                        exitWarpOutY = exitPatch.WarpOut.Y;
                        exitWarpInX = exitPatch.WarpIn.X + exitCellIndex;
                        exitWarpInY = exitPatch.WarpIn.Y;
                    }
                    else
                    {
                        exitWarpOutX = exitPatch.WarpOut.X;
                        exitWarpOutY = exitPatch.WarpOut.Y + exitCellIndex;
                        exitWarpInX = exitPatch.WarpIn.X;
                        exitWarpInY = exitPatch.WarpIn.Y + exitCellIndex;
                    }
                    int entranceWarpInX;
                    int entranceWarpInY;
                    int entranceWarpOutX;
                    int entranceWarpOutY;
                    if (entrancePatch.WarpOrientation == WarpOrientations.Horizontal)
                    {
                        entranceWarpInX = entrancePatch.WarpIn.X + entranceCellIndex;
                        entranceWarpInY = entrancePatch.WarpIn.Y;
                        entranceWarpOutX = entrancePatch.WarpOut.X + entranceCellIndex;
                        entranceWarpOutY = entrancePatch.WarpOut.Y;
                    }
                    else
                    {
                        entranceWarpInX = entrancePatch.WarpIn.X;
                        entranceWarpInY = entrancePatch.WarpIn.Y + entranceCellIndex;
                        entranceWarpOutX = entrancePatch.WarpOut.X;
                        entranceWarpOutY = entrancePatch.WarpOut.Y + entranceCellIndex;
                    }
                    if (warpIndex < exitPatch.WarpOut.NumberofPoints)
                    {
                        IEnumerable<Warp> currentWarp = glA.warps.Where(p => p.X == exitWarpOutX && p.Y == exitWarpOutY);
                        if (currentWarp.Any())
                        {
                            glA.warps.Remove(glA.warps.Where(p => p.X == exitWarpOutX && p.Y == exitWarpOutY).First());
                        }
                        glA.warps.Add(new Warp(exitWarpOutX, exitWarpOutY, glB.Name, entranceWarpInX, entranceWarpInY, false));
                    }
                    if (addWarpBack && warpIndex < entrancePatch.WarpOut.NumberofPoints)
                    {
                        var existingWarp = glB.warps.Where(p => p.X == entranceWarpOutX && p.Y == entranceWarpOutY);
                        if (existingWarp.Any())
                        {
                            glB.warps.Remove(glB.warps.Where(p => p.X == entranceWarpOutX && p.Y == entranceWarpOutY).First());
                        }
                        glB.warps.Add(new Warp(entranceWarpOutX, entranceWarpOutY, glA.Name, exitWarpInX, exitWarpInY, false));
                    }

                    //if (exitPatch.WarpOrientation == WarpOrientations.Horizontal)
                    //{
                    //    //
                    //    //  remove existing warp to support swapping expansions
                    //    //
                    //    IEnumerable<Warp> existingWarp = glA.warps.Where(p => p.X == exitPatch.WarpOut.X + exitCellIndex && p.Y == exitPatch.WarpOut.Y);
                    //    if (existingWarp.Any())
                    //    {
                    //        glA.warps.Remove(glA.warps.Where(p => p.X == exitPatch.WarpOut.X + exitCellIndex && p.Y == exitPatch.WarpOut.Y).First());
                    //    }
                    //    glA.warps.Add(new Warp(exitPatch.WarpOut.X + exitCellIndex, exitPatch.WarpOut.Y, glB.Name, entrancePatch.WarpIn.X + entranceCellIndex, entrancePatch.WarpIn.Y, false));
                    //    //if (addWarpBack)
                    //    //{
                    //    //    //
                    //    //    //  remove existing warp to support swapping expansions
                    //    //    //
                    //    //    existingWarp = glB.warps.Where(p => p.X == entrancePatch.WarpOut.X + iExitCell && p.Y == entrancePatch.WarpOut.Y);
                    //    //    if (existingWarp.Any())
                    //    //    {
                    //    //        glB.warps.Remove(glB.warps.Where(p => p.X == entrancePatch.WarpOut.X + iExitCell && p.Y == entrancePatch.WarpOut.Y).First());
                    //    //    }
                    //    //    glB.warps.Add(new Warp(entrancePatch.WarpOut.X + iEntCell, entrancePatch.WarpOut.Y, glA.Name, oExitPatch.WarpIn.X + iExitCell, oExitPatch.WarpIn.Y, false));
                    //    //}
                    //}
                    //else
                    //{
                    //    IEnumerable<Warp> existingWarp = glA.warps.Where(p => p.X == exitPatch.WarpOut.X && p.Y == exitPatch.WarpOut.Y + exitCellIndex);
                    //    if (existingWarp.Any())
                    //    {
                    //        glA.warps.Remove(glA.warps.Where(p => p.X == exitPatch.WarpOut.X && p.Y == exitPatch.WarpOut.Y + exitCellIndex).First());
                    //    }
                    //    glA.warps.Add(new Warp(exitPatch.WarpOut.X, exitPatch.WarpOut.Y + exitCellIndex, glB.Name, entrancePatch.WarpIn.X, entrancePatch.WarpIn.Y + entranceCellIndex, false));

                    //    //if (addWarpBack)
                    //    //{
                    //    //    existingWarp = glB.warps.Where(p => p.X == entrancePatch.WarpOut.X && p.Y == entrancePatch.WarpOut.Y + iEntCell);
                    //    //    if (existingWarp.Any())
                    //    //    {
                    //    //        glB.warps.Remove(glB.warps.Where(p => p.X == entrancePatch.WarpOut.X && p.Y == entrancePatch.WarpOut.Y + iEntCell).First());
                    //    //    }
                    //    //    glB.warps.Add(new Warp(entrancePatch.WarpOut.X, entrancePatch.WarpOut.Y + iEntCell, glA.Name, oExitPatch.WarpIn.X, oExitPatch.WarpIn.Y + iExitCell, false));
                    //    //}
                    //}

                    //if (addWarpBack)
                    //{
                    //    if (entrancePatch.WarpOrientation == WarpOrientations.Horizontal)
                    //    {
                    //        //
                    //        //  remove existing warp to support swapping expansions
                    //        //
                    //        var existingWarp = glB.warps.Where(p => p.X == entrancePatch.WarpOut.X + entranceCellIndex && p.Y == entrancePatch.WarpOut.Y);
                    //        if (existingWarp.Any())
                    //        {
                    //            glB.warps.Remove(glB.warps.Where(p => p.X == entrancePatch.WarpOut.X + entranceCellIndex && p.Y == entrancePatch.WarpOut.Y).First());
                    //        }
                    //        glB.warps.Add(new Warp(entrancePatch.WarpOut.X + entranceCellIndex, entrancePatch.WarpOut.Y, glA.Name, exitPatch.WarpIn.X + exitCellIndex, exitPatch.WarpIn.Y, false));
                    //    }
                    //    else
                    //    {
                    //        var existingWarp = glB.warps.Where(p => p.X == entrancePatch.WarpOut.X && p.Y == entrancePatch.WarpOut.Y + entranceCellIndex);
                    //        if (existingWarp.Any())
                    //        {
                    //            glB.warps.Remove(glB.warps.Where(p => p.X == entrancePatch.WarpOut.X && p.Y == entrancePatch.WarpOut.Y + entranceCellIndex).First());
                    //        }
                    //        glB.warps.Add(new Warp(entrancePatch.WarpOut.X, entrancePatch.WarpOut.Y + entranceCellIndex, glA.Name, exitPatch.WarpIn.X, exitPatch.WarpIn.Y + exitCellIndex, false));
                    //    }
                    //}
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
        private void RemovePathBlock(Vector2 blockPosition, GameLocation location, WarpOrientations blockOrientation, int blockWidth)
        {
#if DEBUG_LOG
            logger.Log($"   Removing path block {location.Name} {blockPosition}, {blockOrientation} for {blockWidth}", LogLevel.Debug);
#endif

            try
            {
                Layer buildingsLayer = location.map.GetLayer("Buildings");
                switch (blockWidth)
                {
                    case 2:
                        //
                        //  remove boulder
                        //
                        if (buildingsLayer.Tiles[(int)blockPosition.X, (int)blockPosition.Y] != null) location.removeTile((int)blockPosition.X, (int)blockPosition.Y, "Buildings");
                        if (buildingsLayer.Tiles[(int)blockPosition.X + 1, (int)blockPosition.Y] != null) location.removeTile((int)blockPosition.X + 1, (int)blockPosition.Y, "Buildings");
                        if (buildingsLayer.Tiles[(int)blockPosition.X, (int)blockPosition.Y + 1] != null) location.removeTile((int)blockPosition.X, (int)blockPosition.Y + 1, "Buildings");
                        if (buildingsLayer.Tiles[(int)blockPosition.X + 1, (int)blockPosition.Y + 1] != null) location.removeTile((int)blockPosition.X + 1, (int)blockPosition.Y + 1, "Buildings");
                        break;
                    default:
                        //
                        //  remove fence
                        //
                        if (blockOrientation == WarpOrientations.Horizontal)
                        {
                            RemoveTile(location, (int)blockPosition.X, (int)blockPosition.Y - 1, "Buildings");
                            RemoveTile(location, (int)blockPosition.X, (int)blockPosition.Y, "Buildings");
                            int iMid;
                            for (iMid = 1; iMid < blockWidth - 1; iMid++)
                            {
                                RemoveTile(location, (int)blockPosition.X + iMid, (int)blockPosition.Y - 1, "Buildings");
                                RemoveTile(location, (int)blockPosition.X + iMid, (int)blockPosition.Y, "Buildings");
                            }
                            RemoveTile(location, (int)blockPosition.X + iMid, (int)blockPosition.Y - 1, "Buildings");
                            RemoveTile(location, (int)blockPosition.X + iMid, (int)blockPosition.Y, "Buildings");
                        }
                        else
                        {
                            RemoveTile(location, (int)blockPosition.X, (int)blockPosition.Y - 1, "AlwaysFront");
                            //RemoveTile(gl, (int)oBlockPos.X, (int)oBlockPos.Y -  1, "Buildings", 361);
                            RemoveTile(location, (int)blockPosition.X, (int)blockPosition.Y, "Buildings");
                            int iMid;
                            for (iMid = 1; iMid < blockWidth; iMid++)
                            {
                                RemoveTile(location, (int)blockPosition.X, (int)blockPosition.Y + iMid, "Buildings");
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
        private GameLocation GetLocation(string locationName)
        {
            return Game1.getLocationFromName(locationName);
        }
        private void JoinMaps(int sourceGridId, int targetGridId, EntranceDirection sourceSide)
        {
            //
            //  Join intersecting expansions
            //
            //  - remove path blocks
            //  - add warps
            //  - add signs
            //
            List<int> topRow = new List<int> { 0, 1, 4, 7, 10 };
            EntranceDirection targetSide = GetNeighbourSide(sourceSide);
            string sourceMessageName = GetSignPostMessagePrefix(sourceSide);
            string targetMessageName = GetSignPostMessagePrefix(targetSide);
            ExpansionPack targetPack = modDataService.validContents[modDataService.MapGrid[targetGridId]];
            ExpansionPack sourcePack = modDataService.validContents[modDataService.MapGrid[sourceGridId]];
            if (targetSide == EntranceDirection.West && targetPack.isAdditionalFarm)
            {
                //add TouchAction to the Northern exit of source
                //add warp source-east => target-north

                //  map directly east exit to northern entrance
                // remove northern path block and add back warp
                // to sourceGrid east entrance
                EntrancePatch entrancePatch = targetPack.EntrancePatches[((int)EntranceDirection.North).ToString()];
                EntrancePatch exitPatch = sourcePack.EntrancePatches[((int)EntranceDirection.East).ToString()];

                string exitLocation = sourcePack.LocationName;
                if (!string.IsNullOrEmpty(exitPatch.WarpOut.LocationName))
                    exitLocation = exitPatch.WarpOut.LocationName;

                string entranceLocation = targetPack.LocationName;
                if (!string.IsNullOrEmpty(entrancePatch.WarpIn.LocationName))
                    entranceLocation = entrancePatch.WarpIn.LocationName;
                //
                //  remove path block from target
                RemovePathBlock(new Vector2(entrancePatch.PathBlockX, entrancePatch.PathBlockY), GetLocation(entranceLocation), entrancePatch.WarpOrientation, entrancePatch.WarpOut.NumberofPoints);
                // remove path block from source
                RemovePathBlock(new Vector2(exitPatch.PathBlockX, exitPatch.PathBlockY), GetLocation(exitLocation), exitPatch.WarpOrientation, exitPatch.WarpOut.NumberofPoints);
                // add warp source-east => target-north
                AddExpansionWarps(exitPatch, GetLocation(exitLocation), entrancePatch, GetLocation(entranceLocation), false);
                // add exit warp
                if (topRow.Contains(targetGridId))
                {
                    // add source-north => target-east warp
                    AddExpansionWarps(exitPatch, GetLocation(exitLocation), entrancePatch, GetLocation(entranceLocation));
                }
                else
                {
                    // add TouchAction 
                    AddWarpPickTouchAction(entrancePatch, entranceLocation);
                }
            }
            else if (sourceSide == EntranceDirection.West && sourcePack.isAdditionalFarm)
            {
                //add TouchAction to the Northern exit of source
                // add warp target-east => south-north
                EntrancePatch exitPatch = sourcePack.EntrancePatches[((int)EntranceDirection.North).ToString()];
                EntrancePatch entrancePatch = targetPack.EntrancePatches[((int)EntranceDirection.East).ToString()];
                string exitLocation = sourcePack.LocationName;
                if (!string.IsNullOrEmpty(exitPatch.WarpOut.LocationName))
                    exitLocation = exitPatch.WarpOut.LocationName;
                string entranceLocation = targetPack.LocationName;
                if (!string.IsNullOrEmpty(entrancePatch.WarpIn.LocationName))
                    entranceLocation = entrancePatch.WarpIn.LocationName;
                //
                //  remove path block from target
                RemovePathBlock(new Vector2(entrancePatch.PathBlockX, entrancePatch.PathBlockY), GetLocation(entranceLocation), entrancePatch.WarpOrientation, entrancePatch.WarpOut.NumberofPoints);
                //
                // remove path block from source
                RemovePathBlock(new Vector2(exitPatch.PathBlockX, exitPatch.PathBlockY), GetLocation(exitLocation), exitPatch.WarpOrientation, exitPatch.WarpOut.NumberofPoints);
                // add exit warp
                if (topRow.Contains(sourceGridId))
                {
                    // add source-north => target-east warp
                    AddExpansionWarps(exitPatch, GetLocation(exitLocation), entrancePatch, GetLocation(entranceLocation));
                }
                else
                {
                    // add TouchAction 
                    AddWarpPickTouchAction(exitPatch, exitLocation);
                    // add warp back from the east
                    AddExpansionWarps(entrancePatch, GetLocation(entranceLocation), exitPatch, GetLocation(exitLocation), false);
                }
            }
            else if (sourceSide == EntranceDirection.North && modDataService.validContents[modDataService.MapGrid[sourceGridId]].isAdditionalFarm && modDataService.MapGrid.ContainsKey(targetGridId + 4))
            {
                // north side has a pick destination
                //  only add target-south => source-north
                EntrancePatch exitPatch = targetPack.EntrancePatches[((int)EntranceDirection.South).ToString()];
                EntrancePatch entrancePatch = sourcePack.EntrancePatches[((int)EntranceDirection.North).ToString()];
                string exitLocation = sourcePack.LocationName;
                if (!string.IsNullOrEmpty(exitPatch.WarpOut.LocationName))
                    exitLocation = exitPatch.WarpOut.LocationName;
                string entranceLocation = targetPack.LocationName;
                if (!string.IsNullOrEmpty(entrancePatch.WarpIn.LocationName))
                    entranceLocation = entrancePatch.WarpIn.LocationName;

                RemovePathBlock(new Vector2(exitPatch.PathBlockX, exitPatch.PathBlockY), GetLocation(exitLocation), exitPatch.WarpOrientation, exitPatch.WarpOut.NumberofPoints);

                AddExpansionWarps(exitPatch, GetLocation(exitLocation), entrancePatch, GetLocation(entranceLocation), false);

            }
            else if (targetSide == EntranceDirection.North && modDataService.validContents[modDataService.MapGrid[targetGridId]].isAdditionalFarm && modDataService.MapGrid.ContainsKey(targetGridId + 3))
            {
                // north entrance has a pick destination 
                //
                EntrancePatch exitPatch = sourcePack.EntrancePatches[((int)EntranceDirection.South).ToString()];
                EntrancePatch entrancePatch = targetPack.EntrancePatches[((int)EntranceDirection.North).ToString()];
                string exitLocation = sourcePack.LocationName;
                if (!string.IsNullOrEmpty(exitPatch.WarpOut.LocationName))
                    exitLocation = exitPatch.WarpOut.LocationName;
                string entranceLocation = targetPack.LocationName;
                if (!string.IsNullOrEmpty(entrancePatch.WarpIn.LocationName))
                    entranceLocation = entrancePatch.WarpIn.LocationName;
                RemovePathBlock(new Vector2(exitPatch.PathBlockX, exitPatch.PathBlockY), GetLocation(exitLocation), exitPatch.WarpOrientation, exitPatch.WarpOut.NumberofPoints);
                AddExpansionWarps(exitPatch, GetLocation(exitLocation), entrancePatch, GetLocation(entranceLocation), false);
            }
            else if (sourceGridId == 0)
            {


                //if (sourcePack.isAdditionalFarm)
                //{
                //    // add TouchAction to the Northern exit
                //    EntrancePatch sourceExitPatch = sourcePack.EntrancePatches["0"];
                //    AddWarpPickTouchAction(sourceExitPatch, sourcePack.LocationName);
                //}
                //else
                //{
                // add warp from source-west => target-east
                EntrancePatch targetEntPatch = targetPack.EntrancePatches[((int)EntranceDirection.East).ToString()];
                EntrancePatch sourceEntPatch = sourcePack.EntrancePatches[((int)EntranceDirection.North).ToString()];

                RemovePathBlock(new Vector2(targetEntPatch.PathBlockX, targetEntPatch.PathBlockY), modDataService.farmExpansions[targetPack.LocationName], targetEntPatch.WarpOrientation, targetEntPatch.WarpOut.NumberofPoints);
                AddExpansionWarps(targetEntPatch, modDataService.farmExpansions[targetPack.LocationName], sourceEntPatch, modDataService.farmExpansions[sourcePack.LocationName]);
                //}
            }
            //else if (targetGrid == 0)
            //{

            //}
            //else if (targetSide == EntranceDirection.West && targetPack.isAdditionalFarm)
            //{
            //    //if (sourceGrid == 0 || (sourceGrid + 2) % 3 == 0)
            //    //{
            //    //  map directly east exit to northern entrance
            //    // remove northern path block and add back warp
            //    // to sourceGrid east entrance
            //    EntrancePatch targetEntPatch = targetPack.EntrancePatches[((int)EntranceDirection.North).ToString()];
            //    EntrancePatch sourceEntPatch = sourcePack.EntrancePatches[((int)sourceSide).ToString()];
            //    //
            //    //  remove path block from target
            //    RemovePathBlock(new Vector2(targetEntPatch.PathBlockX, targetEntPatch.PathBlockY), modDataService.farmExpansions[targetPack.LocationName], targetEntPatch.WarpOrientation, targetEntPatch.WarpOut.NumberofPoints);
            //    AddExpansionWarps(targetEntPatch, modDataService.farmExpansions[targetPack.LocationName], sourceEntPatch, modDataService.farmExpansions[sourcePack.LocationName]);

            //    //}
            //    //else
            //    //{
            //    //    int x = 3;
            //    //    EntrancePatch sourceExitPatch = sourcePack.EntrancePatches[((int)sourceSide).ToString()];
            //    //    EntrancePatch targetEntrancePatch = targetPack.EntrancePatches[((int)EntranceDirection.East).ToString()];

            //    //    AddWarpPickTouchAction(sourceExitPatch, sourcePack.LocationName);
            //    //    //AddWarpToBridgeLocation(sourceExitPatch, sourcePack.LocationName, true);
            //    //    // remove eastern path block on sourceGrid+3
            //    //    RemovePathBlock(new Vector2(targetEntrancePatch.PathBlockX, targetEntrancePatch.PathBlockY), modDataService.farmExpansions[targetPack.LocationName], targetEntrancePatch.WarpOrientation, targetEntrancePatch.WarpOut.NumberofPoints);
            //    //    // add warp to north entrance from west neighbour
            //    //    AddExpansionWarps(sourceExitPatch, modDataService.farmExpansions[sourcePack.LocationName], targetEntrancePatch, modDataService.farmExpansions[targetPack.LocationName]);
            //    //}
            //}
            //else if (sourceSide == EntranceDirection.West && sourcePack.isAdditionalFarm)
            //{
            //    if (sourceGridId == 0 || (sourceGridId + 2) % 3 == 0)
            //    {
            //        //  map directly east exit to northern entrance
            //        // remove northern path block and add back warp
            //        // to sourceGrid east entrance
            //        EntrancePatch targetEntPatch = targetPack.EntrancePatches[((int)targetSide).ToString()];
            //        EntrancePatch sourceEntPatch = sourcePack.EntrancePatches[((int)EntranceDirection.North).ToString()];
            //        //
            //        //  remove path block from target
            //        RemovePathBlock(new Vector2(sourceEntPatch.PathBlockX, sourceEntPatch.PathBlockY), modDataService.farmExpansions[sourcePack.LocationName], sourceEntPatch.WarpOrientation, sourceEntPatch.WarpOut.NumberofPoints);
            //        AddExpansionWarps(sourceEntPatch, modDataService.farmExpansions[sourcePack.LocationName], targetEntPatch, modDataService.farmExpansions[targetPack.LocationName]);
            //    }
            //    else
            //    {
            //        int x = 1;

            //        EntrancePatch sourceExitPatch = sourcePack.EntrancePatches[((int)EntranceDirection.North).ToString()];
            //        EntrancePatch targetEntrancePatch = targetPack.EntrancePatches[((int)EntranceDirection.East).ToString()];

            //        // add warp to bridge loctaion
            //        AddWarpPickTouchAction(sourceExitPatch, sourcePack.LocationName);
            //        //AddWarpToBridgeLocation(sourceExitPatch, sourcePack.LocationName, true);
            //        // remove eastern path block on sourceGrid+3
            //        RemovePathBlock(new Vector2(targetEntrancePatch.PathBlockX, targetEntrancePatch.PathBlockY), modDataService.farmExpansions[targetPack.LocationName], targetEntrancePatch.WarpOrientation, targetEntrancePatch.WarpOut.NumberofPoints);
            //        // add warp to north entrance from west neighbour
            //        AddExpansionWarps(targetEntrancePatch, modDataService.farmExpansions[targetPack.LocationName], sourceExitPatch, modDataService.farmExpansions[sourcePack.LocationName], false);



            //        //EntrancePatch sourceExitPatch = sourcePack.EntrancePatches[((int)EntranceDirection.North).ToString()];
            //        ////EntrancePatch targetEntrancePatch = targetPack.EntrancePatches[((int)EntranceDirection.East).ToString()];

            //        //// add warp to bridge loctaion
            //        //AddWarpToBridgeLocation(sourceExitPatch, sourcePack.LocationName, false);
            //        //// remove eastern path block on sourceGrid+3
            //        ////RemovePathBlock(new Vector2(targetEntrancePatch.PathBlockX, targetEntrancePatch.PathBlockY), modDataService.farmExpansions[targetPack.LocationName], targetEntrancePatch.WarpOrientation, targetEntrancePatch.WarpOut.NumberofPoints);
            //        //// add warp back to bridge location in sourceGrid+3
            //        ////AddWarpToBridgeLocation(targetEntrancePatch, targetPack.LocationName, false);
            //    }

            //}
            //else if (sourceSide == EntranceDirection.North && sourceGridId < 8 && modDataService.MapGrid.ContainsKey(sourceGridId + 3) && sourcePack.isAdditionalFarm)
            //{
            //    EntrancePatch sourceExitPatch = sourcePack.EntrancePatches[((int)sourceSide).ToString()];
            //    EntrancePatch targetEntrancePatch = targetPack.EntrancePatches[((int)EntranceDirection.East).ToString()];

            //    // add warp to bridge loctaion
            //    AddWarpPickTouchAction(sourceExitPatch, sourcePack.LocationName);
            //    //AddWarpToBridgeLocation(sourceExitPatch, sourcePack.LocationName, true);
            //    // remove eastern path block on sourceGrid+3
            //    RemovePathBlock(new Vector2(targetEntrancePatch.PathBlockX, targetEntrancePatch.PathBlockY), modDataService.farmExpansions[targetPack.LocationName], targetEntrancePatch.WarpOrientation, targetEntrancePatch.WarpOut.NumberofPoints);
            //    // add warp to north entrance from west neighbour
            //    AddExpansionWarps(targetEntrancePatch, modDataService.farmExpansions[targetPack.LocationName], sourceExitPatch, modDataService.farmExpansions[sourcePack.LocationName], false);

            //}
            //else if (sourceSide == EntranceDirection.South && modDataService.MapGrid.ContainsKey(sourceGridId + 1) && modDataService.validContents[modDataService.MapGrid[sourceGridId + 1]].isAdditionalFarm)
            //{
            //    EntrancePatch sourceExitPatch = sourcePack.EntrancePatches[((int)sourceSide).ToString()];
            //    EntrancePatch targetEntrancePatch = targetPack.EntrancePatches[((int)EntranceDirection.North).ToString()];

            //    // remove eastern path block on sourceGrid+1
            //    RemovePathBlock(new Vector2(targetEntrancePatch.PathBlockX, targetEntrancePatch.PathBlockY), modDataService.farmExpansions[targetPack.LocationName], targetEntrancePatch.WarpOrientation, targetEntrancePatch.WarpOut.NumberofPoints);
            //    // add warp to north entrance from south neighbour
            //    AddExpansionWarps(sourceExitPatch, modDataService.farmExpansions[sourcePack.LocationName], targetEntrancePatch, modDataService.farmExpansions[targetPack.LocationName], false);

            //}
            else
            {
                if (!targetPack.EntrancePatches.ContainsKey(((int)targetSide).ToString()))
                    return;

                EntrancePatch targetEntPatch = targetPack.EntrancePatches[((int)targetSide).ToString()];
                EntrancePatch sourceEntPatch = sourcePack.EntrancePatches[((int)sourceSide).ToString()];

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
        }
        private void AddWarpPickTouchAction(EntrancePatch patch, string locationName)
        {
            Map map = GetLocation(locationName).Map;

            Layer backLayer = map.GetLayer("Back");

            if (backLayer != null)
            {
                for (int index = 0; index < patch.WarpOut.NumberofPoints; index++)
                {
                    if (backLayer.IsValidTileLocation(patch.WarpOut.X + index, Math.Max(0, patch.WarpOut.Y)))
                    {
                        backLayer.Tiles[patch.WarpOut.X + index, Math.Max(0, patch.WarpOut.Y)].Properties["TouchAction"] = ModKeyStrings.TAction_PickDestination;
                    }
                }
            }
        }
        //private void AddWarpToBridgeLocation(EntrancePatch sourcePatch, string sourceLocationName, bool southernPatch)
        //{
        //    GameLocation gameLocation = Game1.getLocationFromName(sourceLocationName);

        //    if (gameLocation != null)
        //    {
        //        for (int index = 0; index < sourcePatch.WarpOut.NumberofPoints; index++)
        //        {
        //            int outPosX = sourcePatch.WarpOut.X;
        //            int outPosY = sourcePatch.WarpOut.Y;
        //            if (sourcePatch.WarpOrientation == WarpOrientations.Horizontal)
        //                outPosX += index;
        //            else
        //                outPosY += index;

        //            var currentWarps = gameLocation.warps.Where(p => p.X == outPosX && p.Y == outPosY);
        //            if (currentWarps.Any())
        //                gameLocation.warps.Remove(currentWarps.First());

        //            if (southernPatch)
        //            {
        //                gameLocation.warps.Add(new Warp(
        //                    sourcePatch.WarpOut.X + index, sourcePatch.WarpOut.Y,
        //                    ExpansionBridgingService.BridgingLocationName,
        //                    ExpansionBridgingService.SouthernExit.WarpOut.X + index, ExpansionBridgingService.SouthernExit.WarpOut.Y,
        //                    false
        //                    ));
        //            }
        //            else
        //            {
        //                gameLocation.warps.Add(new Warp(
        //                    sourcePatch.WarpOut.X + index, sourcePatch.WarpOut.Y,
        //                    ExpansionBridgingService.BridgingLocationName,
        //                    ExpansionBridgingService.WesternExit.WarpOut.X, ExpansionBridgingService.WesternExit.WarpOut.Y + index,
        //                    false
        //                    ));

        //            }
        //            //if (sourcePatch.WarpOrientation == WarpOrientations.Horizontal)
        //            //{
        //            //    //var currentWarps = gameLocation.warps.Where(p => p.X == sourcePatch.WarpOut.X + index && p.Y == sourcePatch.WarpOut.Y);
        //            //    //if (currentWarps.Any())
        //            //    //    gameLocation.warps.Remove(currentWarps.First());

        //            //    if (southernPatch)
        //            //    {
        //            //        gameLocation.warps.Add(new Warp(
        //            //            sourcePatch.WarpOut.X + index, sourcePatch.WarpOut.Y,
        //            //            ExpansionBridgingService.BridgingLocationName,
        //            //            ExpansionBridgingService.SouthernExit.WarpOut.X + index, ExpansionBridgingService.SouthernExit.WarpOut.Y,
        //            //            false
        //            //            ));
        //            //    }
        //            //    else
        //            //    {
        //            //        gameLocation.warps.Add(new Warp(
        //            //            sourcePatch.WarpOut.X + index, sourcePatch.WarpOut.Y,
        //            //            ExpansionBridgingService.BridgingLocationName,
        //            //            ExpansionBridgingService.WesternExit.WarpOut.X, ExpansionBridgingService.WesternExit.WarpOut.Y + index,
        //            //            false
        //            //            ));

        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    //var currentWarps = gameLocation.warps.Where(p => p.X == sourcePatch.WarpOut.X && p.Y == sourcePatch.WarpOut.Y + index);
        //            //    //if (currentWarps.Any())
        //            //    //    gameLocation.warps.Remove(currentWarps.First());

        //            //    if (southernPatch)
        //            //    {
        //            //        gameLocation.warps.Add(new Warp(
        //            //            sourcePatch.WarpOut.X, sourcePatch.WarpOut.Y + index,
        //            //            ExpansionBridgingService.BridgingLocationName,
        //            //            ExpansionBridgingService.SouthernExit.WarpOut.X, ExpansionBridgingService.SouthernExit.WarpOut.Y + index,
        //            //            false
        //            //            ));
        //            //    }
        //            //    else
        //            //    {
        //            //        gameLocation.warps.Add(new Warp(
        //            //         sourcePatch.WarpOut.X, sourcePatch.WarpOut.Y + index,
        //            //         ExpansionBridgingService.BridgingLocationName,
        //            //         ExpansionBridgingService.WesternExit.WarpOut.X, ExpansionBridgingService.WesternExit.WarpOut.Y + index,
        //            //         false
        //            //         ));
        //            //    }
        //            //}
        //        }
        //    }
        //}
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
        //private string GetSignPostMessagePrefix(EntranceDirection side)
        //{
        //    return side switch
        //    {
        //        EntranceDirection.West => "WestMessage",
        //        EntranceDirection.East => "EastMessage",
        //        EntranceDirection.North => "NorthMessage",
        //        EntranceDirection.South => "SouthMessage",
        //        _ => ""
        //    };
        //}
        private static EntranceDirection GetNeighbourSide(EntranceDirection side)
        {
            return side switch
            {
                EntranceDirection.West => EntranceDirection.East,// eastSidePatch,
                EntranceDirection.East => EntranceDirection.West,// westSidePatch,
                EntranceDirection.North => EntranceDirection.South,// southSidePatch,
                EntranceDirection.South => EntranceDirection.North,// northSidePatch,
                _ => EntranceDirection.None
            };
        }
        //private string GetNeighbourSide(string side)
        //{
        //    return side switch
        //    {
        //        westSidePatch => eastSidePatch,
        //        eastSidePatch => westSidePatch,
        //        northSidePatch => southSidePatch,
        //        southSidePatch => northSidePatch,
        //        _ => ""
        //    };
        //}
        private bool IsMapGridOccupied(int iGridId)
        {
            return modDataService.MapGrid.ContainsKey(iGridId);
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
                    EntrancePatch? patchNeighbour = null;
                    string neighbour = GetNeighbourExpansionName(iGridId, oPatch.PatchSide);
                    GameLocation? glNeighbour = null;
                    if (!string.IsNullOrEmpty(neighbour))
                    {
                        EntranceDirection opside = GetNeighbourSide(oPatch.PatchSide);
                        modDataService.validContents[neighbour].EntrancePatches.TryGetValue(((int)opside).ToString(), out patchNeighbour);
                        //patchNeighbour = modDataService.validContents[neighbour].EntrancePatches[((int)opside).ToString()];
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
#if DEBUG
            logger.Log($"Fixing grid", LogLevel.Debug);
#endif
            //
            //  called after save to fix any holes in the grid
            //
            for (int farmCounter = 0; farmCounter < modDataService.MaximumExpansions; farmCounter++)
            {
                if (!modDataService.MapGrid.ContainsKey(farmCounter))
                {
                    bool found = false;
                    for (int replace = farmCounter + 1; replace < modDataService.MaximumExpansions; replace++)
                    {
                        if (modDataService.MapGrid.ContainsKey(replace))
                        {
#if DEBUG
                            logger.Log($"* moving Grid: {replace} to {farmCounter}", LogLevel.Debug);
#endif
                            found = true;
                            modDataService.MapGrid[farmCounter] = modDataService.MapGrid[replace];
                            modDataService.farmExpansions[modDataService.MapGrid[farmCounter]].modData["prism99.advize.stardewrealty.FEGridId"] = farmCounter.ToString();
                            modDataService.farmExpansions[modDataService.MapGrid[farmCounter]].GridId = farmCounter;
                            modDataService.MapGrid.Remove(replace);
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
