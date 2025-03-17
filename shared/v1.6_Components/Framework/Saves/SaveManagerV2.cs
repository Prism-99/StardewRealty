using System.Linq;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.Expansions;
using SDict = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation>;
using StardewModdingAPI.Events;
using System.IO;
using System.Xml;
using Prism99_Core.MultiplayerUtils;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System.Xml.Serialization;
using StardewModdingAPI.Enums;
using SDV_Realty_Core.Framework.ServiceProviders.ModMechanics;
using SDV_Realty_Core.Framework.Locations;



namespace SDV_Realty_Core.Framework.Saves
{
    internal class SaveManagerV2
    {
        //
        //  verions 1.6
        //
        private ILoggerService logger;
        private bool ExpansionLoaded = false;
        //
        //  
        //
        private IModHelper helper;
        public XmlSerializer locationSerializer = new XmlSerializer(typeof(SDict));
        //public  XmlSerializer OldlocationSerializer = new XmlSerializer(typeof(SDictOld));
        private IUtilitiesService utilitiesService;
        //private IWarproomService warproomService;
        private IGridManager gridManager;
        private IExpansionManager expansionManager;
        private IExitsService exitsService;
        private IModDataService modDataService;
        private List<GameLocation> savedCustomBuildingsInteriors = new List<GameLocation> { };
        public void Initialize(ILoggerService errorLogger, IUtilitiesService utilitiesService,  IGridManager gridManager, IExpansionManager expansionManager,  IExitsService exitsService, IModDataService modDataService)
        {
            this.utilitiesService = utilitiesService;
            //this.warproomService = warproomService;
            this.gridManager = gridManager;
            this.expansionManager = expansionManager;
            this.exitsService = exitsService;
            this.modDataService = modDataService;
            logger = errorLogger;
            helper = utilitiesService.ModHelperService.modHelper;
        }

        #region EventHandlers
        //
        //  event handlers
        //
        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        public void GameLoop_SaveCreating(EventArgs e)
        {
            logger.Log("GameLoop_SaveCreating", LogLevel.Debug);
            //
            //  Remove expansions from Game1.locations before game save
            //
            PreSave();
        }

        internal void ExitedToTitle(EventArgs e)
        {
            ExpansionLoaded = false;
        }
        internal void Specialized_LoadStageChanged_Step2(EventArgs e)
        {
            LoadStageChangedEventArgs evArgs = (LoadStageChangedEventArgs)e;
            if (evArgs.NewStage == LoadStage.SaveAddedLocations)
            {
                //
                //  expansions have been loaded and added to the map
                //  need to patch map exits
                //
                LoadActiveExpansions();
            }
        }
        /// <summary>
        /// Called after initial locations are loaded
        /// </summary>
        internal void HandlePreload()
        {
            LoadSaveFile("HandlePreload");
        }
        //internal void Specialized_LoadStageChanged(EventArgs e)
        //{
        //    LoadStageChangedEventArgs evArgs = (LoadStageChangedEventArgs)e;
        //    logger.Log($"SaveManager.Specialized_LoadStageChange - {evArgs.NewStage}", LogLevel.Debug);
        //    //
        //    //  called by master and split-screen players
        //    //
        //    if (evArgs.NewStage == LoadStage.SaveAddedLocations)
        //    {
        //        //
        //        //  called by existing save load
        //        //
        //        LoadSaveFile(evArgs.NewStage.ToString());
        //        // moved to ILandManagerService
        //        //FEFramework.CheckForExpansionActivation("SaveManager.LoadStageChanged");
        //    }
        //    else if (evArgs.NewStage == LoadStage.CreatedInitialLocations)
        //    {
        //        //
        //        //  called by new game load
        //        //
        //        LoadSaveFile(evArgs.NewStage.ToString());
        //        // added trigger in ExpansionManager
        //        //expansionManager.expansionManager.ReAddActiveExpansions();
        //    }
        //}

