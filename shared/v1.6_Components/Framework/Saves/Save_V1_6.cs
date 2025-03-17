using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using Prism99_Core.Extensions;
using Netcode;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SDict = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation>;
using System.Linq;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;
using SDV_Realty_Core.Framework.Locations;
using xTile.Dimensions;


namespace SDV_Realty_Core.Framework.Saves
{

    /// <summary>
    /// Version 1.6 data loader
    /// * handles save/load operations
    /// * upgrades 1.5 files to 1.6
    /// </summary>
    public class Save_V1_6 : ISDRSave
    {
        private static XmlSerializer locationSerializer = new XmlSerializer(typeof(SDict));
        private IExpansionManager expansionManager;
        private IGridManager gridManager;
        private IModDataService modDataService;
        private IUtilitiesService utilitiesService;
        public Save_V1_6() { }
        internal Save_V1_6(ILoggerService sdvLogger, IUtilitiesService utilitiesService, IModDataService modDataService, IModHelper modHelper, IExpansionManager expansionManager, IGridManager gridManager)
        {
            logger = sdvLogger;
            helper = modHelper;
            this.utilitiesService = utilitiesService;
            this.expansionManager = expansionManager;
            this.gridManager = gridManager;
            this.modDataService = modDataService;
            FarmExpansions = new();
            foreach (var activeExpansion in modDataService.farmExpansions.Where(p => p.Value.Active))
            {
                FarmExpansions.Add(activeExpansion.Key, activeExpansion.Value);
            }
            StardewMeadows = WarproomManager.StardewMeadows;// (FarmExpansionLocation) Game1.getLocationFromName(WarproomManager.StardewMeadowsLoacationName);
        }
        public override string Version => "2.0.0";

