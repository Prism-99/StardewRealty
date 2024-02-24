using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.Utilities;
using System.Collections.Generic;
using System.Linq;
using Prism99_Core.Utilities;
using SDict = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation>;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using Prism99_Core.Abstractions.Buildings;
using StardewRealty.SDV_Realty_Interface;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using StardewModdingAPI.Events;
using System;

namespace SDV_Realty_Core.Framework.Expansions
{
    internal  class ExpansionManager
    {
        //
        //  version 1.6
        //
        private static ILoggerService logger;
        private static IModHelper helper;
        private const string baseFarmName = "Farm";
        internal SDict farmExpansions;
        internal Dictionary<string, ExpansionDetails> ExpDetails;
        internal  List<string> addedLocationTracker = new List<string>();
        private IContentPackService contentPackService;
        private IGridManager gridManager;
        private IUtilitiesService utilitiesService;
        private IContentManagerService  contentManagerService;
        private ILandManager landManager;
        private IExitsService exitsService;
        private IForSaleSignService forSaleSignService;
        private IModDataService modDataService;
        #region "Public Methods"
        public ExpansionManager(ILoggerService errorLogger, IUtilitiesService utilitiesService, IContentPackService contentPackService,IGridManager gridManager, IContentManagerService contentManagerService, ILandManager landManager, IExitsService exitsService,IForSaleSignService forSaleSignService, IModDataService modDataService)
        {
            this.utilitiesService = utilitiesService;
            logger = errorLogger;
            helper = utilitiesService.ModHelperService.modHelper;
            this.contentPackService = contentPackService;
            this.gridManager = gridManager;
            this.contentManagerService = contentManagerService;
            this.forSaleSignService= forSaleSignService;
            utilitiesService.GameEventsService.AddSubscription(new SaveCreatingEventArgs(), SaveCreated);
            utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), Specialized_LoadStageChanged);
            utilitiesService.GameEventsService.AddSubscription(new SavedEventArgs(), GameSaved);
            this.landManager = landManager;
            this.exitsService = exitsService;
            this.modDataService= modDataService;

