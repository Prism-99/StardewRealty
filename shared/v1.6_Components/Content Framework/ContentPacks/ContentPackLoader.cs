using SDV_Realty_Core.Framework.Buildings;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using SDV_Realty_Core.Framework.CustomEntities.Crops;
using SDV_Realty_Core.Framework.CustomEntities.Machines;
using SDV_Realty_Core.Framework.CustomEntities.Objects;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SDV_Realty_Core.Framework.CustomEntities.LocationContexts;
using xTile;
using xTile.Format;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using xTile.Layers;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.ContentPackFramework.ContentPacks
{
    /// <summary>
    /// 1.6 Loader methods
    /// </summary>
    internal partial class ContentPackLoader
    {
        private ICustomEntitiesServices customEntitiesServices;
        private IUtilitiesService _utilitiesService;
        private IAutoMapperService _autoMapperService;
        private IGameEnvironmentService gameEnvironmentService;
        public ContentPackLoader(ILoggerService olog, IUtilitiesService utilitiesService, ICustomEntitiesServices customEntitiesServices, IModDataService modDataService, IAutoMapperService autoMapperService, IGameEnvironmentService gameEnvironmentService)
        {
            logger = olog;
            Contents = new List<ExpansionPack>();
            ValidContents = modDataService.validContents;// new Dictionary<string, ExpansionPack> { };
            ExpansionMaps = modDataService.ExpansionMaps;
            helper = utilitiesService.ModHelperService.modHelper;
            _utilitiesService= utilitiesService; 
            _autoMapperService= autoMapperService;
            this.customEntitiesServices = customEntitiesServices;
            this.gameEnvironmentService = gameEnvironmentService;
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
                new CustomLocationContext(), new CustomMachine()
               
            };

            List<ISDRContentPack> foundPacks = new List<ISDRContentPack>();
            foreach (var contentPack in contentPacks)
            {
                foreach (ISDRContentPack packType in contentPackTypes)
                {
                    var packs = Directory.GetFiles(contentPack.DirectoryPath, packType.PackFileName, SearchOption.AllDirectories);
                    foreach (var pack in packs)
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
                            logger.Log($"Loaded content pack {newPack.Owner.Manifest.Name}.{newPack.GetType().Name}", LogLevel.Debug);
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
            foreach (var newPack in foundPacks)
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
        public ISDRContentPack LoadContentPack(IContentPack contentPack)
        {
            if (contentPack.HasFile("expansion.json"))
            {
                return LoadExpansionContentPack(contentPack);
            }
            if (contentPack.HasFile("bigcraftable.json"))
            {
                return customEntitiesServices.customBigCraftableService.customBigCraftableManager.ReadContentPack(contentPack);
            }

            logger.Log($"Content pack '{contentPack.Manifest.Name}' has no entry file 'expansion.json'", LogLevel.Error);
            return null;
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
                try
                {
                    if (oPack.LoadMap)
                    {
                        bool mapIsValid = true;
                        Map mNewMap = _utilitiesService.MapLoaderService.Loader.LoadMap(gameEnvironmentService.GamePath, Path.Combine(oPack.ModPath, "assets", oPack.MapName), sPackName, false);
                        //mNewMap.Properties.Add("LocationContext", "sdr.always_winter");
                        //
                        //  parse map for tags
                        //
                        ExpansionPack autoParse = _autoMapperService.ParseMap(mNewMap);

                        logger.Log($"Auto-mapping: {sPackName}", LogLevel.Info);

                        oPack.Bushes.AddRange(autoParse.Bushes);
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


                        oPack.MineCartEnabled = autoParse.MineCartEnabled;

                        if (autoParse.MineCartEnabled)
                        {
                            Layer backLayer = mNewMap.GetLayer("Buildings");

                            if (backLayer == null)
                            {
                                oPack.MineCartEnabled = false;
                                logger.Log($"    Could not find Buildings layer for minecart mapping.", LogLevel.Debug);
                            }
                            else
                            {
                                foreach (var aTile in autoParse.MineCartActionPoints)
                                {
                                    backLayer.Tiles[aTile.X, aTile.Y].Properties.Add("Action", $"MinecartTransport Default {sPackName}");
                                }
                                logger.Log($"    Added minecart Action square(s)", LogLevel.Debug);
                                oPack.MineCart = autoParse.MineCart;
                                oPack.MineCartDirection = autoParse.MineCartDirection;
                                oPack.MineCartDisplayName = autoParse.MineCartDisplayName;
                            }
                        }
                        logger.Log($"    FishData: {(oPack.FishData == null ? 0 : oPack.FishData.Count())}", LogLevel.Debug);
                        logger.Log($"   FishAreas: {(oPack.FishAreas == null ? 0 : oPack.FishAreas.Count())}", LogLevel.Debug);

                        oPack.suspensionBridges = autoParse.suspensionBridges;
                        logger.Log($"       Added {oPack.suspensionBridges.Count()} Suspension Bridges", LogLevel.Debug);

                        if (mapIsValid)
                        {
                            logger.Log($"Loaded map 'Maps/{sPackName}' for {sPackName}", LogLevel.Trace);
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
                    logger.Log(ex.ToString(), LogLevel.Error);
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

    }
}