        public override bool LoadSaveFile(string saveFilePath, bool loadOnly = false)
        {
            logger.Log("SaveLoader 1.6 called", LogLevel.Debug);

            try
            {
                //
                //  verify file exists
                //
                if (!File.Exists(saveFilePath))
                {
                    if (Game1.MasterPlayer?.stats.DaysPlayed > 2)
                    {
                        logger.Log($"Save file {saveFilePath} is missing.", LogLevel.Warn);
                        logger.Log("If you are upgrading to 1.6, did you copy the directory", LogLevel.Warn);
                        logger.Log("\"pslocationdata\" from your old StardewRealty mod directory?", LogLevel.Warn);
                    }
                    return false;
                }
                //
                //  attempt current version load
                //
                using (XmlReader reader = XmlReader.Create(saveFilePath))
                {
                    XmlSerializer saveSerializer = new XmlSerializer(typeof(Save_V1_6));

                    Save_V1_6 saveData = (Save_V1_6)saveSerializer.Deserialize(reader);
                    if (saveData != null)
                    {
                        if (saveData.Version == Version)
                        {
                            LoadedExpansions = new();
                            foreach (var expanansion in saveData.FarmExpansions)
                            {
                                LoadedExpansions.Add(expanansion.Key, expanansion.Value);
                            }
                            ProcessSaveDate(LoadedExpansions, loadOnly);
                            if (saveData.StardewMeadows != null)
                            {
                                FarmExpansionLocation? location =(FarmExpansionLocation) Game1.getLocationFromName(WarproomManager.StardewMeadowsLoacationName);
                                TransferSaveData(saveData.StardewMeadows, location);
                                //saveData.StardewMeadows.mapPath.Value = WarproomManager.StardewMeadowsMapAssetPath;
                                //saveData.StardewMeadows.reloadMap();
                                
                                //if (location != null)
                                //{
                                //    Game1.locations.Remove(location);
                                //    Game1._locationLookup.Remove(location.Name);
                                //}

                                //Game1.locations.Add(saveData.StardewMeadows);
                            }
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                //
                //  check for v1.5 format buildings
                //
                try
                {
                    string fileContent = File.ReadAllText(saveFilePath);

                    bool isV15 = fileContent.Contains("<buildingType>fe.winery</buildingType>");
                    if (!isV15)
                    {
                        isV15 = fileContent.Contains("<buildingType>fe.greenhouse</buildingType>");
                        if (!isV15)
                        {
                            isV15 = fileContent.Contains("<buildingType>fe.fromagerie</buildingType>");
                            if (!isV15)
                            {
                                isV15 = fileContent.Contains("<buildingType>fe.largegreenhouse</buildingType>");
                            }
                        }
                    }
                    if (isV15)
                    {
                        //
                        //  version 1.5 signature found attempt migration
                        //  saves without custom buildings do not need migration
                        //
                        fileContent = fileContent.Replace("fe.winery", "prism99.advize.stardewrealty.winery");
                        fileContent = fileContent.Replace("fe.greenhouse", "prism99.advize.stardewrealty.greenhouse");
                        fileContent = fileContent.Replace("fe.largegreenhouse", "prism99.advize.stardewrealty.largegreenhouse");
                        fileContent = fileContent.Replace("fe.fromagerie", "prism99.advize.stardewrealty.fromagerie");
                        try
                        {
                            logger.Log("Migrating 1.5 save to version 1.6", LogLevel.Info);
                            using (StringReader writer = new StringReader(fileContent))
                            {
                                FarmExpansions = (SDict)locationSerializer.Deserialize(writer);
                                ProcessSaveDate(FarmExpansions, loadOnly);
                                MigrateSaveData();
                                logger.Log("1.5 save migrated to version 1.6 succesfully", LogLevel.Info);
                                return true;
                            }
                        }
                        catch (Exception migrationError)
                        {
                            logger.Log($"Error migrating 1.5 save to 1.6. If you want to file a bug", LogLevel.Error);
                            logger.Log("upload your log to 'https://smapi.io/log' and crete a bug", LogLevel.Error);
                            logger.Log("report on Nexus and include the link to the uploaded log.", LogLevel.Error);
                            logger.LogError("1.5 Migration", migrationError);
                            return false;
                        }
                    }
                }
                catch (Exception textError)
                {
                    logger.Log($"Error loading save text file", LogLevel.Error);
                    logger.LogError("Loader read text file", textError);
                    return false;
                }

                //
                //  try 1.6 alpha/1.5 no custom buildings version
                //
                try
                {
                    using (XmlReader reader = XmlReader.Create(saveFilePath))
                    {
                        FarmExpansions = (SDict)locationSerializer.Deserialize(reader);
                        ProcessSaveDate(FarmExpansions, loadOnly);
                        return true;
                    }
                }
                catch (Exception ex1)
                {
                    logger.Log($"Save file is corrupt", LogLevel.Error);
                    logger.LogError("Loader 1.6 final stage", ex1);
                    return false;
                }
            }
        }
        /// <summary>
        /// Fix any data issues in migration from 1.5 -> 1.6
        /// </summary>
        private void MigrateSaveData()
        {
            var fromagerieAdditions = Game1.buildingData["prism99.advize.stardewrealty.fromagerie"];
            //
            //  check building changes
            //
            foreach (FarmExpansionLocation expansion in FarmExpansions.Values)
            {
                if (expansion.buildings.Any())
                {
                    // check for fromageries, need to add custom cheese vats
                    //
                    var cheeseVats = expansion.buildings.Where(p => p.buildingType.Value == "prism99.advize.stardewrealty.fromagerie");
                    foreach (Building cheese in cheeseVats)
                    {
                        logger.Log($"  Upgrading Fromagerie {expansion.Name} ({cheese.tileX},{cheese.tileY})", LogLevel.Info);
                        foreach (IndoorItemAdd vat in fromagerieAdditions.IndoorItems)
                        {
                            Vector2 vatLocation = new Vector2(vat.Tile.X, vat.Tile.Y);
                            if (!cheese.indoors.Value.objects.ContainsKey(vatLocation))
                            {
                                cheese.indoors.Value.objects.Add(vatLocation, (SDObject)ItemRegistry.Create(vat.ItemId, 1));
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Save current expansion data to a side file
        /// </summary>
        /// <param name="saveFilename">Save filename</param>
        /// <returns></returns>
        public override bool SaveFile(string saveFilename)
        {
            //
            //  save expansion details to side file
            //

            if (File.Exists(saveFilename))
            {
                FileInfo fileInfo = new FileInfo(saveFilename);
                if (fileInfo.Length > 0)
                {
                    //
                    //  do not backup 0 byte file, it is garbage
                    //
                    File.Copy(saveFilename, Path.Combine(Path.GetDirectoryName(saveFilename), Path.GetFileNameWithoutExtension(saveFilename) + ".txt"), true);
                }
            }
            //
            //  serialize to memorystream firts to avoid corrupting save file
            //
            using (MemoryStream ms = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(ms))
                {
                    try
                    {
                        XmlSerializer dataSerializer = new XmlSerializer(typeof(Save_V1_6));
                        Save_V1_6 saveData = new Save_V1_6(logger,utilitiesService, modDataService, helper, expansionManager, gridManager);
                        dataSerializer.Serialize(writer, saveData);
                        // serialization completed, save results to file
                        File.WriteAllBytes(saveFilename, ms.ToArray());

                        //ms.WriteTo(saveFilename);
                        return true;
                    }
                    catch (InvalidOperationException ioe)
                    {
                        logger.Log("Error saving Stardew Realty data file", LogLevel.Error);
                        logger.Log("Your Stardew Realty data is not saved.  The base game save is still valid.", LogLevel.Error);
                        if (ioe.InnerException.InnerException == null)
                        {
                            logger.Log($"Unexpected Type: {ioe.InnerException.Message}", LogLevel.Error);
                        }
                        else
                        {
                            logger.Log($"Unexpected Type: {ioe.InnerException.InnerException.Message}", LogLevel.Error);
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Log("Error saving data file", LogLevel.Error);
                        logger.Log("Your Stardew Realty data is not saved.  The base game save is still valid.", LogLevel.Error);
                        logger.LogError("Save error details", ex);
                    }
                }
            }
            return false;
        }
        private void TransferSaveData(FarmExpansionLocation saveData,FarmExpansionLocation target)
        {
            foreach (var building in saveData.buildings)
            {
                building.load();
                target.buildings.Add(building);
            }
            if (saveData.objects != null) target.objects.ReplaceWith(saveData.objects);

            if (target.objects != null)
            {
                foreach (KeyValuePair<Vector2, SDObject> current in target.objects.Pairs)
                {
                    current.Value.initializeLightSource(current.Key);
                    current.Value.reloadSprite();
                }
            }
            target.furniture.ReplaceWith(saveData.furniture);
            foreach (Furniture furniture in target.furniture)
            {
                furniture.resetState();
                furniture.reloadSprite();
                furniture.InitializeAtTile(furniture.tileLocation.Value);
            }
            if (saveData.terrainFeatures != null) target.terrainFeatures.ReplaceWith(saveData.terrainFeatures);
            target.largeTerrainFeatures.ReplaceWith(saveData.largeTerrainFeatures);
            target.resourceClumps.ReplaceWith(saveData.resourceClumps);

            if (target.terrainFeatures != null)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> terrainFeature in target.terrainFeatures.Pairs)
                    terrainFeature.Value.loadSprite();
            }
        }
        /// <summary>
        /// Reload all required entities in the expansions
        /// </summary>
        /// <param name="saveData">Data to be processed</param>
        private void ProcessSaveDate(SDict saveData, bool loadOnly = false)
        {
            if (saveData != null)
            {
                foreach (string saveExpansionKey in saveData.Keys)
                {
                    try
                    {
                        string saveExpansionName = saveExpansionKey;
                        bool addSaveExpansion = modDataService.validContents.ContainsKey(saveExpansionName);
                        if (!addSaveExpansion)
                        {
                            // check FormerLocationNames
                            var alias = modDataService.validContents.Where(p => p.Value.FormerLocationNames.Contains(saveExpansionName));
                            if (alias.Any())
                            {
                                saveExpansionName = alias.First().Value.LocationName;
                                addSaveExpansion = true;
                            }
                        }


                        if (addSaveExpansion)
                        {
                            logger.Log($"     Setting up '{saveExpansionName}'", LogLevel.Trace);

                            //expansionManager.expansionManager.ActivateExpansion(saveExpansionName, saveData[saveExpansionKey].GridId);

                            if (modDataService.farmExpansions.TryGetValue(saveExpansionKey, out FarmExpansionLocation expansionLocation))
                            {
                                //FarmExpansionLocation expansionLocation = expansionManager.expansionManager.farmExpansions[saveExpansionName];
                                expansionLocation.Active = saveData[saveExpansionKey].Active;
                                expansionLocation.PurchasedBy = saveData[saveExpansionKey].PurchasedBy;
                                if (saveData[saveExpansionKey].animals != null) expansionLocation.animals.CopyFrom(saveData[saveExpansionKey].animals.Pairs);
                                expansionLocation.characters.ReplaceWith(saveData[saveExpansionKey].characters);
                                if (saveData[saveExpansionKey].terrainFeatures != null) expansionLocation.terrainFeatures.ReplaceWith(saveData[saveExpansionKey].terrainFeatures);
                                expansionLocation.largeTerrainFeatures.ReplaceWith(saveData[saveExpansionKey].largeTerrainFeatures);
                                expansionLocation.resourceClumps.ReplaceWith(saveData[saveExpansionKey].resourceClumps);
                                if (saveData[saveExpansionKey].objects != null) expansionLocation.objects.ReplaceWith(saveData[saveExpansionKey].objects);
                                expansionLocation.furniture.ReplaceWith(saveData[saveExpansionKey].furniture);
                                expansionLocation.numberOfSpawnedObjectsOnMap = saveData[saveExpansionKey].numberOfSpawnedObjectsOnMap;
                                expansionLocation.piecesOfHay.Value = saveData[saveExpansionKey].piecesOfHay.Value;
                                expansionLocation.GridId = saveData[saveExpansionKey].GridId;
                                expansionLocation.OfferLetterRead = saveData[saveExpansionKey].OfferLetterRead;
                                expansionLocation.SeasonOverride = expansionManager.expansionManager.GetExpansionSeasonalOverride(saveExpansionName);
                                expansionLocation.CrowsEnabled = saveData[saveExpansionKey].CrowsEnabled;
                                expansionLocation.AllowBlueGrass = saveData[saveExpansionKey].AllowBlueGrass;

                                //  remove auto added buildings as they are stored in the save
                                expansionLocation.buildings.Clear();
                                foreach (Building building in saveData[saveExpansionKey].buildings)
                                {
                                    if (building.indoors.Value == null)
                                    {
                                        try
                                        {
                                            building.load();
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Log($"Error loading building {building.buildingType.Value}", LogLevel.Error);
                                        }
                                    }
                                    else
                                    {
                                        //
                                        //  a kludge to get giant crops transferred from the save
                                        //
                                        //
                                        //  move the clumps to temp storage
                                        //
                                        NetCollection<ResourceClump> resourceClumps = new NetCollection<ResourceClump>();
                                        foreach (ResourceClump oldclump in building.indoors.Value.resourceClumps)
                                        {
                                            resourceClumps.Add(oldclump);
                                        }
                                        //
                                        //  do normal load
                                        //
                                        try
                                        {
                                            building.load();
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Log($"Error loading building {building.buildingType.Value}", LogLevel.Error);
                                        }
                                        //
                                        //  re-add clumps & load
                                        //
                                        foreach (ResourceClump clump in resourceClumps)
                                        {
                                            clump.loadSprite();
                                            building.indoors.Value.resourceClumps.Add(clump);
                                        }
                                    }
                                    utilitiesService.FixBuildingWarps(building, saveExpansionKey);
                                    expansionLocation.buildings.Add(building);
                                }
                                expansionLocation.FixAnimals();

                                expansionLocation.modData.Clear();
                                foreach (string expansionKey in saveData[saveExpansionKey].modData.Keys)
                                {
                                    expansionLocation.modData.Add(expansionKey, saveData[saveExpansionKey].modData[expansionKey]);
                                }

                                for (int characterIndex = expansionLocation.characters.Count - 1; characterIndex >= 0; characterIndex--)
                                {
                                    if (!expansionLocation.characters[characterIndex].DefaultPosition.Equals(Vector2.Zero))
                                        expansionLocation.characters[characterIndex].position.Value = expansionLocation.characters[characterIndex].DefaultPosition;

                                    expansionLocation.characters[characterIndex].currentLocation = expansionLocation;

                                    if (characterIndex < expansionLocation.characters.Count)
                                        expansionLocation.characters[characterIndex].reloadSprite();
                                }

                                if (expansionLocation.terrainFeatures != null)
                                {
                                    foreach (KeyValuePair<Vector2, TerrainFeature> terrainFeature in expansionLocation.terrainFeatures.Pairs)
                                        terrainFeature.Value.loadSprite();
                                }

                                if (expansionLocation.objects != null)
                                {
                                    foreach (KeyValuePair<Vector2, SDObject> current in expansionLocation.objects.Pairs)
                                    {
                                        current.Value.initializeLightSource(current.Key);
                                        current.Value.reloadSprite();
                                    }
                                }

                                foreach (Furniture furniture in expansionLocation.furniture)
                                {
                                    furniture.resetState();
                                    furniture.reloadSprite();
                                }

                                if (modDataService.farmExpansions[saveExpansionName].GridId > -1)
                                {
                                    //
                                    //  re-add expansions to the grid
                                    //
                                    if (modDataService.farmExpansions[saveExpansionName].Active)
                                    {
                                        modDataService.validContents[saveExpansionName].Added = true;
                                        gridManager.AddMapToGrid(saveExpansionName, modDataService.farmExpansions[saveExpansionName].GridId);

                                        //
                                        //  refactor sublocations that have Farm warps
                                        //
                                        foreach (string location in modDataService.validContents[saveExpansionName].SubLocations)
                                        {
                                            GameLocation subLocation = Game1.getLocationFromName(location);
                                            if (subLocation != null)
                                            {
                                                utilitiesService.MapUtilities.AdjustMapActions(subLocation.Map, modDataService.validContents[saveExpansionName].LocationName);

                                                List<Warp> warpsToRemove = new();
                                                if (subLocation != null)
                                                {
                                                    foreach (var warp in subLocation.warps)
                                                    {
                                                        if (warp.TargetName == "Farm")
                                                        {
                                                            warp.TargetName = modDataService.validContents[saveExpansionName].LocationName;
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
                                        }


                                    }
                                    else
                                    {
                                        modDataService.farmExpansions[saveExpansionName].GridId = -1;
                                    }
                                }
                                //if (!loadOnly)
                                //expansionManager.expansionManager.AddGameLocation(expansionLocation, "Save_V1_6");
                            }
                            else
                            {
                                logger.Log($"   Save contains unknown expansion '{saveExpansionName}'.  Expansion ignored.", LogLevel.Warn);
                            }
                        }
                        else
                        {
                            logger.Log($"   Save contains unknown expansion '{saveExpansionName}'.  Expansion ignored.", LogLevel.Warn);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Error loading Expansion {saveExpansionKey}", LogLevel.Error);
                        logger.LogError(ex);
                    }
                }
                //
                //  now things are loaded, re-add active expansions to the grid
                //
                //logger.Log($"     Adding active expansions to map grid.", LogLevel.Debug);
                //foreach (string saveExpansionName in saveData.Keys)
                //{
                //    //
                //    //  added to support FormerLocationNames
                //    //
                //    bool addToGrid;
                //    string currentName = saveExpansionName;
                //    addToGrid = expansionManager.expansionManager.farmExpansions.ContainsKey(saveExpansionName);

                //    if (!addToGrid)
                //    {
                //        // check FormerLocationNames
                //        var alias = contentPackService.contentPackLoader.ValidContents.Where(p => p.Value.FormerLocationNames.Contains(saveExpansionName));
                //        if (alias.Any())
                //        {
                //            currentName = alias.First().Value.LocationName;
                //            addToGrid = true;
                //        }
                //    }
                //    if (addToGrid)
                //    {
                //        if (expansionManager.expansionManager.farmExpansions[currentName].GridId > -1)
                //        {
                //            //
                //            //  re-add expansions to the grid
                //            //
                //            if (expansionManager.expansionManager.farmExpansions[currentName].Active)
                //            {
                //                gridManager.AddMapToGrid(currentName, expansionManager.expansionManager.farmExpansions[currentName].GridId);
                //            }
                //            else
                //            {
                //                expansionManager.expansionManager.farmExpansions[currentName].GridId = -1;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        logger.Log($"Save contains missing Expansion '{saveExpansionName}'",LogLevel.Warn);
                //    }
                //}
                //
                //  fix grid layout in case any expansion mods had
                //  been removed.
                //
                gridManager.FixGrid();
            }
        }
    }
}