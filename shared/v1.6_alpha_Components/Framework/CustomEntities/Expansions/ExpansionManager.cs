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
using StardewModdingAPI.Enums;
using xTile.Layers;
using xTile;
using System.IO;

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
        internal Dictionary<string, ExpansionDetails> ExpDetails;
        internal List<string> addedLocationTracker = new List<string>();
        private IContentPackService contentPackService;
        private IGridManager gridManager;
        private IUtilitiesService utilitiesService;
        private IContentManagerService contentManagerService;
        private ILandManager landManager;
        private IExitsService exitsService;
        private IForSaleSignService forSaleSignService;
        private IModDataService modDataService;

        #region "Public Methods"
        public ExpansionManager(ILoggerService errorLogger, IUtilitiesService utilitiesService, IContentPackService contentPackService, IGridManager gridManager, IContentManagerService contentManagerService, ILandManager landManager, IExitsService exitsService, IForSaleSignService forSaleSignService, IModDataService modDataService)
        {
            this.utilitiesService = utilitiesService;
            logger = errorLogger;
            //helper = utilitiesService.ModHelperService.modHelper;
            this.contentPackService = contentPackService;
            this.gridManager = gridManager;
            this.contentManagerService = contentManagerService;
            this.forSaleSignService = forSaleSignService;
            utilitiesService.GameEventsService.AddSubscription(new SaveCreatingEventArgs(), SaveCreated);
            utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), Specialized_LoadStageChanged, 500);
            utilitiesService.GameEventsService.AddSubscription(new SavedEventArgs(), GameSaved);
            this.landManager = landManager;
            this.exitsService = exitsService;
            this.modDataService = modDataService;

            farmExpansions = modDataService.farmExpansions;
            ExpDetails = modDataService.expDetails;
        }
        private void GameSaved(EventArgs e)
        {
            ReAddActiveExpansions();
        }
        private void Specialized_LoadStageChanged(EventArgs e)
        {
            //
            //  this needs to run before the save file is loaded
            //
            LoadStageChangedEventArgs evArgs = (LoadStageChangedEventArgs)e;
            if (evArgs.NewStage == LoadStage.SaveAddedLocations || evArgs.NewStage == LoadStage.CreatedInitialLocations)
            {
                LoadLocationDefinition("LoadStageChanged");
                //ReAddActiveExpansions();
            }
        }
        private void SaveCreated(EventArgs e)
        {
            addedLocationTracker.Clear();
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
            ExpansionPack oPackToActivate = contentPackService.contentPackLoader.ValidContents[expansionName];
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
        public bool ActivateExpansion(string expansionName, int gridId = -1)
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
                gridId = gridManager.AddMapToGrid(expansionName, gridId);

                if (gridId == -1)
                {
                    farmExpansions[expansionName].Active = false;
                    farmExpansions[expansionName].GridId = gridId;
                    logger.Log($"     Expansion {expansionName} not added, the grid is full.", LogLevel.Warn);
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
            exitsService.AddMapExitBlockers(expansionLocation.GridId);
            // open exits to adjacent expansions
            gridManager.PatchInMap(gridId);
            //  repair building warps
            RepairAllLocationBuildingWarps(expansionLocation);
            contentPackService.contentPackLoader.ValidContents[expansionName].AddedToFarm = true;
            if (landManager.LandForSale.Contains(expansionName))
            {
                landManager.LandBought(expansionName, false, 0);
            }
            else
            {
                ExpDetails[expansionName].Active = true;
            }
            expansionLocation.Active = true;
            contentPackService.contentPackLoader.ValidContents[expansionName].AddedToFarm = true;

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

       /// <summary>
       /// Clears data when Player returns to Title
       /// </summary>
       /// <param name="e"></param>
        internal void ResetForNewGame(EventArgs e)
        {
            farmExpansions.Clear();
            ExpDetails.Clear();
            modDataService.BuildingMaps.Clear();
            modDataService.SubLocations.Clear();
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
        internal void RepairAllLocationBuildingWarps(FarmExpansionLocation expansionLocation)
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
        internal void FixBuildingWarps(Building building, string expansionName)
        {
            if (building.indoors.Value == null)
            {
                logger.Log($"          No indoors for building {expansionName}.{building.indoors.Name}", LogLevel.Debug);
            }
            else
            {
                logger.Log($"          Fixing {building.indoors.Value.warps.Count()} warps for {expansionName}.{building.GetIndoorsName()}", LogLevel.Debug);
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

        internal void CheckForActivationsAndForSales()
        {
            if (utilitiesService.IsMasterGame())
            {
                //
                //  add new activations
                //
                var activeExpansions = farmExpansions.Where(p => p.Value.Active && p.Value.GridId == -1).Select(r => r.Key).ToList();

                foreach (string expansionName in activeExpansions)
                {
                    logger.Log($"  Activating new expansion {expansionName}", LogLevel.Debug);
                    ActivateExpansion(expansionName);
                    //farmExpansions[expansionName].DayUpdate(Game1.dayOfMonth);
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
        }
        internal void LoadLocationDefinition(string loadContext)
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
            if (Game1.getLocationFromName(baseFarmName).modData.ContainsKey(IModDataKeysService.FELocationName))
            {
                Game1.getLocationFromName(baseFarmName).modData.Remove(IModDataKeysService.FELocationName);
            }

            foreach (ExpansionPack contentPack in contentPackService.contentPackLoader.ValidContents.Values)
            {
                try
                {
                    logger?.Log($"    Adding {contentPack.LocationName} to farmexpansions", LogLevel.Debug);

                    ExpansionDetails expansionDetails = new ExpansionDetails(contentPack, false, false);

                    //
                    //  add sub maps
                    //
                    if (contentPack.SubMaps != null)
                    {
                        foreach (string subLocation in contentPack.SubMaps.Keys)
                        {
                            string relPath = Path.Combine(contentPack.ModPath, "assets", contentPack.SubMaps[subLocation]).Replace(utilitiesService.GameEnvironment.GamePath, "");
                            Map subMap = utilitiesService.MapLoaderService.LoadMap(relPath, subLocation, true);
                            //string subMapPath= $"SDR{FEConstants.AssetDelimiter}Maps{FEConstants.AssetDelimiter}{subLocation}";
                            string subMapPath = $"Maps{FEConstants.AssetDelimiter}{subLocation}";
                            //contentManagerService.ExternalReferences.Add(subMapPath,subMap);
                            //
                            //  add map to BuildingMaps so ContentManager can handle the map
                            //  request
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

                    FarmExpansionLocation expansionLocation = new FarmExpansionLocation(contentPackService.contentPackLoader.ExpansionMaps[contentPack.LocationName], mapPath, contentPack.LocationName, (SDVLogger)logger.CustomLogger, this)
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

                    expansionLocation.isAlwaysActive.Value = true;
                    Layer backLayer = expansionLocation.map.GetLayer("Back");
                    if (backLayer != null && !backLayer.Tiles[contentPack.CaveEntrance.WarpOut.X, contentPack.CaveEntrance.WarpOut.Y - 1].Properties.ContainsKey("Passable"))
                    {
                        backLayer.Tiles[contentPack.CaveEntrance.WarpOut.X, contentPack.CaveEntrance.WarpOut.Y - 1].Properties.Add("Passable", "N");
                    }
                    if (!expansionLocation.modData.ContainsKey(IModDataKeysService.FEExpansionType))
                        expansionLocation.modData.Add(IModDataKeysService.FEExpansionType, "Expansion");
                    if (!expansionLocation.modData.ContainsKey(IModDataKeysService.FELocationName))
                        expansionLocation.modData.Add(IModDataKeysService.FELocationName, expansionLocation.Name);

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
                        if (cd.Treasure!=null)
                        {
                            if (expansionLocation.map.Properties.ContainsKey("Treasure"))
                                expansionLocation.map.Properties.Remove("Treasure");

                            if (!string.IsNullOrEmpty( cd.Treasure))
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
                    farmExpansions.Add(contentPack.LocationName, expansionLocation);
                    ExpDetails.Add(contentPack.LocationName, expansionDetails);
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
