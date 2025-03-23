using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.Objects;
using System.Collections.Generic;
using System.Linq;
using Prism99_Core.Utilities;
using SDict = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation>;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using StardewRealty.SDV_Realty_Interface;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using StardewModdingAPI.Events;
using System;
using xTile.Layers;
using xTile;
using System.IO;
using Microsoft.Xna.Framework.Content;
using StardewValley.GameData.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Integrations;


namespace SDV_Realty_Core.Framework.Expansions
{
    internal class ExpansionManager
    {
        //
        //  version 1.6
        //
        private static ILoggerService logger;
        //private static IModHelper helper;
        private const string baseFarmName = "Farm";
        internal SDict farmExpansions;
        internal SDict activeExpansions = new();
        internal Dictionary<string, ExpansionDetails> ExpDetails;
        internal Dictionary<string, LocationData> LocationDataCache = new();
        //private IContentPackService contentPackService;
        private IGridManager gridManager;
        private IUtilitiesService utilitiesService;
        private IContentManagerService contentManagerService;
        private ILandManager landManager;
        //private IExitsService exitsService;
        private IForSaleSignService forSaleSignService;
        private IModDataService modDataService;
        private ILocationTunerIntegrationService locationTunerIntegrationService;
        #region "Public Methods"
        public ExpansionManager(ILoggerService errorLogger, IUtilitiesService utilitiesService,
            IGridManager gridManager, IContentManagerService contentManagerService,
            ILandManager landManager, 
            IForSaleSignService forSaleSignService, IModDataService modDataService,
            ILocationTunerIntegrationService locationTunerIntegrationService)
        {
            this.utilitiesService = utilitiesService;
            logger = errorLogger;
            //helper = utilitiesService.ModHelperService.modHelper;
            //this.contentPackService = contentPackService;
            this.gridManager = gridManager;
            this.contentManagerService = contentManagerService;
            this.forSaleSignService = forSaleSignService;
            utilitiesService.GameEventsService.AddSubscription(new SaveCreatingEventArgs(), SaveCreated);
            //utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), Specialized_LoadStageChanged, 500);
            //utilitiesService.GameEventsService.AddSubscription(new SavedEventArgs(), GameSaved);


            this.landManager = landManager;
            //this.exitsService = exitsService;
            this.modDataService = modDataService;
            this.locationTunerIntegrationService = locationTunerIntegrationService;

            farmExpansions = modDataService.farmExpansions;
            ExpDetails = modDataService.expDetails;
        }
        public void HandleGameSaved()
        {
            ReAddActiveExpansions();
            CheckForActivationsAndForSales();
        }
        //private void GameSaved(EventArgs e)
        //{
        //    HandleGameSaved();
        //}
        internal void HandlePreLoadEvent()
        {
            LoadLocationDefinitions("HandlePreLoad");
        }
        //private void Specialized_LoadStageChanged(EventArgs e)
        //{
        //    //
        //    //  this needs to run before the save file is loaded
        //    //
        //    LoadStageChangedEventArgs evArgs = (LoadStageChangedEventArgs)e;
        //    if (evArgs.NewStage == LoadStage.SaveAddedLocations || evArgs.NewStage == LoadStage.CreatedInitialLocations)
        //    //if (evArgs.NewStage==LoadStage.CreatedBasicInfo)
        //    {
        //        LoadLocationDefinitions("LoadStageChanged");
        //        // moved to GameSaved
        //        //CheckForActivationsAndForSales();
        //        //ReAddActiveExpansions();
        //    }
        //}
        private void SaveCreated(EventArgs e)
        {
            modDataService.addedLocationTracker.Clear();
        }
        public void ReAddActiveExpansions()
        {
            //
            //  re-add active expansions to Game1.locations,
            //  if game is single player or multiplayer master
            //
            if ((Game1.IsMultiplayer && Game1.IsMasterGame) || !Game1.IsMultiplayer)
            {
                logger.Log($"ExpansionManager - ReAddActiveExpansions", LogLevel.Debug);
                List<string> activeExpansions = farmExpansions.Where(p => p.Value.Active && p.Value.GridId > -1).OrderBy(q => q.Value.GridId).Select(r => r.Key).ToList();

                foreach (string expansionName in activeExpansions)
                {
                    AddGameLocation(farmExpansions[expansionName], "ReAddActiveExpansions");
                }
            }
        }
        internal void AddGameLocation(GameLocation gl, string caller)
        {
            logger.Log($"AddGameLocation.{caller}: {gl.Name}:{gl.GetSeason()}", LogLevel.Debug);

            if (modDataService.addedLocationTracker.Contains(gl.Name))
            {
                logger.Log($"     re-adding {gl.Name}", LogLevel.Debug);
            }

            if (Game1.getLocationFromName(gl.Name) == null || Game1.currentLocation?.Name == gl.Name)
            {
                Game1.locations.Add(gl);
                logger.Log($"     added {gl.Name}", LogLevel.Debug);
                modDataService.addedLocationTracker.Add(gl.Name);
            }
            else
            {
                logger.Log($"     location already in Game1.locations: {gl.Name}", LogLevel.Debug);
            }
        }
        public void ActivateExpansionOnRemote(string expansionName, int gridId)
        {
            //
            //  server takes care of patching the map
            //
            //
            FarmExpansionLocation expansionLocation = farmExpansions[expansionName];
            //  add custom MapStrings, if defined
            //
            //  MapStrings are handled by the AssetEditor class
            //
            ExpansionPack oPackToActivate = modDataService.validContents[expansionName];
            if (oPackToActivate?.MapStrings != null)
            {
                foreach (string key in oPackToActivate.MapStrings.Keys)
                {
                    logger.Log($"     Adding map string {key}.{oPackToActivate.MapStrings[key]}", LogLevel.Debug);

                    contentManagerService.contentManager.stringFromMaps.Add(key, oPackToActivate.MapStrings[key]);
                }
            }
            // not sure if needed
            if (expansionLocation.modData.ContainsKey(IModDataKeysService.FEGridId))
            {
                expansionLocation.modData[IModDataKeysService.FEGridId] = gridId.ToString();
            }
            else
            {
                expansionLocation.modData.Add(IModDataKeysService.FEGridId, gridId.ToString());
            }
            if (farmExpansions[expansionName].modData.ContainsKey(IModDataKeysService.FEGridId))
            {
                farmExpansions[expansionName].modData[IModDataKeysService.FEGridId] = gridId.ToString();
            }
            else
            {
                farmExpansions[expansionName].modData.Add(IModDataKeysService.FEGridId, gridId.ToString());
            }
            // end of not sure
            if (Game1.getLocationFromName(expansionName) == null)
            {
                AddGameLocation(expansionLocation, "RemoteActivateExpansion");
            }
        }
        public bool ActivateExpansion(string expansionName, int gridId = -1, bool addToGrid = true)
        {
            logger.Log($"    Activating: {expansionName}, Grid Id={gridId}", LogLevel.Debug);


            if (!modDataService.validContents[expansionName].AddedToFarm)
            {
                // load expansion
                LoadExpansionDetails(expansionName);

            }
            //
            //  get the grid location, if auto mapped
            //
            bool bAutoAdd = farmExpansions[expansionName].AutoAdd;

            FarmExpansionLocation expansionLocation = farmExpansions[expansionName];
            FarmExpansionLocation expansionToAdd = (FarmExpansionLocation)farmExpansions[expansionName].Clone();
            expansionLocation.baseExpansionName = expansionName;

            ExpansionPack expansionPackToActivate = modDataService.validContents[expansionName];

            if (addToGrid)
            {
                //  
                //  get a grid id for the new expansion
                //
                gridId = gridManager.AddMapToGrid(expansionName, gridId);

                if (gridId == -1)
                {
                    farmExpansions[expansionName].Active = false;
                    farmExpansions[expansionName].GridId = gridId;
                    logger.Log($"     Expansion {expansionName} not added, the grid is full.", LogLevel.Info);
                    return false;
                }
                else
                {
                    farmExpansions[expansionName].GridId = gridId;
                    expansionLocation.GridId = gridId;
                }
                logger.Log($"     GridId={gridId} for Expansion='{expansionName}'", LogLevel.Debug);
            }
            //
            //  add custom buildings
            //
            if (!farmExpansions[expansionName].BaseBuildingsAdded)
            {
                if (expansionPackToActivate.StaterBuildings != null)
                {
                    foreach (ExpansionPack.StarterBuilding baseBuilding in expansionPackToActivate.StaterBuildings)
                    {
                        Building b = new Building(baseBuilding.BuldingType, baseBuilding.Location);
                        b.FinishConstruction();
                        b.LoadFromBuildingData(b.GetData(), forUpgrade: false, forConstruction: true);
                        // add animals
                        if (baseBuilding.AnimalsToAdd != null)
                        {
                            foreach (string animal in baseBuilding.AnimalsToAdd)
                            {
                                FarmAnimal starterAnimal = new FarmAnimal(animal, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
                                starterAnimal.Name = Dialogue.randomName();
                                (b.GetIndoors() as AnimalHouse).adoptAnimal(starterAnimal);
                            }
                        }

                        farmExpansions[expansionName].buildings.Add(b);
                    }
                }

                farmExpansions[expansionName].BaseBuildingsAdded = true;
            }
            //
            //  refactor sublocations that have Farm warps
            //
            foreach (string location in expansionPackToActivate.SubLocations)
            {
                GameLocation subLocation = Game1.getLocationFromName(location);
                utilitiesService.MapUtilities.AdjustMapActions(subLocation.Map, expansionPackToActivate.LocationName);

                List<Warp> warpsToRemove = new();
                if (subLocation != null)
                {
                    foreach (var warp in subLocation.warps)
                    {
                        if (warp.TargetName == "Farm")
                        {
                            warp.TargetName = expansionPackToActivate.LocationName;
                        }
                        //  remove Backwoods warps
                        if (warp.TargetName == "Backwoods")
                        {
                            warpsToRemove.Add(warp);
                        }
                    }
                    foreach (var oldWarp in warpsToRemove)
                    {
                        subLocation.warps.Remove(oldWarp);
                    }
                }
            }

            if (expansionPackToActivate.InternalMineCarts != null && expansionPackToActivate.InternalMineCarts.Destinations.Any())
            {
                if (!modDataService.CustomMineCartNetworks.ContainsKey(expansionName))
                {
                    modDataService.CustomMineCartNetworks.Add(expansionName, expansionPackToActivate.InternalMineCarts);
                    utilitiesService.InvalidateCache("Data/Minecarts");
                }
            }

            //
            //  add custom MapStrings, if defined
            //
            //  MapStrings are handled by the AssetEditor class
            //
            if (expansionPackToActivate?.MapStrings != null)
            {
                foreach (string key in expansionPackToActivate.MapStrings.Keys)
                {
                    logger.Log($"     Adding map string {key}.{expansionPackToActivate.MapStrings[key]}", LogLevel.Debug);

                    contentManagerService.contentManager.stringFromMaps.Add(key, expansionPackToActivate.MapStrings[key]);
                }
            }

            if (expansionLocation.modData.ContainsKey(IModDataKeysService.FEGridId))
            {
                expansionLocation.modData[IModDataKeysService.FEGridId] = gridId.ToString();
            }
            else
            {
                expansionLocation.modData.Add(IModDataKeysService.FEGridId, gridId.ToString());
            }
            if (farmExpansions[expansionName].modData.ContainsKey(IModDataKeysService.FEGridId))
            {
                farmExpansions[expansionName].modData[IModDataKeysService.FEGridId] = gridId.ToString();
            }
            else
            {
                farmExpansions[expansionName].modData.Add(IModDataKeysService.FEGridId, gridId.ToString());
            }

            //
            //  create a new FarmExpansion object
            //
            //
            //  add custom modData keys to the new expansion,
            //  if they are not there
            //
            if (!expansionLocation.modData.ContainsKey(IModDataKeysService.FELocationName)) expansionLocation.modData.Add(IModDataKeysService.FELocationName, expansionName);
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
            //if (!expansionLocation.map.Properties.ContainsKey("EnableGrassSpread")) expansionLocation.map.Properties.Add("EnableGrassSpread", "T");

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
                    if (!building.modData.ContainsKey(IModDataKeysService.FELocationName))
                    {
                        building.modData.Add(IModDataKeysService.FELocationName, expansionName);
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
            gridManager.AddMapExitBlockers(expansionLocation.GridId);
            // open exits to adjacent expansions
            gridManager.PatchInMap(gridId);
            //  repair building warps
            utilitiesService.RepairAllLocationBuildingWarps(expansionLocation);
            if (modDataService.LandForSale.Contains(expansionName))
            {
                landManager.LandBought(expansionName, false, 0,0);
            }
            else
            {
                ExpDetails[expansionName].Active = true;
            }
            modDataService.validContents[expansionName].Added = true;
            expansionLocation.Active = true;
            modDataService.validContents[expansionName].AddedToFarm = true;

            //
            //  if the player is the master player, edit the map
            //
            if (Game1.IsMultiplayer && Game1.IsMasterGame)
            {
                utilitiesService.CustomEventsService.TriggerCustomEvent("SendLandBought", new object[] { expansionName, expansionLocation.GridId });
            }

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

            utilitiesService.CustomEventsService.TriggerModEvent(ICustomEventsService.ModEvents.ExpansionActivated, new object[] { expansionName });

            return true;
        }

        /// <summary>
        /// Returns a list of all active Expansions
        /// </summary>
        /// <returns></returns>
        internal List<FarmExpansionLocation> GetActiveFarmExpansions()
        {
            return farmExpansions.Where(p => p.Value.Active && p.Value.GridId > -1).Select(q => q.Value).ToList();
        }
        public string GetExpansionSeasonalOverride(string expansionName)
        {
            if (modDataService.CustomDefinitions.TryGetValue(expansionName, out CustomizationDetails? custom) && !string.IsNullOrEmpty(custom.SeasonOverride))
                return custom.SeasonOverride;

            if (!string.IsNullOrEmpty(modDataService.validContents[expansionName].SeasonOverride))
                return modDataService.validContents[expansionName].SeasonOverride;

            if (modDataService.farmExpansions.TryGetValue(expansionName, out FarmExpansionLocation? farmExpansion))
                return farmExpansion.GetSeasonOverride();

            return "";
        }

        #endregion

        #region "Internal Methods"

        /// <summary>
        /// Clears data when Player returns to Title
        /// </summary>
        /// <param name="e"></param>
        internal void ResetForNewGame(EventArgs e)
        {
            farmExpansions.Clear();
            //foreach (var expansion in farmExpansions.Values)
            //    expansion.Active = false;

            ExpDetails.Clear();
            LocationDataCache.Clear();
            utilitiesService.InvalidateCache("Data/Locations");
            //modDataService.BuildingMaps.Clear();
        }
        internal void ResetHorses()
        {
            //
            // reset horses
            //
            var activeExpansions = farmExpansions.Where(p => p.Value.Active).Select(p => p.Value);

            foreach (FarmExpansionLocation expansionLocation in activeExpansions)
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
        internal void DayStarted(EventArgs e)
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
       

        /// <summary>
        /// Activates an expansion and make it accessible to the player
        /// </summary>
        /// <param name="expansionName"></param>
        

        internal void CheckForActivationsAndForSales()
        {
            List<string> toActivate = new List<string>();

            if (utilitiesService.IsMasterGame())
            {
                //
                //  add new activations
                //
                var activeExpansions = farmExpansions.Where(p => p.Value.Active && p.Value.GridId == -1).Select(r => r.Key).ToList();

                foreach (string expansionName in activeExpansions)
                {
                    logger.Log($"  Activating new expansion {expansionName}", LogLevel.Debug);
                    toActivate.Add(expansionName);
                    //ActivateExpansion(expansionName);
                    //farmExpansions[expansionName].DayUpdate(Game1.dayOfMonth);
                }
                //  activate freebies in random order
                Random rnd = new Random(Game1.ticks);
                while (toActivate.Count > 0 && modDataService.farmExpansions.Count()<modDataService.MaximumExpansions)
                {
                    int selected = rnd.Next(toActivate.Count);
#if DEBUG
                    logger.Log($"Activating For Sale Queue #{selected}", LogLevel.Debug);
#endif
                    ActivateExpansion(toActivate[selected]);
                    toActivate.Remove(toActivate[selected]);
                }
                //
                //  add lands that can be sold
                //
                //activeExpansions = farmExpansions.Where(p => !p.Value.Active).Select(r => r.Key).ToList();
                activeExpansions = modDataService.validContents.Where(p => !p.Value.Added).Select(r => r.Key).ToList();

                foreach (string expansionName in activeExpansions)
                {
                    landManager.CheckIfCanBePutForSale(expansionName);
                }
            }
        }
        internal void AddLocationDataDefinitionToCache(FarmExpansionLocation location)
        {
            try
            {
                Random rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Minute);
                ExpansionPack contentPack = modDataService.validContents[location.Name];
                bool createNewData = true;
                LocationData locationData = null;

                if (contentPack.isAdditionalFarm)
                {
                    createNewData = !Game1.locationData.TryGetValue($"Farm_{contentPack.BaseLocationName}", out locationData);
                }

                if (createNewData)
                {
                    ExpansionDetails expansionDetails = modDataService.expDetails[location.Name];

                    locationData = new LocationData
                    {
                        DisplayName = location.DisplayName,
                        ExcludeFromNpcPathfinding = true,
                        FishAreas = new Dictionary<string, FishAreaData>(),
                        ArtifactSpots = new List<ArtifactSpotDropData> { },
                        CanPlantHere = true,
                        CanHaveGreenRainSpawns = contentPack.CanHaveGreenRainSpawns,
                        ChanceForClay = contentPack.ChanceForClay.HasValue ? contentPack.ChanceForClay.Value : Math.Round((float)rand.Next(1, 5) / 100f, 2),
                        FormerLocationNames = contentPack.FormerLocationNames,
                        CreateOnLoad = new CreateLocationData
                        {
                            AlwaysActive = true,
                            MapPath = $"SDR{FEConstants.AssetDelimiter}Maps{FEConstants.AssetDelimiter}{utilitiesService.RemoveMapExtensions(contentPack.MapName)}"
                        }
                    };
                    //
                    //  set weed and forage data
                    //
                    locationData.MinDailyWeeds = contentPack.MinDailyWeeds.HasValue ? contentPack.MinDailyWeeds.Value : rand.Next(1, 5);
                    locationData.MaxDailyWeeds = contentPack.MaxDailyWeeds.HasValue ? contentPack.MaxDailyWeeds.Value : rand.Next(locationData.MinDailyWeeds, 14);
                    locationData.FirstDayWeedMultiplier = contentPack.FirstDayWeedMultiplier.HasValue ? contentPack.FirstDayWeedMultiplier.Value : rand.Next(7, 12);
                    locationData.MinDailyForageSpawn = contentPack.MinDailyForageSpawn.HasValue ? contentPack.MinDailyForageSpawn.Value : rand.Next(3, 7);
                    locationData.MaxDailyForageSpawn = contentPack.MaxDailyForageSpawn.HasValue ? contentPack.MaxDailyForageSpawn.Value : rand.Next(locationData.MinDailyForageSpawn, 12);

                    if (contentPack.CaveEntrance != null)
                    {
                        locationData.DefaultArrivalTile = new Point(contentPack.CaveEntrance.WarpIn.X, contentPack.CaveEntrance.WarpIn.Y);
                    }
                    //
                    //  add artifact details
                    //
                    if (expansionDetails.Artifacts != null && expansionDetails.Artifacts.Count > 0)
                    {
                        foreach (ArtifactData arti in expansionDetails.Artifacts)
                        {
                            ArtifactSpotDropData artifactData = new ArtifactSpotDropData
                            {
                                ItemId = arti.ArtifactId,
                                Chance = arti.Chance
                            };
                            if (!string.IsNullOrEmpty(arti.Season) && arti.Season != "Any")
                            {
                                //
                                //  add seasonal condition
                                //
                                artifactData.Condition = $"LOCATION_SEASON Here {arti.Season}";
                            }

                            locationData.ArtifactSpots.Add(artifactData);
                        }
                    }
                    //
                    //  add fishing details
                    //
                    if (contentPack.FishAreas != null)
                    {
                        foreach (string fishAreaKey in contentPack.FishAreas.Keys)
                        {
                            locationData.FishAreas.Add(fishAreaKey, FishAreaDetails.GetData(contentPack.FishAreas[fishAreaKey]));
                            if (expansionDetails.FishAreas != null && expansionDetails.FishAreas.ContainsKey(fishAreaKey))
                            {
                                foreach (FishStockData stockData in expansionDetails.FishAreas[fishAreaKey].StockData)
                                {
                                    Season s;
                                    SDVUtilities.TryParseEnum(stockData.Season, out s);
                                    locationData.Fish.Add(new SpawnFishData
                                    {
                                        FishAreaId = fishAreaKey,
                                        ItemId = stockData.FishId,
                                        IgnoreFishDataRequirements = true,
                                        Season = s
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    locationData.DisplayName = contentPack.DisplayName;
                    if (locationData.CreateOnLoad == null)
                    {
                        locationData.CreateOnLoad = new CreateLocationData
                        {
                            AlwaysActive = true,
                            MapPath = $"Maps/{contentPack.MapName}"
                        };
                    }
                }
                if (locationData != null)
                    LocationDataCache[location.Name] = locationData;

            }
            catch (Exception ex)
            {
                logger.LogError("AddLocationDataDefinitionCache", ex);
            }

        }
        public bool LoadExpansionDetails(string expansionName)
        {
            if (modDataService.farmExpansions.ContainsKey(expansionName))
                return true;

            if (modDataService.validContents.TryGetValue(expansionName, out ExpansionPack contentPack))
            {
                //
                //  need to load expansion map
                //
                //if (contentPackService.contentPackLoader.LoadExpansionMap(expansionName))
                //{
                //    //contentManagerService.contentManager.AddExpansionFiles(expansionName);

                //contentPackService.contentPackLoader.lo
                ExpansionDetails expansionDetails = new ExpansionDetails(contentPack, false, false);

                if (contentPack.SubMaps != null)
                {
                    foreach (string subLocation in contentPack.SubMaps.Keys)
                    {
                        string relPath = Path.Combine(contentPack.ModPath, "assets", contentPack.SubMaps[subLocation]).Replace(utilitiesService.GameEnvironment.GamePath, "");
                        Map subMap = utilitiesService.MapLoaderService.LoadMap(relPath, subLocation, true);
                        string subMapPath = $"Maps{FEConstants.AssetDelimiter}{subLocation}";
                        //
                        //  add map to BuildingMaps so ContentManager can handle the map
                        //  request
                        if (!modDataService.BuildingMaps.ContainsKey(subMapPath))
                            modDataService.BuildingMaps.Add(subMapPath, subMap);

                        GameLocation subGameLocation = new GameLocation(subMapPath, subLocation);
                        subGameLocation.IsGreenhouse = subMap.Properties.TryGetValue("IsGreenhouse", out var _);
                        //
                        //  add location to SubLocations so LocationData handler can
                        //  add LocationData
                        //
                        modDataService.SubLocations.Add(subLocation, subGameLocation);

                        AddGameLocation(subGameLocation, "AddSubMaps");
                    }
                }

                string mapPath = $"SDR{FEConstants.AssetDelimiter}Maps{FEConstants.AssetDelimiter}{utilitiesService.GetMapUniqueName(contentPack)}";
                //string mapPath = $"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{contentPack.LocationName}";

                string season = GetExpansionSeasonalOverride(contentPack.LocationName);
                //
                //  set any season override
                //
                string mapName = utilitiesService.GetMapUniqueName(contentPack);
                if (modDataService.ExpansionMaps[mapName].Properties.ContainsKey("SeasonOverride"))
                {
                    modDataService.ExpansionMaps[mapName].Properties.Remove("SeasonOverride");
                }

                if (!string.IsNullOrEmpty(season))
                {
                    modDataService.ExpansionMaps[mapName].Properties.Add("SeasonOverride", season);
                }
                //
                //  set ContextId Override
                //
                if (!string.IsNullOrEmpty(contentPack.LocationContextId))
                {
                    modDataService.ExpansionMaps[mapName].Properties["LocationContext"] = contentPack.LocationContextId;
                }

                FarmExpansionLocation expansionLocation = new FarmExpansionLocation(modDataService.ExpansionMaps[mapName], mapPath, contentPack.LocationName, (SDVLogger)logger.CustomLogger, expansionDetails, utilitiesService, modDataService, locationTunerIntegrationService)
                {
                    IsFarm = true,
                    IsOutdoors = true,
                    StockedPonds = contentPack.StockedPonds,
                    DisplayName = contentPack.DisplayName,

                    //locationContextId = contentPack.LocationContextId
                    //SeasonOverride = GetExpansionSeasonalOverride(oContent.LocationName)// oContent.SeasonOverride
                };


                foreach (Point bridge in contentPack.suspensionBridges)
                {
                    expansionLocation.suspensionBridges.Add(new StardewValley.BellsAndWhistles.SuspensionBridge(bridge.X, bridge.Y));
                }

                expansionLocation.map.Description = contentPack.Description;

                //
                //  add 1.6 map properties
                //
                if (!expansionLocation.map.Properties.ContainsKey("DefaultWarpLocation"))
                    expansionLocation.map.Properties.Add("DefaultWarpLocation", $"{contentPack.CaveEntrance.WarpOut.X} {contentPack.CaveEntrance.WarpOut.Y}");
                //
                //  set flag for allowing giant crops
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

                Layer backLayer = expansionLocation.map.GetLayer("Back");
                if (backLayer != null)
                {
                    if (backLayer.Tiles[contentPack.CaveEntrance.WarpOut.X, contentPack.CaveEntrance.WarpOut.Y - 1] == null)
                    {
                        // cave entrance does not exist
                        logger.Log($"Cave entrance does not exist for {contentPack.DisplayName}", LogLevel.Info);
                    }
                    else if (!backLayer.Tiles[contentPack.CaveEntrance.WarpOut.X, contentPack.CaveEntrance.WarpOut.Y - 1].Properties.ContainsKey("Passable"))
                    {
                        backLayer.Tiles[contentPack.CaveEntrance.WarpOut.X, contentPack.CaveEntrance.WarpOut.Y - 1].Properties.Add("Passable", "N");
                    }
                }

                if (contentPack.AllowGrassGrowInWinter)
                    expansionLocation.map.Properties["AllowGrassGrowInWinter"] = "T";
                if (contentPack.AllowGrassSurviveInWinter)
                    expansionLocation.map.Properties["AllowGrassSurviveInWinter"] = "T";
                if (contentPack.EnableGrassSpread)
                    expansionLocation.map.Properties["EnableGrassSpread"] = "T";
                if (contentPack.skipWeedGrowth)
                    expansionLocation.map.Properties["skipWeedGrowth"] = "T";
                if (contentPack.SpawnGrassFromPathsOnNewYear)
                    expansionLocation.map.Properties["SpawnGrassFromPathsOnNewYear"] = "T";
                if (contentPack.SpawnRandomGrassOnNewYear)
                    expansionLocation.map.Properties["SpawnRandomGrassOnNewYear"] = "T";
                if (!string.IsNullOrEmpty(contentPack.Treasure))
                    expansionLocation.map.Properties["Treasure"] = contentPack.Treasure;

                expansionLocation.AllowBlueGrass = contentPack.EnableBlueGrass;

                //
                //  apply any customizations to the expansion
                //
                if (modDataService.CustomDefinitions.TryGetValue(contentPack.LocationName, out CustomizationDetails cd))
                {
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
                        expansionLocation.map.Properties["GetDirtDecayChance"] = cd.GetDirtDecayChance.ToString();
                    }
                    if (cd.AllowGrassGrowInWinter.HasValue)
                    {
                        if (expansionLocation.map.Properties.ContainsKey("AllowGrassGrowInWinter"))
                            expansionLocation.map.Properties.Remove("AllowGrassGrowInWinter");

                        if (cd.AllowGrassGrowInWinter.Value)
                            expansionLocation.map.Properties.Add("AllowGrassGrowInWinter", "T");

                    }
                    if (cd.AllowGrassSurviveInWinter.HasValue)
                    {
                        if (expansionLocation.map.Properties.ContainsKey("AllowGrassSurviveInWinter"))
                            expansionLocation.map.Properties.Remove("AllowGrassSurviveInWinter");

                        if (cd.AllowGrassSurviveInWinter.Value)
                            expansionLocation.map.Properties.Add("AllowGrassSurviveInWinter", "T");

                    }
                    if (cd.EnableGrassSpread.HasValue)
                    {
                        if (expansionLocation.map.Properties.ContainsKey("EnableGrassSpread"))
                            expansionLocation.map.Properties.Remove("EnableGrassSpread");

                        if (cd.EnableGrassSpread.Value)
                            expansionLocation.map.Properties.Add("EnableGrassSpread", "T");

                    }
                    if (cd.skipWeedGrowth.HasValue)
                    {
                        if (expansionLocation.map.Properties.ContainsKey("skipWeedGrowth"))
                            expansionLocation.map.Properties.Remove("skipWeedGrowth");

                        if (cd.skipWeedGrowth.Value)
                            expansionLocation.map.Properties.Add("skipWeedGrowth", "T");

                    }
                    if (cd.SpawnGrassFromPathsOnNewYear.HasValue)
                    {
                        if (expansionLocation.map.Properties.ContainsKey("SpawnGrassFromPathsOnNewYear"))
                            expansionLocation.map.Properties.Remove("SpawnGrassFromPathsOnNewYear");

                        if (cd.SpawnGrassFromPathsOnNewYear.Value)
                            expansionLocation.map.Properties.Add("SpawnGrassFromPathsOnNewYear", "T");

                    }
                    if (cd.SpawnRandomGrassOnNewYear.HasValue)
                    {
                        if (expansionLocation.map.Properties.ContainsKey("SpawnRandomGrassOnNewYear"))
                            expansionLocation.map.Properties.Remove("SpawnRandomGrassOnNewYear");

                        if (cd.SpawnRandomGrassOnNewYear.Value)
                            expansionLocation.map.Properties.Add("SpawnRandomGrassOnNewYear", "T");

                    }
                    if (cd.Treasure != null)
                    {
                        if (expansionLocation.map.Properties.ContainsKey("Treasure"))
                            expansionLocation.map.Properties.Remove("Treasure");

                        if (!string.IsNullOrEmpty(cd.Treasure))
                            expansionLocation.map.Properties.Add("Treasure", cd.Treasure);

                    }
                }
                //
                //    add bushes
                //
                foreach (Tuple<Point, int> bush in contentPack.Bushes)
                {
                    expansionLocation.largeTerrainFeatures.Add(new Bush(new Vector2(bush.Item1.X, bush.Item1.Y), bush.Item2, expansionLocation));
                }

                expansionLocation.loadObjects();

                if (farmExpansions.ContainsKey(contentPack.LocationName))
                {
                    logger.Log($"LoadExpansionDetails duplication location name '{contentPack.LocationName}'", LogLevel.Error);
                }
                else
                {
                    farmExpansions.Add(contentPack.LocationName, expansionLocation);
                    ExpDetails.Add(contentPack.LocationName, expansionDetails);
                    AddLocationDataDefinitionToCache(expansionLocation);
                }
                return true;
            }

            return false;
        }
        internal void LoadLocationDefinitions(string loadContext)
        {
            //
            //  includes warp room
            //
            bool bIsReady = Context.IsWorldReady;

            logger.Log($"ExpansionManager.LoadLocationDefinition.  Called from {loadContext}", LogLevel.Debug);

            modDataService.MapGrid.Clear();
            farmExpansions.Clear();
            modDataService.LandForSale.Clear();
            forSaleSignService.forsaleManager.isShowingForSaleBoard = false;
            //
            //  clean up things
            //
            if (Game1.getLocationFromName(baseFarmName).modData.ContainsKey(IModDataKeysService.FELocationName))
            {
                Game1.getLocationFromName(baseFarmName).modData.Remove(IModDataKeysService.FELocationName);
            }

            foreach (ExpansionPack contentPack in modDataService.validContents.Values)
            {
                try
                {
                    logger?.Log($"    Adding {contentPack.LocationName} to farmexpansions", LogLevel.Debug);

                    LoadExpansionDetails(contentPack.LocationName);
                }
                catch (Exception fnf) when (fnf is FileNotFoundException || fnf is ContentLoadException)
                {
                    logger.Log($"LoadLocationDefintion could not find file '{fnf.InnerException.Message}'", LogLevel.Error);
                }
                catch (Exception ex)
                {
                    logger.Log($"Error loading expansion {contentPack.LocationName}", LogLevel.Error);
                    logger.LogError("Error", ex);
                }
            }
        }
        #endregion
    }
}
