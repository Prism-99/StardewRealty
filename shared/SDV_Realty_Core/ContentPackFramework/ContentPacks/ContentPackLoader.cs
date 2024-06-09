using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewRealty.SDV_Realty_Interface;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.Buildings;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using SDV_Realty_Core.Framework.CustomEntities.Crops;
using SDV_Realty_Core.Framework.CustomEntities.LocationContexts;
using SDV_Realty_Core.Framework.CustomEntities.Machines;
using SDV_Realty_Core.Framework.CustomEntities.Movies;
using SDV_Realty_Core.Framework.CustomEntities.Objects;
using xTile.Format;
using xTile.Layers;
using System.Text.Json;
using Newtonsoft.Json;
using xTile.Dimensions;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks
{
    /// <summary>
    /// Loads any SDR content packs
    /// </summary>
    internal class ContentPackLoader
    {
        public ILoggerService logger { get; }
        public List<ExpansionPack> Contents { get; }
        private ICustomEntitiesServices customEntitiesServices;
        private IUtilitiesService _utilitiesService;
        private IAutoMapperService _autoMapperService;
        private IGameEnvironmentService gameEnvironmentService;


        public Dictionary<string, Map> ExpansionMaps;
        public Dictionary<string, ExpansionPack> ValidContents { get; }
        public ContentPackLoader() { }
        public ContentPackLoader(ILoggerService olog, IUtilitiesService utilitiesService, ICustomEntitiesServices customEntitiesServices, IModDataService modDataService, IAutoMapperService autoMapperService, IGameEnvironmentService gameEnvironmentService)
        {
            logger = olog;
            Contents = new List<ExpansionPack>();
            ValidContents = modDataService.validContents;
            ExpansionMaps = modDataService.ExpansionMaps;
            _utilitiesService = utilitiesService;
            _autoMapperService = autoMapperService;
            this.customEntitiesServices = customEntitiesServices;
            this.gameEnvironmentService = gameEnvironmentService;
        }

        public ContentPackLoader(ILoggerService olog, IModHelper helper)
        {
            logger = olog;
            Contents = new List<ExpansionPack>();
            ValidContents = new Dictionary<string, ExpansionPack> { };
        }
        private void PrintContentPackListSummary()
        {
            logger.Log($"Loaded {ValidContents.Count} content packs:", LogLevel.Info);
            ValidContents.Select(c => c.Value.Owner.Manifest)
                .ToList()
                .ForEach(
                    (m) => logger.Log($"   {m.Name} {m.Version} by {m.Author} ({m.UniqueID})", LogLevel.Info));
        }
        private void Prepare(ISDRContentPack contentPack)
        {
            switch (contentPack)
            {
                case ExpansionPack content:

                    if (!string.IsNullOrEmpty(content.ThumbnailName))
                    {
                        try
                        {
                            FileStream filestream = new FileStream(Path.Combine(content.ModPath, "assets", content.ThumbnailName), FileMode.Open);
                            content.Thumbnail = Texture2D.FromStream(Game1.graphics.GraphicsDevice, filestream);
                        }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(content.ForSaleImageName))
                    {
                        try
                        {
                            FileStream filestream = new FileStream(Path.Combine(content.ModPath, "assets", content.ForSaleImageName), FileMode.Open);
                            content.ForSaleImage = Texture2D.FromStream(Game1.graphics.GraphicsDevice, filestream);
                        }
                        catch { }
                    }
                    if (content.FishAreas == null || content.FishAreas.Count == 0)
                    {
                        content.FishAreas = new Dictionary<string, FishAreaDetails>
                        {
                            {"default",new FishAreaDetails{  DisplayName="Default",Id="default"} }
                        };
                    }
                    break;
            }
        }
        public void LoadPacks(IEnumerable<IContentPack> contentPacks)
        {
            //
            //  get a list of content packs
            //
            List<string> contentPackNames = new List<string>
            {
                "expansion.json", "bigcraftable.json","building.json",
                "object.json","machine.json","machinedata.json", "crop.json",
                "locationcontext.json"
            };

            List<ISDRContentPack> contentPackTypes = new List<ISDRContentPack>
            {
                new CustomBigCraftableData(), new ExpansionPack(),
                new GenericBuilding(), new CustomObjectData(),
                new CustomMachineData(), new CustomCropData(),
                new CustomLocationContext(), new CustomMachine(),
                new CustomMovieData()
            };

            List<ISDRContentPack> foundPacks = new List<ISDRContentPack>();
            foreach (IContentPack contentPack in contentPacks)
            {
                foreach (ISDRContentPack packType in contentPackTypes)
                {
                    string[] packs = Directory.GetFiles(contentPack.DirectoryPath, packType.PackFileName, SearchOption.AllDirectories);
                    foreach (string pack in packs)
                    {
                        //
                        //  check for disabled directories
                        //
                        string[] pathParts = pack.Split(Path.DirectorySeparatorChar);
                        if (pathParts.Where(p => p.StartsWith(".")).Any())
                        {
                            //disabled
                            continue;
                        }
                        try
                        {
                            ISDRContentPack newPack = packType.ReadContentPack(pack);
                            newPack.translations = contentPack.Translation;
                            newPack.ModPath = Path.GetDirectoryName(pack);// contentPack.DirectoryPath;
                            newPack.Owner = contentPack;
                            newPack.SetLogger(logger);
                            logger.Log($"Loaded content pack {newPack.Owner.Manifest.Name}.{newPack.GetType().Name}", LogLevel.Debug, false);
                            foundPacks.Add(newPack);
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error loading content pack {pack}", LogLevel.Error);
                            logger.Log($"Error:  {ex}", LogLevel.Error);
                        }
                    }
                }
            }
            foreach (ISDRContentPack newPack in foundPacks)
            {
                try
                {
                    switch (newPack)
                    {
                        case ExpansionPack farmExp:
                            Prepare(farmExp);
                            if (ValidateContentPack(farmExp))
                                ValidContents.Add(farmExp.LocationName, farmExp);

                            break;
                        case GenericBuilding building:
                            building.LoadExternalReferences(_utilitiesService.MapLoaderService.Loader, gameEnvironmentService.GamePath);
                            customEntitiesServices.customBuildingService.customBuildingManager.AddBuilding(building);
                            break;
                        case CustomObjectData customObjectData:
                            customEntitiesServices.customObjectService.AddObjectDefinition(customObjectData);
                            break;
                        case CustomBigCraftableData customBigCraftableData:
                            customEntitiesServices.customBigCraftableService.customBigCraftableManager.AddBigCraftable(customBigCraftableData);
                            break;
                        case CustomMachineData machineData:
                            customEntitiesServices.customMachineDataService.AddMachine(machineData);
                            break;
                        case CustomCropData cropData:
                            customEntitiesServices.customCropService.AddCropDefinition(cropData);
                            break;
                        case CustomMachine machine:
                            //CustomMachineManager.AddMachine(machine);
                            break;
                        case CustomMovieData movieData:
                            customEntitiesServices.customMovieService.AddMovieDefinition(movieData.MovieId, movieData);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"Error adding pack: {Path.Combine(newPack.ModPath, newPack.PackFileName)}", LogLevel.Error);
                    logger.Log($"Error: {ex}", LogLevel.Error);
                }
            }
        }
        public bool ValidateContentPack(ISDRContentPack content)
        {
            bool isValid = false;

            switch (content)
            {
                case ExpansionPack content2:

                    isValid = !string.IsNullOrEmpty(content2.MapName);
                    if (isValid)
                    {
                        //isValid = content.Owner.Manifest.Version.MajorVersion >= 1 && content.Owner.Manifest.Version.MinorVersion >= 2;

                        //if (!isValid) logger.Log($"Content Pack '{content.LocationName}' is not at least 1.2.", LogLevel.Info);
                    }
                    else
                    {
                        logger.Log($"Content Pack '{content2.LocationName}' does not have a map file defined.", LogLevel.Info);
                    }
                    break;
                case CustomBigCraftableData data2:
                    isValid = true;
                    break;
                case ICustomBuilding building:
                    isValid = true;
                    break;
                case CustomObjectData data:
                    isValid = true;
                    break;
                case CustomMachineData machine:
                    isValid = true;
                    break;
            }
            return isValid;
        }
        public bool LoadMaps()
        {

            //
            //  preload maps
            //
            ExpansionMaps.Clear();
            bool bHaveActive = false;
            FormatManager formatManager = FormatManager.Instance;


            List<string> lFailedPacks = new List<string> { };
            foreach (string sPackName in ValidContents.Keys.ToList())
            {
                ExpansionPack oPack = ValidContents[sPackName];
                Map mNewMap=null;
                try
                {
                    if (oPack.LoadMap)
                    {
                        bool mapIsValid = true;
                        if (oPack.IsBaseFarm)
                        {
                            //
                            //  converting base farm into expansion farm
                            //
                            try
                            {
                                mNewMap = Game1.content.Load<Map>($"Maps/{oPack.BaseFarmMap}");
                                RemoveBaseFarmProperties(mNewMap, oPack.BasePropertiesToKeep);
                                if (oPack.BaseMapEdits != null)
                                {
                                    foreach (var baseEdit in oPack.BaseMapEdits)
                                    {
                                        Map sourceMap = oPack.Owner.ModContent.Load<Map>($"assets/{baseEdit.PatchMapName}");
                                        _utilitiesService.MapUtilities.PatchInMap(mNewMap, sourceMap, baseEdit.PatchPoint);
                                    }
                                }
                                //var locationData = GameLocation.GetData(oPack.BaseLocationName);
                                //GameLocation baseLocation = Game1.getLocationFromName();
                                //if (locationData != null)
                                //{
                                //    oPack.FishAreas = new Dictionary<string, FishAreaDetails>();
                                //    foreach (var fishArea in locationData.FishAreas)
                                //    {
                                //        oPack.FishAreas.Add(fishArea.Key, new FishAreaDetails(fishArea.Value));
                                //    }
                                //    oPack.Artifacts = new List<ArtifactData>();
                                //    if (locationData.ArtifactSpots != null)
                                //    {
                                //        foreach(var spot in locationData.ArtifactSpots)
                                //        {
                                //            oPack.Artifacts.Add(new ArtifactData {Chance= spot.Chance, ArtifactId=spot.Id,Season=spot.se });
                                //        }
                                //    }                                
                                //}
                            }
                            catch(Exception err)
                            {
                                logger.Log($"Error loading baseMap", LogLevel.Error);
                                logger.Log($"{err}",LogLevel.Error);
                                mapIsValid=false;
                            }
                        }
                        else
                        {
                            mNewMap = _utilitiesService.MapLoaderService.Loader.LoadMap(gameEnvironmentService.GamePath, Path.Combine(oPack.ModPath, "assets", oPack.MapName), sPackName, false);
                            //
                            //  parse map for tags
                            //
                            ExpansionPack autoParse = _autoMapperService.ParseMap(mNewMap);
#if DEBUG_LOG
                            logger.Log($"Auto-mapping: {sPackName}", LogLevel.Info);
#endif
                            oPack.Bushes.AddRange(autoParse.Bushes);
                            oPack.TreasureSpots = autoParse.TreasureSpots;
                            if (autoParse.CaveEntrance.WarpIn.X != -1 && autoParse.CaveEntrance.WarpIn.Y != -1)
                            {
                                logger.Log($"   auto-mapping CaveEntrance.WarpIn", LogLevel.Debug);
                                if (oPack.CaveEntrance == null) { oPack.CaveEntrance = new EntranceDetails(); }
                                oPack.CaveEntrance.WarpIn = autoParse.CaveEntrance.WarpIn;
                            }
                            else
                            {
                                logger.Log($"    missing CaveEntrance WarpIn tag", LogLevel.Warn);
                            }
                            if (autoParse.CaveEntrance.WarpOut.X != -1 && autoParse.CaveEntrance.WarpOut.Y != -1)
                            {
                                logger.Log($"   auto-mapping CaveEntrance.WarpOut", LogLevel.Debug);
                                if (oPack.CaveEntrance == null) { oPack.CaveEntrance = new EntranceDetails(); }
                                oPack.CaveEntrance.WarpOut = autoParse.CaveEntrance.WarpOut;
                            }
                            else
                            {
                                logger.Log($"    missing CaveEntrance WarpOut tag", LogLevel.Warn);
                            }
                            if (oPack.EntrancePatches == null)
                            {
                                oPack.EntrancePatches = new Dictionary<string, EntrancePatch> { };

                            }
                            if (autoParse.EntrancePatches.ContainsKey("0"))
                            {
                                logger.Log($"   auto-mapping North Entrance Patch", LogLevel.Debug);
                                if (oPack.EntrancePatches.ContainsKey("0"))
                                    oPack.EntrancePatches.Remove("0");

                                oPack.EntrancePatches.Add("0", autoParse.EntrancePatches["0"]);
                            }
                            else
                            {
                                mapIsValid = false;
                                logger.Log($"    missing North Entrance Patch", LogLevel.Warn);
                            }
                            if (autoParse.EntrancePatches.ContainsKey("1"))
                            {
                                logger.Log($"   auto-mapping East Entrance Patch", LogLevel.Debug);
                                if (oPack.EntrancePatches.ContainsKey("1"))
                                    oPack.EntrancePatches.Remove("1");

                                oPack.EntrancePatches.Add("1", autoParse.EntrancePatches["1"]);
                            }
                            else
                            {
                                mapIsValid = false;
                                logger.Log($"    missing East Entrance Patch", LogLevel.Warn);
                            }
                            if (autoParse.EntrancePatches.ContainsKey("2"))
                            {
                                logger.Log($"   auto-mapping South Entrance Patch", LogLevel.Debug);
                                if (oPack.EntrancePatches.ContainsKey("2"))
                                    oPack.EntrancePatches.Remove("2");

                                oPack.EntrancePatches.Add("2", autoParse.EntrancePatches["2"]);
                            }
                            else
                            {
                                mapIsValid = false;
                                logger.Log($"    missing South Entrance Patch", LogLevel.Warn);
                            }
                            if (autoParse.EntrancePatches.ContainsKey("3"))
                            {
                                logger.Log($"   auto-mapping West Entrance Patch", LogLevel.Debug);
                                if (oPack.EntrancePatches.ContainsKey("3"))
                                    oPack.EntrancePatches.Remove("3");

                                oPack.EntrancePatches.Add("3", autoParse.EntrancePatches["3"]);
                            }
                            else
                            {
                                mapIsValid = false;
                                logger.Log($"    missing West Entrance Patch", LogLevel.Warn);
                            }
                            if (autoParse.FishAreas != null)
                            {
                                foreach (var area in autoParse.FishAreas)
                                {
                                    if (oPack.FishAreas.ContainsKey(area.Key))
                                    {
                                        oPack.FishAreas.Remove(area.Key);
                                    }

                                    oPack.FishAreas.Add(area.Key, area.Value);
                                }
                            }

                            oPack.FishData = autoParse.FishData;

                            oPack.MineCarts = autoParse.MineCarts;

                            if (autoParse.MineCarts.Any())
                            {
                                Layer backLayer = mNewMap.GetLayer("Buildings");

                                if (backLayer == null)
                                {
                                    logger.Log($"    Could not find Buildings layer for minecart mapping.", LogLevel.Debug);
                                }
                                else
                                {
                                    foreach (ExpansionPack.MineCartSpot mineCartStation in autoParse.MineCarts)
                                    {
                                        foreach (Point aTile in mineCartStation.MineCartActionPoints)
                                        {
                                            backLayer.Tiles[aTile.X, aTile.Y].Properties.Add("Action", $"MinecartTransport Default {oPack.LocationName}.{mineCartStation.MineCartDisplayName}");
                                        }
                                        logger.Log($"    Added minecart Action square(s)", LogLevel.Debug);
                                    }
                                }
                            }
                            logger.Log($"    FishData: {(oPack.FishData == null ? 0 : oPack.FishData.Count())}", LogLevel.Debug);
                            logger.Log($"   FishAreas: {(oPack.FishAreas == null ? 0 : oPack.FishAreas.Count())}", LogLevel.Debug);

                            oPack.suspensionBridges = autoParse.suspensionBridges;
                            logger.Log($"       Added {oPack.suspensionBridges.Count()} Suspension Bridges", LogLevel.Debug);
                        }
                        if (mapIsValid)
                        {
#if DEBUG_LOG
                            logger.Log($"Loaded map '{sPackName}' for {sPackName}", LogLevel.Trace);
#endif
                            //
                            //  add custom map properties
                            //  usually only done when editing base maps
                            //
                            if (oPack.MapProperties != null)
                            {
                                foreach (var property in oPack.MapProperties)
                                {
#if DEBUG
                                    logger.Log($"  Adding map property: [{property.Key}]='{property.Value}'", LogLevel.Debug);
#endif
                                    mNewMap.Properties[property.Key] = property.Value;
                                }
                            }
                            ExpansionMaps.Add(sPackName, mNewMap);
                            bHaveActive = true;
                        }
                        else
                        {
                            ValidContents.Remove(sPackName);
                            logger.Log($"{sPackName} cannot be loaded.  It is missing required map Tokens", LogLevel.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex);
                    logger.Log($"Unable to load map file '{sPackName}', unloading mod. Please try re-installing the mod.", LogLevel.Alert);
                    lFailedPacks.Add(sPackName);
                }
            }
            foreach (string sFail in lFailedPacks)
            {
                ValidContents.Remove(sFail);
            }

            return bHaveActive;
        }
        private void RemoveBaseFarmProperties(Map targetMap, List<string> mapPropertiesToKeep)
        {
            //  map properties to keep
            //
            foreach (var mapProperty in targetMap.Properties.ToList())
            {
                if (mapPropertiesToKeep==null ||!mapPropertiesToKeep.Contains(mapProperty.Key))
                    targetMap.Properties.Remove(mapProperty.Key);
            }
            // paths layer
            Layer pathsLayer = targetMap.GetLayer("Paths");
            Layer buildingsLayer = targetMap.GetLayer("Buildings");
            if (pathsLayer != null)
            {
                for (int x = 0; x < pathsLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < pathsLayer.LayerHeight; y++)
                    {
                        if (pathsLayer.Tiles[x, y] != null && pathsLayer.Tiles[x, y].Properties != null && pathsLayer.Tiles[x, y].Properties.Any())
                        {
                            foreach (var prop in pathsLayer.Tiles[x, y].Properties.ToList())
                            {
                                if (prop.Key == "Order")
                                {
                                    pathsLayer.Tiles[x, y].Properties.Remove(prop.Key);
                                }
                            }
                        }
                        if (buildingsLayer.Tiles[x, y] != null && buildingsLayer.Tiles[x, y].Properties != null && buildingsLayer.Tiles[x, y].Properties.Any())
                        {
                            foreach (var prop in buildingsLayer.Tiles[x, y].Properties.ToList())
                            {
                                if (prop.Key == "Action")
                                {
                                    buildingsLayer.Tiles[x, y].Properties.Remove(prop.Key);
                                }
                            }
                        }
                    }
                }
            }
        }

        //private ExpansionPack LoadExpansionContentPack(IContentPack contentPack)
        //{
        //    ExpansionPack content = null;

        //    try
        //    {
        //        content = contentPack.ReadJsonFile<ExpansionPack>("expansion.json");

        //        if (content.FileFormat == null || content.FileFormat.MajorVersion < 1 || content.FileFormat.MinorVersion < 2)
        //        {
        //            logger.Log($"{contentPack.Manifest.Name} is not at least version 1.2 and will not be loaded.", LogLevel.Error);
        //            return null;
        //        }

        //        content.Owner = contentPack;
        //        content.ModPath = contentPack.DirectoryPath;
        //        Prepare(content);

        //        logger.Log("CP: " + content.LocationName + ", " + content.Description, LogLevel.Debug);

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log($"Content pack {contentPack.Manifest.Name} failed loading 'expansion.json' and will not be available in game.", LogLevel.Error);
        //        logger.Log($"Error Details: {ex.Message}");
        //        content = null;
        //    }

        //    return content;
        //}
    }
}
