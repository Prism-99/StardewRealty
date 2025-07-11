using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using StardewModHelpers;
using xTile;
using StardewValley.GameData.Buildings;
using Newtonsoft.Json;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.Buildings;
using SDV_Realty_Core.Framework.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;


namespace SDV_Realty_Core.Framework.CustomEntities.Buildings
{
    internal class CustomBuildingManager
    {
        //
        //  version 1.6
        //
        internal readonly ICollection<BuildingData> FarmBlueprints = new List<BuildingData>();
        internal readonly ICollection<BuildingData> ExpansionBlueprints = new List<BuildingData>();
        internal MapsDataProvider mapDataProvider = null;
        internal Dictionary<string, object> ExternalReferences = new();
        internal Dictionary<string, Map> Maps = new();
        private IModHelper helper;
        private ILoggerService logger;
        private IMonitor monitor;
        private IUtilitiesService utilitiesService;
        private string modRootDirectory;
        private List<ICustomBuilding> buildingDefinitions;
        public readonly Dictionary<string, ICustomBuilding> CustomBuildings = new Dictionary<string, ICustomBuilding>();
        public CustomBuildingManager(ILoggerService logger, IUtilitiesService utilitiesService, IModDataService modDataService)
        {
            this.utilitiesService = utilitiesService;
            monitor = utilitiesService.MonitorService.monitor;
            this.logger = logger;
            helper = utilitiesService.ModHelperService.modHelper;
            Maps = modDataService.BuildingMaps;
        }

