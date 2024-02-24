using Prism99_Core.Utilities;
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
        public Save_V1_6() { }
        internal Save_V1_6(SDVLogger sdvLogger, IModHelper modHelper,IExpansionManager expansionManager, IGridManager gridManager)
        {
            logger = sdvLogger;
            helper = modHelper;
            this.expansionManager = expansionManager;
            this.gridManager = gridManager;
        }
        public override string Version => "2.0.0";

        public override bool LoadSaveFile(string saveFilePath)
        {
            logger.Log("SaveLoader 1.6 called", LogLevel.Debug);

            try
            {
                //
                //  verify file exists
                //
                if (!File.Exists(saveFilePath))
                {
                    if (Game1.MasterPlayer.stats.DaysPlayed > 2)
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
                using (var reader = XmlReader.Create(saveFilePath))
                {
                    XmlSerializer saveSerializer = new XmlSerializer(typeof(Save_V1_6));

                    Save_V1_6 saveData = (Save_V1_6)saveSerializer.Deserialize(reader);
                    if (saveData != null)
                    {
                        if (saveData.Version == Version)
                        {
                            FarmExpansions = saveData.FarmExpansions;
                            ProcessSaveDate(FarmExpansions);
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch
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
                                ProcessSaveDate(FarmExpansions);
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
                    using (var reader = XmlReader.Create(saveFilePath))
                    {
                        FarmExpansions = (SDict)locationSerializer.Deserialize(reader);
                        ProcessSaveDate(FarmExpansions);
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
            foreach (var expansion in FarmExpansions.Values)
            {
                if (expansion.buildings.Any())
                {
                    // check for fromageries, need to add custom cheese vats
                    //
                    var cheeseVats = expansion.buildings.Where(p => p.buildingType.Value == "prism99.advize.stardewrealty.fromagerie");
                    foreach (var cheese in cheeseVats)
                    {
                        logger.Log($"  Upgrading Fromagerie {expansion.Name} ({cheese.tileX},{cheese.tileY})", LogLevel.Info);
                        foreach (var vat in fromagerieAdditions.IndoorItems)
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

            using (var writer = XmlWriter.Create(saveFilename))
            {
                try
                {
                    XmlSerializer dataSerializer = new XmlSerializer(typeof(Save_V1_6));
                    Save_V1_6 saveData = new Save_V1_6(logger, helper,expansionManager,gridManager)
                    {
                        FarmExpansions = expansionManager.expansionManager.farmExpansions
                    };
                    dataSerializer.Serialize(writer, saveData);
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Log("Error saving data file", LogLevel.Error);
                    logger.LogError("Save error details", ex);
                }
            }
            return false;
        }

        /// <summary>
        /// Reload all required entities in the expansions
        /// </summary>
        /// <param name="saveData">Data to be processed</param>
        private void ProcessSaveDate(SDict saveData)
        {
            if (saveData != null)
            {
                foreach (string expansionName in saveData.Keys)
                {
                    logger.Log($"     Setting up '{expansionName}'", LogLevel.Trace);
                    if (expansionManager.expansionManager.farmExpansions.ContainsKey(expansionName))
                    {
                        FarmExpansionLocation expansionLocation = expansionManager.expansionManager.farmExpansions[expansionName];
                        expansionLocation.Active = saveData[expansionName].Active;
                        expansionLocation.PurchasedBy = saveData[expansionName].PurchasedBy;
                        if (saveData[expansionName].animals != null) expansionLocation.animals.CopyFrom(saveData[expansionName].animals.Pairs);
                        expansionLocation.characters.ReplaceWith(saveData[expansionName].characters);
                        if (saveData[expansionName].terrainFeatures != null) expansionLocation.terrainFeatures.ReplaceWith(saveData[expansionName].terrainFeatures);
                        expansionLocation.largeTerrainFeatures.ReplaceWith(saveData[expansionName].largeTerrainFeatures);
                        expansionLocation.resourceClumps.ReplaceWith(saveData[expansionName].resourceClumps);
                        if (saveData[expansionName].objects != null) expansionLocation.objects.ReplaceWith(saveData[expansionName].objects);
                        expansionLocation.furniture.ReplaceWith(saveData[expansionName].furniture);
                        expansionLocation.numberOfSpawnedObjectsOnMap = saveData[expansionName].numberOfSpawnedObjectsOnMap;
                        expansionLocation.piecesOfHay.Value = saveData[expansionName].piecesOfHay.Value;
                        expansionLocation.GridId = saveData[expansionName].GridId;
                        expansionLocation.OfferLetterRead = saveData[expansionName].OfferLetterRead;
                        expansionLocation.SeasonOverride = expansionManager.expansionManager.GetExpansionSeasonalOverride(expansionName);
                        expansionLocation.CrowsEnabled = saveData[expansionName].CrowsEnabled;

                        foreach (var building in saveData[expansionName].buildings)
                        {
                            if (building.indoors.Value == null)
                            {
                                building.load();
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
                                foreach (var oldclump in building.indoors.Value.resourceClumps)
                                {
                                    resourceClumps.Add(oldclump);
                                }
                                //
                                //  do normal load
                                //
                                building.load();
                                //
                                //  re-add clumps & load
                                //
                                foreach (var clump in resourceClumps)
                                {
                                    clump.loadSprite();
                                    building.indoors.Value.resourceClumps.Add(clump);
                                }
                            }
                            expansionManager.expansionManager.FixBuildingWarps(building, expansionName);
                            expansionLocation.buildings.Add(building);
                        }
                        expansionLocation.modData.Clear();
                        foreach (string expansionKey in saveData[expansionName].modData.Keys)
                        {
                            expansionLocation.modData.Add(expansionKey, saveData[expansionName].modData[expansionKey]);
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
                    }
                    else
                    {
                        logger.Log($"   Save contains unknown expansion '{expansionName}'.  Expansion ignored.", LogLevel.Warn);
                    }
                }
                //
                //  now things are loaded, re-add active expansions to the grid
                //
                logger.Log($"     Adding active expansions to map grid.", LogLevel.Debug);
                foreach (string expansionName in saveData.Keys)
                {
                    if (expansionManager.expansionManager.farmExpansions.ContainsKey(expansionName))
                    {
                        if (expansionManager.expansionManager.farmExpansions[expansionName].GridId > -1)
                        {
                            //
                            //  re-add expansions to the grid
                            //
                            if (expansionManager.expansionManager.farmExpansions[expansionName].Active)
                            {
                                gridManager.AddMapToGrid(expansionName);
                            }
                            else
                            {
                                expansionManager.expansionManager.farmExpansions[expansionName].GridId = -1;
                            }
                        }
                    }
                }
                //
                //  fix grid layout in case any expansion mods had
                //  been removed.
                //
                gridManager.FixGrid();
            }
        }
    }
}