            farmExpansions=modDataService.farmExpansions;
            ExpDetails=modDataService.expDetails;
        }
        private void GameSaved(EventArgs e)
        {
            ReAddActiveExpansions();
        }
        private void Specialized_LoadStageChanged(EventArgs eventArgs)
        {
            ReAddActiveExpansions();
        }
        private void SaveCreated(EventArgs e)
        {
            addedLocationTracker.Clear();
        }
        public  void ReAddActiveExpansions()
        {
            //
            //  re-add active expansions to Game1.locations
            //
            logger.Log($"ExpansionManager.ReAddActiveExpansions", LogLevel.Debug);
            var activeExpansions = farmExpansions.Where(p => p.Value.Active && p.Value.GridId > -1).OrderBy(q => q.Value.GridId).Select(r => r.Key).ToList();

            foreach (string expansionName in activeExpansions)
            {
                AddGameLocation(farmExpansions[expansionName], "ReAddActiveExpansions");
             }
        }
        internal  void AddGameLocation(GameLocation gl, string caller)
        {

            logger.Log($"AddGameLocation.{caller}: {gl.Name}:{gl.GetSeason()}", LogLevel.Debug);

            if (addedLocationTracker.Contains(gl.Name))
            {
                logger.Log($"     re-adding {gl.Name}", LogLevel.Debug);
            }

            if (Game1.getLocationFromName(gl.Name) == null || Game1.currentLocation?.Name == gl.Name)
            {
                Game1.locations.Add(gl);
                logger.Log($"     added {gl.Name}", LogLevel.Debug);
                addedLocationTracker.Add(gl.Name);
            }
            else
            {
                logger.Log($"     location already in Game1.locations: {gl.Name}", LogLevel.Debug);
            }
        }
        public  bool ActivateExpansion(string expansionName, int gridId = -1)
        {
            logger.Log($"    Activating: {expansionName}, Grid Id={gridId}", LogLevel.Debug);

            bool bAutoAdd = farmExpansions[expansionName].AutoAdd;
            //
            //  get the grid location, if auto mapped
            //

             FarmExpansionLocation expansionLocation = farmExpansions[expansionName];

            ExpansionPack oPackToActivate = contentPackService.contentPackLoader.ValidContents[expansionName];
 
            if (bAutoAdd)
            {
                //  
                //  get a grid id for the new expansion
                //
                gridId = gridManager.AddMapToGrid(expansionName);

                if (gridId == -1)
                {
                    logger.Log($"     Expansion {expansionName} not added, the grid is full.", LogLevel.Error);
                    return false;
                }
                else
                {
                    farmExpansions[expansionName].GridId = gridId;
                    expansionLocation.GridId = gridId;

                    //
                    //  add custom MapStrings, if defined
                    //
                    //  MapStrings are handled by the AssetEditor class
                    //
                    if (oPackToActivate?.MapStrings != null)
                    {
                        foreach (string key in oPackToActivate.MapStrings.Keys)
                        {
                            logger.Log($"     Adding map string {key}.{oPackToActivate.MapStrings[key]}", LogLevel.Debug);

                            contentManagerService.contentManager.stringFromMaps.Add(key, oPackToActivate.MapStrings[key]);
                        }
                    }

                    if (expansionLocation.modData.ContainsKey(FEModDataKeys.FEGridId))
                    {
                        expansionLocation.modData[FEModDataKeys.FEGridId] = gridId.ToString();
                    }
                    else
                    {
                        expansionLocation.modData.Add(FEModDataKeys.FEGridId, gridId.ToString());
                    }
                    if (farmExpansions[expansionName].modData.ContainsKey(FEModDataKeys.FEGridId))
                    {
                        farmExpansions[expansionName].modData[FEModDataKeys.FEGridId] = gridId.ToString();
                    }
                    else
                    {
                        farmExpansions[expansionName].modData.Add(FEModDataKeys.FEGridId, gridId.ToString());
                    }
                }
            }

            logger.Log($"     GridId={gridId} for Expansion='{expansionName}'", LogLevel.Debug);

            //
            //  create a new FarmExpansion object
            //
            //
            //  add custom modData keys to the new expansion,
            //  if they are not there
            //
            if (!expansionLocation.modData.ContainsKey(FEModDataKeys.FELocationName)) expansionLocation.modData.Add(FEModDataKeys.FELocationName, expansionName);
            //
            //  fixup map tilesheet paths
            //
            // does nothing, there is no code in routine
            //FEFramework.FixTileSheetReferences(contentPackService.contentPackLoader.ExpansionMaps[expansionName], expansionLocation, true);
            //
            //  set map properties
            //
            //
            //  allow grass to be grown, might remove this or make it an option
            //
            if (!expansionLocation.map.Properties.ContainsKey("EnableGrassSpread")) expansionLocation.map.Properties.Add("EnableGrassSpread", "T");

            // not used
            //FEFramework.ParseMapProperties(contentPackService.contentPackLoader.ExpansionMaps[expansionName], expansionLocation);
            //
            //  add moddata, if required to any buildings
            //  in the expanssion (for game saves being loaded)
            //

            if (expansionLocation.IsBuildableLocation())
            {

                foreach (Building building in expansionLocation.buildings)
                {
                    if (!building.modData.ContainsKey(FEModDataKeys.FELocationName))
                    {
                        building.modData.Add(FEModDataKeys.FELocationName, expansionName);
                    }
                }
            }
            //
            //  add expansion to game locations
            //
            if (Game1.getLocationFromName(expansionName) == null)
            {
                AddGameLocation(expansionLocation, "ActivateExpansion");
            }
            // add exit blocks
            exitsService.AddMapExitBlockers(expansionLocation.GridId);
            // open exits to adjacent expansions
            gridManager.PatchInMap(gridId);
            //  repair building warps
            RepairAllLocationBuildingWarps(expansionLocation);
            contentPackService.contentPackLoader.ValidContents[expansionName].AddedToFarm = true;
            if (landManager.LandForSale.Contains(expansionName))
            {
                landManager.LandBought(expansionName, false,0);
            }
            else
            {
                ExpDetails[expansionName].Active = true;
            }
            expansionLocation.Active = true;
            contentPackService.contentPackLoader.ValidContents[expansionName].AddedToFarm = true;

            //FEFramework.AfterAppendEvent?.Invoke(oExp, EventArgs.Empty);
            //
            //  if the player is the master player, edit the map
            //
            //if (Game1.IsMultiplayer && Game1.IsMasterGame)
            //{
            //    //SDRMultiplayer.SendActivateExpansions(-1, new List<FarmExpansionLocation> { FEFramework.farmExpansions[sExpansionName] });
            //}
            //}
            //else
            //{
            //    FEFramework.farmExpansions[sExpansionName].Active = false;
            //    logger?.Log($"{sExpansionName} does not have any active rule, so it will not be added", LogLevel.Debug);
            //}




            if (GetActiveFarmExpansions().Count == 1)
            {
                //
                //  first expansion, patch the Backwoods
                //
                utilitiesService.InvalidateCache("Maps/Backwoods");
            }

            //
            //  update world map
            //
            utilitiesService.InvalidateCache("Data/WorldMap");
            //
            //  update locations info
            //

            utilitiesService.InvalidateCache("Data/Locations");
            utilitiesService.InvalidateCache("Data/Minecarts");


            //if (!FEFramework.ActiveFarmProfile.SDEInstalled)
            //    //helper.Content.InvalidateCache("LooseSprites/map");
            //    try
            //    {
            //        helper?.GameContent.InvalidateCache("LooseSprites/map");
            //    }
            //    catch { }

            return true;
        }

        /// <summary>
        /// Returns a list of all active Expansions
        /// </summary>
        /// <returns></returns>
        internal  List<FarmExpansionLocation> GetActiveFarmExpansions()
        {
            return farmExpansions.Where(p => p.Value.Active && p.Value.GridId > -1).Select(q => q.Value).ToList();
        }
        public  string GetExpansionSeasonalOverride(string expansionName)
        {
            //if (!FEFramework.farmExpansions.ContainsKey(expName))
            //{
            //    return "";
            //}
            if (modDataService.CustomDefinitions.ContainsKey(expansionName))
            {
                if (!string.IsNullOrEmpty(modDataService.CustomDefinitions[expansionName].SeasonOverride))
                {
                    //logger.Log($"Using custom season for {Name}", LogLevel.Debug);
                    return modDataService.CustomDefinitions[expansionName].SeasonOverride;
                }
            }

            if (!string.IsNullOrEmpty(contentPackService.contentPackLoader.ValidContents[expansionName].SeasonOverride))
            {
                return contentPackService.contentPackLoader.ValidContents[expansionName].SeasonOverride;
            }
            return "";

        }

        #endregion

        #region "Interal Methods"
        
        //
        //  day started entry point
        //
        internal  void ResetForNewGame(EventArgs e)
        {
            farmExpansions.Clear();
            ExpDetails.Clear();
            //firstDay = true;
        }
        internal  void ResetHorses()
        {
            //
            // reset horses
            //
            var activeExpansions = farmExpansions.Where(p => p.Value.Active).Select(p => p.Value);

            foreach (var expansionLocation in activeExpansions)
            {
                foreach (Building building in expansionLocation.buildings)
                {
                    if (building is Stable && building.daysOfConstructionLeft.Value <= 0)
                    {
                        (building as Stable).grabHorse();
                    }
                }
            }
        }
        internal  void DayStarted(EventArgs e)
        {
            int dayOfMonth = Game1.dayOfMonth;
            //
            //  Check for Activations and For Sales
            //
            //  Reset horses
            //
            //  Add Warproom
            //
            //  Call DayUpdate for Expansions
            //
            logger.Log($"Running ExpansionManager.DayStarted, day: {dayOfMonth}", LogLevel.Debug);

            // do not need to repair building warps on 
            // day start
            //RepairAllExpansionBuildingWarps();
            //
            //  check for activations and new sale candidates
            //
            CheckForActivationsAndForSales();
            // moved to exits service
            //FEFramework.PatchBackwoods();

            //FEFramework.CaveEntrances.Clear();
            //ResetHorses();
            //FEFramework.AddWarpRoom();
            //if (!firstDay)
            //
            //  not required for 1.6
            //
            //DayUpdateForExpansions(day); 

            //firstDay = false;
            //
            //  add warp home
            //
            //Warp? oHomeWarp = FEFramework.GetPlayerHomeWarp();

            //if (oHomeWarp != null)
            //    FEFramework.CaveEntrances.Add(oHomeWarp.TargetName, new Tuple<string, EntranceDetails>(oHomeWarp.TargetName, new EntranceDetails { WarpIn = new EntranceWarp { X = oHomeWarp.X, Y = oHomeWarp.Y }, WarpOut = new EntranceWarp { X = oHomeWarp.X, Y = oHomeWarp.Y } }));

        }
        internal  void RepairAllLocationBuildingWarps(FarmExpansionLocation expansionLocation)
        {
            logger.Log($"    Repairing building warps for location {expansionLocation.NameOrUniqueName}, buildings: {expansionLocation.buildings.Count()}", LogLevel.Debug);
            foreach (Building building in expansionLocation.buildings)
            {
                FixBuildingWarps(building, expansionLocation.NameOrUniqueName);
            }
        }

        /// <summary>
        /// Activates an expansion and make it accessible to the player
        /// </summary>
        /// <param name="expansionName"></param>
        internal  void FixBuildingWarps(Building building, string expansionName)
        {
            if (building.indoors.Value == null)
            {
                logger.Log($"          No indoors for building {expansionName}.{building.indoors.Name}", LogLevel.Debug);
            }
            else
            {
                prism_Building pbld = new prism_Building(building);

                logger.Log($"          Fixing {building.indoors.Value.warps.Count()} warps for {expansionName}.{pbld.NameOfIndoors}", LogLevel.Debug);
                List<Warp> warps = new List<Warp>();
                foreach (Warp warp in building.indoors.Value.warps)
                {
                    if (warp.TargetName.Equals(baseFarmName))
                    {
                        warps.Add(new Warp(warp.X, warp.Y, expansionName, building.humanDoor.X + building.tileX.Value, building.humanDoor.Y + building.tileY.Value + 1, false));
                    }
                }
                building.indoors.Value.warps.Clear();
                building.indoors.Value.warps.AddRange(warps);
            }
        }
        internal  void DayUpdateForExpansions(int day)
        {
            //
            //  not needed for 1.6
            //
            //  cycle through each active expansion and
            //  check for construction and activate Robin
            //  as needed
            //
            //  water all terrainFeature hoedirt
            //

            //foreach (string sKey in FEFramework.farmExpansions.Keys)
            //{
            //    FarmExpansionLocation oBaseExp = FEFramework.farmExpansions[sKey];
            //    if (oBaseExp.Active)
            //    {
            //        FarmExpansionLocation oExp = FEFramework.farmExpansions[sKey];
            //        //PatchMap(oExp, ContentPacks.ValidContents[sKey]);
            //        if (Game1.isRaining)
            //        {
            //            foreach (var feature in oExp.terrainFeatures.Values)
            //            {
            //                if (feature is HoeDirt dirt)
            //                    dirt.state.Value = 1;
            //            }
            //        }
            //        //
            //        //  do building updates
            //        //
            //        //foreach (Building b in FEFramework.farmExpansions[sKey].buildings)
            //        //{
            //        //    b.dayUpdate(day);
            //        //}
            //        //oExp.DayUpdate(day);

            //        //
            //        //  TODO: fix for 1.6
            //        //
            //        //if (Game1.player.currentUpgrade != null)
            //        //    if (oExp.objects.ContainsKey(new Vector2(Game1.player.currentUpgrade.positionOfCarpenter.X / Game1.tileSize, Game1.player.currentUpgrade.positionOfCarpenter.Y / Game1.tileSize)))
            //        //        oExp.objects.Remove(new Vector2(Game1.player.currentUpgrade.positionOfCarpenter.X / Game1.tileSize, Game1.player.currentUpgrade.positionOfCarpenter.Y / Game1.tileSize));


            //        if (oExp.isThereABuildingUnderConstruction() && !Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
            //        {
            //            bool flag2 = false;
            //            foreach (GameLocation location in Game1.locations)
            //            {
            //                if (flag2)
            //                    break;

            //                foreach (NPC npc in location.characters)
            //                {
            //                    if (!npc.Name.Equals("Robin"))
            //                        continue;

            //                    robin = npc;
            //                    npc.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
            //                        {
            //                            new FarmerSprite.AnimationFrame(24, 75),
            //                            new FarmerSprite.AnimationFrame(25, 75),
            //                            new FarmerSprite.AnimationFrame(26, 300, false, false, new AnimatedSprite.endOfAnimationBehavior(robinHammerSound), false),
            //                            new FarmerSprite.AnimationFrame(27, 1000, false, false, new AnimatedSprite.endOfAnimationBehavior(robinVariablePause), false)
            //                        });
            //                    npc.ignoreScheduleToday = true;
            //                    Building buildingUnderConstruction = oExp.getBuildingUnderConstruction();
            //                    if (buildingUnderConstruction.daysUntilUpgrade.Value > 0)
            //                    {
            //                        if (!buildingUnderConstruction.indoors.Value.characters.Contains(npc))
            //                            buildingUnderConstruction.indoors.Value.addCharacter(npc);

            //                        if (npc.currentLocation != null)
            //                            npc.currentLocation.characters.Remove(npc);

            //                        npc.currentLocation = buildingUnderConstruction.indoors.Value;
            //                        npc.setTilePosition(1, 5);
            //                    }
            //                    else
            //                    {
            //                        Game1.warpCharacter(
            //                            npc,
            //                           sKey,
            //                            new Vector2(
            //                                buildingUnderConstruction.tileX.Value + buildingUnderConstruction.tilesWide.Value / 2,
            //                                buildingUnderConstruction.tileY.Value + buildingUnderConstruction.tilesHigh.Value / 2
            //                            ));
            //                        npc.position.X = npc.position.X + Game1.tileSize / 4;
            //                        npc.position.Y = npc.position.Y - Game1.tileSize / 2;
            //                    }
            //                    npc.CurrentDialogue.Clear();

            //                    npc.CurrentDialogue.Push(new Dialogue(npc,Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3926", new object[0])));
            //                    flag2 = true;
            //                    break;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            //
            //            //  TODO: fix for 1.6
            //            //
            //            //oExp.removeCarpenter();
            //        }
            //    }
            //}

        }
        internal  void CheckForActivationsAndForSales()
        {
            //FEFramework.CheckForExpansionActivation("CheckForActivationsAndForSales");

            //
            //  add new activations
            //
            var activeExpansions = farmExpansions.Where(p => p.Value.Active && p.Value.GridId == -1).Select(r => r.Key).ToList();

            foreach (string expansionName in activeExpansions)
            {
                logger.Log($"  Activating new expansion {expansionName}", LogLevel.Debug);
                ActivateExpansion(expansionName);
                farmExpansions[expansionName].DayUpdate(Game1.dayOfMonth);
            }
            //
            //  add lands that can be sold
            //
            activeExpansions = farmExpansions.Where(p => !p.Value.Active).Select(r => r.Key).ToList();

            foreach (string expansionName in activeExpansions)
            {
                landManager.CheckIfCanBePutForSale(expansionName);
            }

        }
        internal  void LoadLocationDefinition(string loadContext)
        {
            //
            //  includes warp room
            //
            bool bIsReady = Context.IsWorldReady;

            logger.Log($"ExpansionManager.LoadLocationDefinition.  Called from {loadContext}", LogLevel.Debug);

 
            gridManager.MapGrid.Clear();
            farmExpansions.Clear();// = new SDict { };
            landManager.LandForSale = new List<string>();
            forSaleSignService.forsaleManager.isShowingForSaleBoard = false;
            //
            //  clean up things
            //
            if (Game1.getLocationFromName(baseFarmName).modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                Game1.getLocationFromName(baseFarmName).modData.Remove(FEModDataKeys.FELocationName);
            }

            foreach (ExpansionPack contentPack in contentPackService.contentPackLoader.ValidContents.Values)
            {
                logger?.Log($"    Adding {contentPack.LocationName} to farmexpansions", LogLevel.Debug);

                ExpansionDetails expansionDetails = new ExpansionDetails(contentPack, false, false);

                string mapPath = $"SDR{FEConstants.AssetDelimiter}Maps{FEConstants.AssetDelimiter}{contentPack.LocationName}";

                string season = GetExpansionSeasonalOverride(contentPack.LocationName);
                //
                //  set any season override
                //
                if (contentPackService.contentPackLoader.ExpansionMaps[contentPack.LocationName].Properties.ContainsKey("SeasonOverride"))
                {
                    contentPackService.contentPackLoader.ExpansionMaps[contentPack.LocationName].Properties.Remove("SeasonOverride");
                }

                if (!string.IsNullOrEmpty(season))
                {
                    contentPackService.contentPackLoader.ExpansionMaps[contentPack.LocationName].Properties.Add("SeasonOverride", season);
                }
                //
                //  set ContextId Override
                //
                if (!string.IsNullOrEmpty(contentPack.LocationContextId))
                {
                    if (contentPackService.contentPackLoader.ExpansionMaps[contentPack.LocationName].Properties.ContainsKey("LocationContext"))
                    {
                        contentPackService.contentPackLoader.ExpansionMaps[contentPack.LocationName].Properties.Remove("LocationContext");
                    }
                    contentPackService.contentPackLoader.ExpansionMaps[contentPack.LocationName].Properties.Add("LocationContext", contentPack.LocationContextId);

                }

                FarmExpansionLocation expansionLocation = new FarmExpansionLocation(contentPackService.contentPackLoader.ExpansionMaps[contentPack.LocationName], mapPath, contentPack.LocationName,(SDVLogger) logger.CustomLogger,this)
                {
                    IsFarm = true,
                    IsOutdoors = true,
                    Active = string.IsNullOrEmpty(contentPack.Requirements) && contentPack.Cost == 0,
                    StockedPonds = contentPack.StockedPonds,
                    DisplayName = contentPack.DisplayName,
                    
                    //locationContextId = contentPack.LocationContextId
                    //SeasonOverride = GetExpansionSeasonalOverride(oContent.LocationName)// oContent.SeasonOverride
                };
                foreach(var bridge in contentPack.suspensionBridges)
                {
                    expansionLocation.suspensionBridges.Add(new StardewValley.BellsAndWhistles.SuspensionBridge(bridge.X, bridge.Y));
                }
                //Traverse.Create(expansionLocation).Field("loadedMapPath").SetValue(expansionLocation.mapPath.Value);
                //GameLocation expansionLocation = new GameLocation( mapPath, contentPack.LocationName)
                //{
                //    IsFarm = true,
                //    IsOutdoors = true,
                //    //Active = string.IsNullOrEmpty(contentPack.Requirements) && contentPack.Cost == 0,
                //    //StockedPonds = contentPack.StockedPonds,
                //    DisplayName = contentPack.DisplayName
                //    //SeasonOverride = GetExpansionSeasonalOverride(oContent.LocationName)// oContent.SeasonOverride
                //};
                //expansionLocation.FishAreas

                //oExp.mapPath.Value = 
                //oExp.loadMap(FEConstants.MapPathPrefix + oContent.LocationName + FEConstants.AssetDelimiter + oContent.MapName, false);
                expansionLocation.map.Description = contentPack.Description;



                //expansionLocation.FishAreas = contentPack.FishAreas;

                if (modDataService.CustomDefinitions.ContainsKey(contentPack.LocationName))
                {
                    expansionDetails.Update(modDataService.CustomDefinitions[contentPack.LocationName]);
                }

                if (expansionDetails?.AlwaysSnowing ?? false)
                    expansionLocation.locationContextId = "prism99.advize.stardewrealty.always_snow";
                else if (expansionDetails?.AlwaysRaining ?? false)
                    expansionLocation.locationContextId = "prism99.advize.stardewrealty.always_rain";
                else if (expansionDetails?.AlwaysSunny ?? false)
                    expansionLocation.locationContextId = "prism99.advize.stardewrealty.always_sunny";
                else
                    expansionLocation.locationContextId = null;
                //
                //  add 1.6 map properties
                //
                if (!expansionLocation.map.Properties.ContainsKey("DefaultWarpLocation"))
                    expansionLocation.map.Properties.Add("DefaultWarpLocation", $"{contentPack.CaveEntrance.WarpOut.X} {contentPack.CaveEntrance.WarpOut.Y}");
                //
                //  set flag fo allowing giant crops
                //
                if (contentPack.AllowGiantCrops && !expansionLocation.map.Properties.ContainsKey("AllowGiantCrops"))
                    expansionLocation.map.Properties.Add("AllowGiantCrops", "T");
                //
                //  set CanBuildHere flag to allow buildings
                //
                if (!expansionLocation.map.Properties.ContainsKey("CanBuildHere"))
                    expansionLocation.map.Properties.Add("CanBuildHere", "T");
                if (!expansionLocation.map.Properties.ContainsKey("LooserBuildRestrictions"))
                    expansionLocation.map.Properties.Add("LooserBuildRestrictions", "T");
                //
                //  set the rate at which diry decays (0-1)(never-always)
                //  Game default is 0.1
                //
                if (!expansionLocation.map.Properties.ContainsKey("GetDirtDecayChance"))
                    expansionLocation.map.Properties.Add("GetDirtDecayChance", contentPack.DirtDecayChance.ToString());

                 expansionLocation.isAlwaysActive.Value = true;
                xTile.Layers.Layer backLayer = expansionLocation.map.GetLayer("Back");
                if (backLayer != null && !backLayer.Tiles[contentPack.CaveEntrance.WarpOut.X, contentPack.CaveEntrance.WarpOut.Y - 1].Properties.ContainsKey("Passable"))
                {
                    backLayer.Tiles[contentPack.CaveEntrance.WarpOut.X, contentPack.CaveEntrance.WarpOut.Y - 1].Properties.Add("Passable", "N");
                }
                if (!expansionLocation.modData.ContainsKey(FEModDataKeys.FEExpansionType))
                    expansionLocation.modData.Add(FEModDataKeys.FEExpansionType, "Expansion");
                if (!expansionLocation.modData.ContainsKey(FEModDataKeys.FELocationName))
                    expansionLocation.modData.Add(FEModDataKeys.FELocationName, expansionLocation.Name);

                //
                //  apply any customizations to the expansion
                //
                if (modDataService.CustomDefinitions.ContainsKey(contentPack.LocationName))
                {
                    CustomizationDetails cd = modDataService.CustomDefinitions[contentPack.LocationName];
                    expansionDetails.Update(cd);
                    if (cd.AllowGiantCrops.HasValue)
                    {
                        if (expansionLocation.map.Properties.ContainsKey("AllowGiantCrops"))
                            expansionLocation.map.Properties.Remove("AllowGiantCrops");

                        if (cd.AllowGiantCrops.Value)
                        {
                            expansionLocation.map.Properties.Add("AllowGiantCrops", "T");
                        }
                    }
                    if (cd.AlwaysSnowing.HasValue)
                    {
                        if (cd.AlwaysSnowing.Value)
                        {
                            expansionLocation.locationContextId = "prism99.advize.stardewrealty.always_snow";
                        }
                        else if (expansionLocation.locationContextId == "prism99.advize.stardewrealty.always_snow")
                        {
                            expansionLocation.locationContextId = "";
                        }
                    }
                    if (cd.AlwaysRaining.HasValue)
                    {
                        if (cd.AlwaysRaining.Value)
                        {
                            expansionLocation.locationContextId = "prism99.advize.stardewrealty.always_rain";
                        }
                        else if (expansionLocation.locationContextId == "prism99.advize.stardewrealty.always_rain")
                        {
                            expansionLocation.locationContextId = "";
                        }
                    }
                    else if (cd.AlwaysSunny.HasValue)
                    {
                        if (cd.AlwaysSunny.Value)
                        {
                            expansionLocation.locationContextId = "prism99.advize.stardewrealty.always_sunny";
                        }
                        else if (expansionLocation.locationContextId == "prism99.advize.stardewrealty.always_sunny")
                        {
                            expansionLocation.locationContextId = "";
                        }
                    }
                    //
                    //  Add custom dirt decay property
                    //
                    if (cd.GetDirtDecayChance.HasValue)
                    {
                        if (expansionLocation.map.Properties.ContainsKey("GetDirtDecayChance"))
                            expansionLocation.map.Properties.Remove("GetDirtDecayChance");

                        expansionLocation.map.Properties.Add("GetDirtDecayChance", cd.GetDirtDecayChance.ToString());

                    }
                }
                //
                //    add bushes
                //
                foreach (var bush in contentPack.Bushes)
                {
                    expansionLocation.largeTerrainFeatures.Add(new Bush(new Vector2(bush.Item1.X, bush.Item1.Y), bush.Item2, expansionLocation));
                }

                expansionLocation.loadObjects();
                farmExpansions.Add(contentPack.LocationName, expansionLocation);
                ExpDetails.Add(contentPack.LocationName, expansionDetails);
            }
            
        }
        #endregion
    }
}