        /// <summary>
        /// Load all CustomBuildings bundled with SDR
        /// </summary>
        private void LoadBuildingDefinitions(string modRootDir, ITranslationHelper translations)
        {
            //
            //  get a list of all custom building definitions
            //
            //  stored in directory:  modRootPath\data\assets\buildings\
            //
            //  subdirectory per building: buildingname\building.json
            //
            try
            {
                string buildingRoot = Path.Combine(modRootDir, "data", "assets", "buildings");
                if (Directory.Exists(buildingRoot))
                {
                    string[] buildingJSONs = Directory.GetFiles(buildingRoot, "building.json", SearchOption.AllDirectories);
                    logger.Log($"Found {buildingJSONs.Length} custom building definitions.", LogLevel.Debug);
                    buildingDefinitions = new List<ICustomBuilding> { };

                    // parse found definitions
                    //
                    foreach (string definitionPath in buildingJSONs)
                    {
                        try
                        {
                            //
                            //  check for disabled directories starting with a'.'
                            //
                            if (definitionPath.Replace(modRootDir, "").Split(Path.DirectorySeparatorChar).Where(p => p.StartsWith('.')).Any())
                            {
                                logger.Log($"Directory contains '.', skipping building {Path.GetDirectoryName(definitionPath)}", LogLevel.Info);
                                continue;
                            }
                            string fileContent = File.ReadAllText(definitionPath);
                            GenericBuilding customBuilding = JsonConvert.DeserializeObject<GenericBuilding>(fileContent);
                            customBuilding.SetLogger(logger);
                            customBuilding.translations = translations;
                            customBuilding.ModPath = Path.GetDirectoryName(definitionPath);
                            customBuilding.LoadExternalReferences(utilitiesService.MapLoaderService.Loader, utilitiesService.GameEnvironment.GamePath);
                            AddBuilding(customBuilding);
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error loading building defintion: {ex}", LogLevel.Error);
                        }
                    }
                }
                else
                {
                    logger.Log($"Missing custom Building directory '{buildingRoot}'", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading building defintion: {ex}", LogLevel.Error);
            }
        }
        /// <summary>
        /// Entry point to setup CustomBuilding manager
        /// </summary>
        /// <param name="ologger">Error logger</param>
        /// <param name="ohelper"></param>
        public void LoadDefinitions()
        {
            modRootDirectory = helper.DirectoryPath;
            //
            // load all base custom buildings
            //
            LoadBuildingDefinitions(modRootDirectory, helper.Translation);
            //
            //  add all custom buildings to the ContentHandler
            //  should be empty as all buildings are dynamic in 1.6
            //
            foreach (var buildng in CustomBuildings)
            {
                //
                //  add building texture to content manager
                //
                string assetPath = $"SDR{FEConstants.AssetDelimiter}Buildings{FEConstants.AssetDelimiter}" + buildng.Key;
                ExternalReferences.Add(assetPath, new StardewBitmap(Path.Combine(utilitiesService.GameEnvironment.ModPath, buildng.Value.BuildingTexture())).Texture());
                //
                //  add any building assets
                //
                if (buildng.Value.ExternalReferences != null)
                {
                    foreach (var asset in buildng.Value.ExternalReferences)
                    {
                        ExternalReferences.Add(asset.Key, asset.Value);
                    }
                }
                //
                //  load the building interior map
                //Map buildingMap = buildng.Value.InteriorMap;// MapLoader.LoadMap(SDVEnvironment.GamePath, Path.Combine(modRootDirectory, buildng.Value.MapAssetPath()), buildng.Key, true);
                //
                //  add flag for allow casks
                //
                if (buildng.Value.CanCaskHere && !buildng.Value.InteriorMap.Properties.ContainsKey("CanCaskHere"))
                {
                    buildng.Value.InteriorMap.Properties.Add("CanCaskHere", "Y");
                }
                //
                //  add building map to the content manager
                assetPath = $"Maps{FEConstants.AssetDelimiter}{buildng.Key}{FEConstants.AssetDelimiter}{buildng.Value.InteriorMapName}";
                Maps.Add(assetPath, buildng.Value.InteriorMap);
                logger.Log($"Loaded Building Interior map {assetPath} for building {buildng.Value.Name}", LogLevel.Debug);
                // FEFramework.ContentManager.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}Buildings{FEConstants.AssetDelimiter}{buildng.Key}{FEConstants.AssetDelimiter}{buildng.Value.InteriorMapName}", buildingMap);
            }
            //
            //  v2 dynamic buildings
            //
            foreach (var nBuild in buildingDefinitions)
            {
                logger.Log($"Adding dynamic building {nBuild.Name}", LogLevel.Debug);
                AddBuilding(nBuild);

                string assetPath = $"SDR{FEConstants.AssetDelimiter}Buildings{FEConstants.AssetDelimiter}" + nBuild.Name;
                ExternalReferences.Add(assetPath, new StardewBitmap(nBuild.BuildingTexture()).Texture());
                logger.Log($"Loaded Building texture {assetPath} for building {nBuild.DisplayName}", LogLevel.Debug);

                foreach (var asset in nBuild.ExternalReferences)
                {
                    ExternalReferences.Add(asset.Key, asset.Value);
                    logger.Log($"Loaded Building texture {asset.Key} for building {nBuild.DisplayName}", LogLevel.Debug);
                }

                //Map buildingMap = nBuild.InteriorMap;// MapLoader.LoadMap(SDVEnvironment.GamePath, nBuild.MapAssetPath(), nBuild.Name, false, true, false);
                foreach (var ts in nBuild.InteriorMap.TileSheets)
                {
                    if (ts.ImageSource.StartsWith(nBuild.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        ts.ImageSource = $"SDR{FEConstants.AssetDelimiter}Buildings{FEConstants.AssetDelimiter}" + ts.ImageSource;
                    }
                }
                //
                //  add flag for allow casks
                //
                if (nBuild.CanCaskHere && !nBuild.InteriorMap.Properties.ContainsKey("CanCaskHere"))
                {
                    nBuild.InteriorMap.Properties.Add("CanCaskHere", "Y");
                }
                //
                //  the game is hard coded for building maps in the Maps directory
                //
                assetPath = $"Maps{FEConstants.AssetDelimiter}{nBuild.Name}{FEConstants.AssetDelimiter}{nBuild.InteriorMapName}";
                Maps.Add(assetPath, nBuild.InteriorMap);
                logger.Log($"Loaded Building Interior map {assetPath} for building {nBuild.DisplayName}", LogLevel.Debug);
            }
            utilitiesService.InvalidateCache("Data/Buildings");
        }


        public void AddBuilding(ICustomBuilding building)
        {
            CustomBuildings.Add(building.ID, building);
            if (!string.IsNullOrEmpty(building.DLLToLoad))
            {
                building.LoadDll(building.DLLToLoad, building.ID, monitor, helper);
            }
        }
    }
}