        #endregion
        internal string GetDataDirectoryLocation()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dataDirectory = Path.Combine(appDataPath, "StardewValley", "StardewRealty");

            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
                MigrateSaveFiles(dataDirectory);
            }
            return dataDirectory;
        }
        private void MigrateSaveFiles(string destinationDirectoy)
        {
            //
            //  migrate save data from old save directory (ModDir\pslocationdata)
            //  to new directory (User\AppData\Roaming\StardewValley\destinationDirectory)
            //
            logger.Log($"Migrating Stardew Realty data directory", LogLevel.Info);
            string oldDirectoy = Path.Combine(utilitiesService.ModHelperService.DirectoryPath, "pslocationdata");
            if (Directory.Exists(oldDirectoy))
            {
                //
                //  move save data to new location
                //
                var dir = new DirectoryInfo(oldDirectoy);

                // Get the files in the source directory and copy to the destination directory
                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(destinationDirectoy, file.Name);
                    file.CopyTo(targetFilePath);
                }
            }
        }
        internal void DeleteSave(string saveName)
        {
            //
            //  called when game slot is being deleted
            //  removes all SDR data related to that save
            //  from the filesystem
            //
            if (!string.IsNullOrEmpty(saveName) && saveName != "*" && saveName != "*.*")
            {
                string saveDir = GetDataDirectoryLocation();
                List<string> saveNames = Directory.GetFiles(saveDir, $"*{saveName}.*").ToList();

                if (saveNames.Count < 5)
                {
                    foreach (var name in saveNames)
                    {
                        if (File.Exists(name))
                        {
                            try
                            {
                                File.Delete(name);
                            }
                            catch { }
                        }
                    }
                }
            }
        }
        private bool Load(string filename = null, bool loadOnly = false)
        {
            //
            //  on save loaded, load details of saved expansions
            //
            //  transfer save data to game objects
            //
            //  reload sprites and other objects
            //
            //  reload expansion buildings
            //
            //  re-add expansions to the grid
            //
            logger?.Log("SaveManager.Load called", LogLevel.Debug);

            Save_V1_6 save_V1_6 = new Save_V1_6(logger,utilitiesService, modDataService, helper, expansionManager, gridManager);

            string saveFilename = filename;
            if (string.IsNullOrEmpty(saveFilename))
            {
                saveFilename = save_V1_6.GetSaveFilename(GetDataDirectoryLocation());
            }

            if (save_V1_6.LoadSaveFile(saveFilename, loadOnly))
            {
                SaveLoaded();
                return true;
            }
            else if (save_V1_6.BackupExists(GetDataDirectoryLocation(), out string backupPath))
            {
                if (save_V1_6.LoadSaveFile(backupPath, loadOnly))
                {
                    logger.Log("Backup file used.", LogLevel.Info);
                    SaveLoaded();
                    return true;
                }
            }
            return false;
        }
        private void SaveLoaded()
        {
            //
            //  update world map
            //
            utilitiesService.InvalidateCache("Data/WorldMap");
            //
            //  update locations info
            //

            utilitiesService.InvalidateCache("Data/Locations");
            utilitiesService.InvalidateCache("Data/Minecarts");
        }
        //internal void PreSaveProcessing (EventArgs e)
        //{
        //    PreSave();
        //}
        internal void PostSave()
        {
            expansionManager.expansionManager.ReAddActiveExpansions();
        }
        internal void PreSave()
        {
            //
            //  remove expansions from Game1.locations
            //
            try
            {
                logger.Log("SaveManager.PreSave", LogLevel.Debug);
                SaveFile();
            }
            catch
            { }


            logger.Log($"SaveManager.PreSave. {modDataService.farmExpansions.Count} Expansion records.", LogLevel.Debug);

            savedCustomBuildingsInteriors = new List<GameLocation> { };
            //
            //  remove expansions from Gamelocation so they are not saved
            //  in the vanilla save
            //
            foreach (FarmExpansionLocation expansionLocation in modDataService.farmExpansions.Values)
            {
                try
                {
                    if (Game1.getLocationFromName(expansionLocation.Name) != null)
                    {
                        logger.Log($"    Removing expansion {expansionLocation.Name} from save", LogLevel.Debug);
                        ExpansionPack contentPack = modDataService.validContents[expansionLocation.Name];
                        //BeforeRemoveEvent?.Invoke(oExp, EventArgs.Empty);
                        Game1.locations.Remove(Game1.getLocationFromName(expansionLocation.Name));
                        Game1.removeLocationFromLocationLookup(expansionLocation.Name);

                        //
                        //  need to update sign removal code
                        //
                        //PatchingDetails oPatch = oPack.InsertDetails[oPack.ActiveRule];

                        //foreach (MapEdit oEdit in oPatch.MapEdits)
                        //{
                        //    if (oEdit.SignPostLocation != null && !string.IsNullOrEmpty(oEdit.SignLocationName))
                        //    {
                        //        GameLocation gl = Game1.getLocationFromName(oEdit.SignLocationName);
                        //        if (gl.objects.ContainsKey(oEdit.SignVector()))
                        //        {
                        //            gl.objects.Remove(oEdit.SignVector());
                        //        }
                        //    }
                        //}
                    }
                }
                catch (Exception ex) { logger?.Log($"Error: {ex}", LogLevel.Error); }
            }
            ExpansionLoaded = false;
            //
            //  remove warp room
            //
            Game1.locations.Remove(Game1.getLocationFromName(WarproomManager.StardewMeadowsLoacationName));
            Game1.removeLocationFromLocationLookup(WarproomManager.StardewMeadowsLoacationName);

            utilitiesService.GameEventsService.TriggerEvent(ServiceInterfaces.Events.IGameEventsService.EventTypes.PreSave);
            //warproomService.warproomManager.RemoveWarpRoom();
            //if (Game1.getLocationFromName(FEFramework.WarpRoomLoacationName) != null)
            //{
            //    logger.Log($"   Warproom removed.", LogLevel.Debug);
            //    if (FEFramework.WarpRoom == null)
            //    {
            //        FEFramework.WarpRoom = Game1.getLocationFromName(FEFramework.WarpRoomLoacationName);
            //    }
            //    Game1.locations.Remove(Game1.getLocationFromName(FEFramework.WarpRoomLoacationName));
            //    Game1._locationLookup.Remove(FEFramework.WarpRoomLoacationName);
            //}
            foreach (GameLocation gameLocation in Game1.locations.ToList())
            {
                if (gameLocation is FarmExpansionLocation farmExpansion)
                {
                    logger?.Log($"FarmExpansionLocation {farmExpansion.Name} did not get cleared.", LogLevel.Debug);
                }
                else if (gameLocation is LocationExpansion locationExpansion)
                {
                    logger?.Log($"LocationExpansion {locationExpansion.Name} did not get cleared.", LogLevel.Debug);
                    Game1._locationLookup.Remove(locationExpansion.Name);
                }
                if (gameLocation.modData.ContainsKey(IModDataKeysService.FELocationName))
                {
                    //
                    //  remove expansion buidling interiors
                    //
                    logger.Log($"Removing custom interior {gameLocation.Name}", LogLevel.Debug);
                    Game1._locationLookup.Remove(gameLocation.Name);
                }
            }

        }
        internal void DoDayStartedTasks(EventArgs e)
        {

            logger.Log("Running SM.DoDayStartedTasks", LogLevel.Debug);
            //
            //  reload base farm custom buildings
            //
            LoadBaseFarmData();
        }

        internal void SaveFile()
        {
            Save_V1_6 dataSaver = new Save_V1_6(logger,utilitiesService, modDataService, helper, expansionManager, gridManager);
            if (dataSaver.SaveFile(dataSaver.GetSaveFilename(GetDataDirectoryLocation())))
            {
                logger.Log($"Farm Expansions saved to {dataSaver.GetSaveFilename(GetDataDirectoryLocation())}", LogLevel.Debug);
                return;
            }
            //
            //  save expansion details
            //
            string saveFilename = Path.Combine(GetDataDirectoryLocation(), $"{Constants.SaveFolderName}.xml");
            string saveDirectory = Path.GetDirectoryName(saveFilename);

            if (saveDirectory == null)
                return;

            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            if (File.Exists(saveFilename))
            {
                File.Copy(saveFilename, Path.Combine(saveDirectory, Path.GetFileNameWithoutExtension(saveFilename) + ".txt"), true);
            }

            using (var writer = XmlWriter.Create(saveFilename))
            {
                try
                {
                    locationSerializer.Serialize(writer, modDataService.farmExpansions);
                }
                catch (Exception ex)
                {
                    logger.Log("error: " + ex.ToString(), LogLevel.Error);
                }
            }

            //
            //  save custom buildings on base Farm
            //
            //FarmExpansionLocation baseBuildings = new FarmExpansionLocation();

            //baseBuildings.name.Value = Game1.getFarm().Name;

            //Farm baseFarm = Game1.getFarm();

            //saveFilename = Path.Combine(helper?.DirectoryPath ?? "", "pslocationdata", $"{Game1.getFarm().Name}_{Constants.SaveFolderName}.xml");

            //SDict spLocs = new()
            //{
            //    { Game1.getFarm().Name, baseBuildings }
            //};

            //using (var writer = XmlWriter.Create(saveFilename))
            //{
            //    try
            //    {
            //        locationSerializer.Serialize(writer, spLocs);
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.LogError("SaveManager.Save", ex);
            //        Game1.drawDialogueBox("Error saving expansions:\n" + ex.ToString());
            //    }
            //}

            //FEFramework.addedLocationTracker.Clear();

            //logger.Log($"Farm Expansions saved to {saveFilename}", LogLevel.Debug);
        }
        //internal void SaveCreated(EventArgs e)
        //{
        //    //LoadLocations("SM.SaveCreated");
        //    PostSave();
        //}
        internal bool LoadSaveFile(string loadContext, string saveFilename = null, bool loadOnly = false)
        {
            //
            //  includes warp room
            //
            //  load expansion definitions
            //
            //  load saved expansions
            //
            bool bIsReady = Context.IsWorldReady;

            logger.Log($"    LoadLocations called from {loadContext}", LogLevel.Debug);

            //
            //  load expansion locations
            //  moved to expansionmanager
            //expansionManager.expansionManager.LoadLocationDefinition(loadContext);

            // moved to FEFramework.GameLoop_SaveLoaded
            //SDRMultiplayer.AddHooks(helper)
            string filename = saveFilename;
            if (string.IsNullOrEmpty(filename))
            {
                filename = Path.Combine(GetDataDirectoryLocation(), $"{Constants.SaveFolderName}.xml");
            }
            if (P99Core_MultiPlayer.IsMasterGame && File.Exists(filename))
            {
                //
                //  data exists for this save, load it
                //
                return Load(filename, loadOnly);
            }
            return false;
        }


        internal void LoadBaseFarmData()
        {
            //
            //  read custom buildings on base farm
            //
            string saveFilePath = Path.Combine(GetDataDirectoryLocation(), $"{Game1.getFarm().Name}_{Constants.SaveFolderName}.xml");

            if (File.Exists(saveFilePath))
            {
                SDict baseBuildings = null;

                try
                {
                    using (var reader = XmlReader.Create(saveFilePath))
                    {
                        baseBuildings = (SDict)locationSerializer.Deserialize(reader);
                        if (baseBuildings != null && baseBuildings.ContainsKey(Game1.getFarm().Name))
                        {
                            logger.Log($"Found custom buildings in Farm, {baseBuildings[Game1.getFarm().Name].buildings.Count}", LogLevel.Debug);
                            FarmExpansionLocation tmpLocation = baseBuildings[Game1.getFarm().Name];
                            tmpLocation.LoadBuildings(true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("FEFramework.LoadBaseFarmData", ex);
                }
            }

        }




        //private static void LoadExpansionBuildings(Farm expansionLocation,  string sExpName, bool addToBaseFarm)
        //{

        //    logger?.Log($"LoadingExpansionBuildings for {sExpName}", LogLevel.Debug);
        //    foreach (Building building in expansionLocation.buildings)
        //    {

        //        Vector2 tile = new Vector2((float)building.tileX.Value, (float)building.tileY.Value);

        //        if (building.indoors?.Value is Shed)
        //        {
        //            building.load();
        //            (building.indoors.Value as Shed).furniture.ReplaceWith((expansionLocation.getBuildingAt(tile).indoors.Value as Shed).furniture);
        //            (building.indoors.Value as Shed).wallPaper.Set((expansionLocation.getBuildingAt(tile).indoors.Value as Shed).wallPaper);
        //            (building.indoors.Value as Shed).floor.Set((expansionLocation.getBuildingAt(tile).indoors.Value as Shed).floor);
        //        }
        //        else if (building.indoors?.Value is SlimeHutch hutch)
        //        {
        //            building.load();
        //            for (int i = hutch.characters.Count - 1; i >= 0; i--)
        //            {
        //                if (!hutch.characters[i].DefaultPosition.Equals(Vector2.Zero))
        //                    hutch.characters[i].position.Value = hutch.characters[i].DefaultPosition;

        //                hutch.characters[i].currentLocation = hutch;

        //                if (i < hutch.characters.Count)
        //                    hutch.characters[i].reloadSprite();
        //            }
        //        }
        //        else if (building is JunimoHut jh)
        //        {
        //            jh.load();
        //            if ((int)Traverse.Create(jh).Field("junimoSendOutTimer").GetValue() == 0)
        //            {
        //                Traverse.Create(jh).Field("junimoSendOutTimer").SetValue(1000);
        //            }
        //            jh.shouldSendOutJunimos.Value = true;
        //        }
        //        else if (building.buildingType.Value == "Winery")
        //        {
        //            building.buildingType.Value = "fe.winery";
        //            AddWineryFromSave((Cellar)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, sExpName), building, sExpName);
        //        }
        //        else if (building.buildingType.Value == "fe.winery")
        //        {
        //            AddWineryFromSave((Cellar)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, sExpName), building, sExpName);
        //        }
        //        else if (building.buildingType.Value == "Fromagerie")
        //        {
        //            building.buildingType.Value = "fe.fromagerie";
        //            AddFromagerieFromSave((FromagerieLocation)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, sExpName), building, sExpName);
        //        }
        //        else if (building.buildingType.Value == "fe.fromagerie")
        //        {
        //            AddFromagerieFromSave((FromagerieLocation)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, sExpName), building, sExpName);
        //        }
        //        else if (building.buildingType.Value == "fe.greenhouse")
        //        {
        //            AddGreenhouseFromSave((GreenhouseLocation)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, sExpName), building, sExpName);
        //        }
        //        else if (building.buildingType.Value == "fe.largegreenhouse")
        //        {
        //            AddLargeGreenhouseFromSave((LargeGreenhouseLocation)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, sExpName), building, sExpName);
        //        }
        //        else
        //        {
        //            building.load();
        //            ExpansionManager.FixBuildingWarps(building, sExpName);
        //        }
        //        //
        //        //  if the buidling is on the Base farm, re-add it
        //        //  to the building list
        //        //
        //        if (addToBaseFarm)
        //        {
        //            Game1.getFarm().buildings.Add(building);
        //        }

        //        prism_Building pBuilding = new prism_Building(building);

        //        logger?.Log($"Checking building {pBuilding.NameOfIndoors ?? building.buildingType.Value}", LogLevel.Debug);
        //        logger?.Log($"Building Type {building.buildingType.Value ?? "Unknown"}", LogLevel.Debug);

        //        if (!building.modData.ContainsKey(IModDataKeysService.FELocationName))
        //        {
        //            building.modData.Add(IModDataKeysService.FELocationName, sExpName);
        //        }

        //        //logger?.Log($"Loading Building: {building.nameOfIndoorsWithoutUnique}", LogLevel.Debug);

        //        //SetupCustomBuilding(building, true);

        //        if (building.indoors?.Value != null && building.indoors.Value is AnimalHouse oHouse)
        //        {
        //            if (!oHouse.modData.ContainsKey(IModDataKeysService.FELocationName))
        //            {
        //                oHouse.modData.Add(IModDataKeysService.FELocationName, sExpName);
        //            }
        //            //
        //            // do animal check
        //            //

        //            logger?.Log($"animalsthatlivehere count={oHouse.animalsThatLiveHere.Count}", LogLevel.Debug);

        //            if (building.indoors?.Value != null && building.indoors.Value is AnimalHouse oAnimalHouse)
        //            {
        //                List<long> delIds = new List<long>();
        //                foreach (long iD in oAnimalHouse.animalsThatLiveHere)
        //                {
        //                    if (!oAnimalHouse.animals.ContainsKey(iD))
        //                    {
        //                        if (expansionLocation.animals != null && expansionLocation.animals.ContainsKey(iD))
        //                        {
        //                            oAnimalHouse.animals.Add(iD, expansionLocation.animals[iD]);
        //                            expansionLocation.animals.Remove(iD);
        //                        }
        //                    }
        //                    if (oAnimalHouse.animals.ContainsKey(iD))
        //                    {
        //                        try
        //                        {
        //                            var p = oAnimalHouse.animals[iD].GetBoundingBox();
        //                        }
        //                        catch
        //                        {
        //                            logger.Log($"SM.LoadexpansionBuildings Removing invalid Animal {iD}:{oAnimalHouse.animals[iD].type.Value}", LogLevel.Error);
        //                            delIds.Add(iD);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        delIds.Add(iD);
        //                    }

        //                }
        //                foreach (long iD in delIds)
        //                {
        //                    logger.Log($"Removing Animal {iD}:", LogLevel.Debug);
        //                    oAnimalHouse.animals.Remove(iD);
        //                    building.currentOccupants.Value--;
        //                }
        //            }

        //            logger?.Log($"checking AnimalHouse {oHouse.Name}", LogLevel.Debug);
        //            logger?.Log($"animal count: {oHouse.animals.Count()}", LogLevel.Debug);
        //        }

        //        //if (!addToBaseFarm)
        //        //{
        //        //    FixAnimals(expansionLocation);
        //        //}
        //    }
        //}

        private void LoadActiveExpansions()
        {

            if (ExpansionLoaded)
            {
                logger?.Log("    Skipping restore expansions", LogLevel.Debug);
            }
            else
            {
                logger.Log("     Restoring expansions", LogLevel.Debug);
                bool haveActivations = false;
                foreach (string expansionKey in modDataService.farmExpansions.Keys)
                {
                    FarmExpansionLocation expansionLocation = modDataService.farmExpansions[expansionKey];
                    if (expansionLocation.Active)
                    {
                        haveActivations = true;
                        //
                        //  expansion is active, restore it to the
                        //  game
                        //
                        logger.Log($"     Restoring {expansionLocation.Name}", LogLevel.Debug);
                        if (expansionLocation.GridId == -1)
                        {
                            expansionLocation.GridId = gridManager.AddMapToGrid(expansionLocation.Name, -1);
                        }
                        else
                        {
                            modDataService.MapGrid[expansionLocation.GridId] = expansionLocation.Name;
                        }
                        //  add to Game1.locations
                        expansionManager.expansionManager.AddGameLocation(expansionLocation, "LoadActiveExpansions");
                        // block the 4 exits in the map
                        gridManager.AddMapExitBlockers(expansionLocation.GridId);

                        // return animals to their home
                        for (int animalIndex = expansionLocation.animals.Count() - 1; animalIndex >= 0; animalIndex--)
                        {
                            expansionLocation.animals.Pairs.ElementAt(animalIndex).Value.warpHome();
                        }
                        //  add exit signs
                        //gridManager.AddSign(modDataService.validContents[expansionLocation.Name]);
                        modDataService.expDetails[expansionLocation.Name].Active = true;
                    }
                }
                if (haveActivations)
                {
                    utilitiesService.InvalidateCache("Data/Locations");
                    utilitiesService.InvalidateCache("Data/MineCarts");
                    utilitiesService.InvalidateCache("Data/LocationContexts");

                }
                //
                //  patch in maps (remove required exit blocks)
                //
                foreach (string expansionKey in modDataService.farmExpansions.Where(p => p.Value.Active).OrderBy(p => p.Value.GridId).Select(p => p.Key))
                {
                    gridManager.PatchInMap(modDataService.farmExpansions[expansionKey].GridId);
                }
                ExpansionLoaded = true;
            }
        }
        //internal void SetupForNewDay(EventArgs e)
        //{
        //    //
        //    //  
        //    //
        //    //  called at the begining of a new day
        //    //  to restored all base farm data
        //    //
        //    logger.Log("SaveManager.SetupForNewDay started", LogLevel.Debug);
        //    //
        //    //  restore active expansions
        //    //
        //    //LoadActiveExpansions();
        //    //
        //    //  re-add the warp room
        //    //moved to WarpRoomManager
        //    //warproomService.warproomManager.AddWarpRoom();

        //    //
        //    //  restore custom buildings to the base Farm
        //    //

        //    //LoadBaseFarmData(); 
        //}
    }
}
