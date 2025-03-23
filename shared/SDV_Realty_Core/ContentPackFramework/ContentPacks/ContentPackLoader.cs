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
using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewValley.Extensions;
using StardewModHelpers;
using StardewValley.GameData;
using System.Text.Json;



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
        private IModDataService modDataService;
        // public Dictionary<string, Map> ExpansionMaps;
        //public Dictionary<string, ExpansionPack> ValidContents { get; set; }
        private Dictionary<string, object> externalReferences;
        public ContentPackLoader() { }
        public ContentPackLoader(ILoggerService olog, Dictionary<string, object> externalReferences, IUtilitiesService utilitiesService, ICustomEntitiesServices customEntitiesServices, IModDataService modDataService, IAutoMapperService autoMapperService, IGameEnvironmentService gameEnvironmentService)
        {
            logger = olog;
            Contents = new List<ExpansionPack>();
            this.modDataService = modDataService;
            modDataService.validContents.Clear();
            //ExpansionMaps = modDataService.ExpansionMaps;
            _utilitiesService = utilitiesService;
            _autoMapperService = autoMapperService;
            this.customEntitiesServices = customEntitiesServices;
            this.gameEnvironmentService = gameEnvironmentService;
            this.externalReferences = externalReferences;
        }

        public ContentPackLoader(ILoggerService olog, IModHelper helper)
        {
            logger = olog;
            Contents = new List<ExpansionPack>();
        }
        private void PrintContentPackListSummary()
        {
            logger.Log($"Loaded {modDataService.validContents.Count} content packs:", LogLevel.Info);
            modDataService.validContents.Select(c => c.Value.Owner.Manifest)
                .ToList()
                .ForEach(
                    (m) => logger.Log($"   {m.Name} {m.Version} by {m.Author} ({m.UniqueID})", LogLevel.Info));
        }
        /// <summary>
        /// Preload Thumbnail and ForSaleImage
        /// </summary>
        /// <param name="contentPack">Content pack to preload</param>
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
                            if (content.isAdditionalFarm)
                            {
                                try
                                {
                                    //StardewBitmap bitmap = new StardewBitmap(_utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(content.ForSaleImageName));
                                    //content.ForSaleImage = Texture2D.FromStream(Game1.graphics.GraphicsDevice, bitmap.SourceStream());
                                    //content.ForSaleImage = new StardewBitmap(_utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(content.ForSaleImageName)).Texture();
                                    //Color[] az = Enumerable.Range(0, 100).Select(i => Color.White).ToArray();
                                    //Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, 30, 30, false, SurfaceFormat.Color);
                                    //texture.SetData(az);
                                    //
                                    //  cp additional farms world image
                                    //  not necessarily loaded
                                    //
                                    //_utilitiesService.InvalidateCache(content.ForSaleImageName);
                                    //Texture2D tmpTexture = _utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(content.ForSaleImageName);
                                    //using (FileStream fs = File.OpenWrite("test.png"))
                                    //{
                                    //    tmpTexture.SaveAsPng(fs, tmpTexture.Width, tmpTexture.Height);
                                    //}
                                    //StardewBitmap srcBitmp = new StardewBitmap(tmpTexture);
                                    //StardewBitmap tmpBitMap = new StardewBitmap(tmpTexture.Width, tmpTexture.Height);
                                    //tmpBitMap.DrawImage(srcBitmp, new Rectangle(0, 0, tmpBitMap.Width, tmpBitMap.Height), new Rectangle(0, 0, tmpBitMap.Width, tmpBitMap.Height));

                                    ////Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                                    ////texture.SetData<Color>(new Color[] { Color.Blue });
                                    //content.ForSaleImage = tmpBitMap.Texture();
                                    //_utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(content.ForSaleImageName);
                                    content.ForSaleImage = Game1.content.Load<Texture2D>(content.ForSaleImageName);
                                    //content.ForSaleImage = _utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(content.ForSaleImageName);
                                }
                                catch (Exception ex)
                                {
                                    content.ForSaleImageName = null;
                                    content.ForSaleImage = new StardewBitmap(modDataService.ExternalReferences[$"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}no_world_map"].ToString()).Texture();// null;
                                    content.WorldMapTexture = $"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}no_world_map";
                                }
                            }
                            else
                            {
                                FileStream filestream = new FileStream(Path.Combine(content.ModPath, "assets", content.ForSaleImageName), FileMode.Open);
                                content.ForSaleImage = Texture2D.FromStream(Game1.graphics.GraphicsDevice, filestream);
                            }
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
        private bool TryGetCustomAdditionalFarm(string key, ModFarmType farmType, out ExpansionPack expansionPack)
        {
            expansionPack = null;

            switch (farmType.Id)
            {
                case "A_TK.FarmProjectForaging/WaFFLE":
                    expansionPack = new ExpansionPack(key, farmType);
                    expansionPack.EntrancePatches = new();
                    expansionPack.FishAreas = new();
                    expansionPack.OriginalFarmKey = farmType.Id;
                    string name = Game1.content.LoadString(farmType.TooltipStringPath);
                    int split = name.IndexOf("_");
                    if (split == -1)
                    {
                        string displayName = farmType.Id.Split('/')[farmType.Id.Split('/').Length - 1];
                        displayName = displayName.Split('.')[displayName.Split(".").Length - 1];
                        expansionPack.DisplayName = displayName;
                        expansionPack.Description = $"Additional Farm type {expansionPack.DisplayName} added as a Farm Expansion.";
                    }
                    else
                    {
                        expansionPack.DisplayName = name.Substring(0, split);
                        expansionPack.Description = name.Substring(split + 1);
                    }
                    expansionPack.Cost = modDataService.Config.useGlobalPrice ? modDataService.Config.globalPrice : 100000;
                    expansionPack.Conditions = modDataService.Config.useGlobalCondition ? modDataService.Config.globalCondition : "";
                    expansionPack.WorldMapTexture = farmType.WorldMapTexture;
                    expansionPack.ForSaleImageName = farmType.WorldMapTexture;
                    expansionPack.MapName = farmType.MapName;
                    expansionPack.BaseLocationName = farmType.Id;
                    expansionPack.Owner = new OwnerPack(
                        new OwnerManifest
                        {
                            UniqueID = farmType.Id
                        }
                        );
                    expansionPack.SubLocations.Add("A_TK.FarmProjectForaging.Extra_Custom_waterfall_path_north");
                    expansionPack.SubLocations.Add("A_TK.FarmProjectForaging.LE_Custom_waterfall_cave_WaFFLE");
                    // add cave exits
                    expansionPack.CaveEntrance.WarpOut = new EntranceWarp
                    {
                        X = 34,
                        Y = 5
                    };
                    expansionPack.CaveEntrance.WarpIn = new EntranceWarp
                    {
                        X = 34,
                        Y = 6
                    };
                    // add north exit
                    int srcX = 99;
                    int srcY = 11;
                    expansionPack.EntrancePatches.Add("0", new EntrancePatch
                    {
                        PatchSide = EntranceDirection.North,
                        PathBlockX = srcX,
                        PathBlockY = srcY + 1,
                        WarpOrientation = WarpOrientations.Horizontal,
                        WarpOut = new EntranceWarp
                        {
                            NumberofPoints = 3,
                            X = srcX,
                            Y = srcY,
                            LocationName = "A_TK.FarmProjectForaging.Extra_Custom_waterfall_path_north"
                        },
                        WarpIn = new EntranceWarp
                        {
                            NumberofPoints = 3,
                            X = srcX,
                            Y = srcY + 1,
                            LocationName = "A_TK.FarmProjectForaging.Extra_Custom_waterfall_path_north"
                        }
                    });
                    // add east exit
                    srcX = 93;
                    srcY = 1;
                    expansionPack.EntrancePatches.Add("1", new EntrancePatch
                    {
                        PatchSide = EntranceDirection.East,
                        PathBlockX = srcX - 1,
                        PathBlockY = srcY,
                        WarpOrientation = WarpOrientations.Vertical,
                        WarpOut = new EntranceWarp
                        {
                            NumberofPoints = 2,
                            X = srcX,
                            Y = srcY
                        },
                        WarpIn = new EntranceWarp
                        {
                            NumberofPoints = 2,
                            X = srcX - 1,
                            Y = srcY
                        }
                    });
                    // add south exit
                    srcX = 13;
                    srcY = 109;
                    expansionPack.EntrancePatches.Add("2", new EntrancePatch
                    {
                        PatchSide = EntranceDirection.South,
                        PathBlockX = srcX,
                        PathBlockY = srcY - 2,
                        WarpOrientation = WarpOrientations.Horizontal,
                        WarpOut = new EntranceWarp
                        {
                            NumberofPoints = 2,
                            X = srcX,
                            Y = srcY
                        },
                        WarpIn = new EntranceWarp
                        {
                            NumberofPoints = 2,
                            X = srcX,
                            Y = srcY - 1
                        }
                    });
                    string json = JsonSerializer.Serialize(expansionPack);
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Load definition files for Additional Farm mods that cannot
        /// be auto mapped in
        /// </summary>
        /// <returns>A dictionary of FarmID:ExpansionPack of custom configs</returns>
        private Dictionary<string, ExpansionPack> LoadAdditionlFarmConfigs()
        {
            Dictionary<string, ExpansionPack> configs = new();

            try
            {
                string configsPath = Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "additionalfarms", "configs");
                string[] files = Directory.GetFiles(configsPath);
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        ExpansionPack? expansionPack = JsonSerializer.Deserialize<ExpansionPack>(json);
                        if (expansionPack != null)
                            configs.Add(expansionPack.BaseLocationName, expansionPack);
                    }
                    catch (Exception ex1)
                    {
                        logger.Log($"Error loading AdditionalFarm config: {file}", LogLevel.Error);
                        logger.LogError(ex1);
                    }
                }
#if DEBUG
                logger.Log($"Loaded {configs.Count} AdditionalFarm configs.", LogLevel.Debug);
#endif

            }
            catch (Exception ex)
            {
                logger.LogError(ex);
            }
            return configs;
        }
        /// <summary>
        /// Loads farms listed in Data/AdditionalFarms as Expansions
        /// </summary>
        public void LoadAdditionalFarms()
        {
            Dictionary<string, ExpansionPack> configs = LoadAdditionlFarmConfigs();
            List<ModFarmType> additionalFarms = DataLoader.AdditionalFarms(Game1.content);
            foreach (ModFarmType? farm in additionalFarms)
            {
                try
                {
                    bool haveCave = false;
                    bool haveNorth = false;
                    bool haveEast = false;
                    bool haveSouth = false;
                    // check to see if the farm is blacklisted
                    if (gameEnvironmentService.BlackListedAdditionalFarms.Contains(farm.Id))
                    {
                        logger.Log($"Skipping farm '{farm.Id}' it is not supported.", LogLevel.Info);
                        continue;
                    }

                    string key = $"sdr.additionalFarm.{farm.Id}";
                    Map farmMap = _utilitiesService.ModHelperService.modHelper.GameContent.Load<Map>($"Maps/{farm.MapName}");
                    // parse warps
                    //
                    bool haveCustom = false;
                    if (farmMap != null)
                    {
                        ExpansionPack expansionPack;

                        if (configs.TryGetValue(farm.Id, out expansionPack))
                        {
                            haveCustom = true;
                        }
                        else
                        {
                            expansionPack = new ExpansionPack(key, farm);


                            expansionPack.EntrancePatches = new();
                            expansionPack.FishAreas = new();
                            expansionPack.OriginalFarmKey = farm.Id;
                            string name;
                            if (string.IsNullOrEmpty(farm.TooltipStringPath))
                                name = farm.MapName;
                            else
                                name = Game1.content.LoadString(farm.TooltipStringPath);
                            int split = name.IndexOf("_");
                            if (split == -1)
                            {
                                string displayName = farm.Id.Split('/')[farm.Id.Split('/').Length - 1];
                                displayName = displayName.Split('.')[displayName.Split(".").Length - 1];
                                expansionPack.DisplayName = displayName;
                                expansionPack.Description = $"Additional Farm type {expansionPack.DisplayName} added as a Farm Expansion.";
                            }
                            else
                            {
                                expansionPack.DisplayName = name.Substring(0, split);
                                expansionPack.Description = name.Substring(split + 1);
                            }
                            expansionPack.Cost = farmMap.DisplayHeight * farmMap.DisplayWidth * 10;
                            //modDataService.Config.useGlobalPrice ? modDataService.Config.globalPrice : 100000;
                            expansionPack.Conditions = modDataService.Config.useGlobalCondition ? modDataService.Config.globalCondition : "";
                            expansionPack.WorldMapTexture = farm.WorldMapTexture ?? "SDR/images/no_world_map";
                            expansionPack.ForSaleImageName = farm.WorldMapTexture ?? "SDR/images/no_world_map";
                            expansionPack.MapName = farm.MapName;
                            expansionPack.BaseLocationName = farm.Id;
                            expansionPack.Owner = new OwnerPack(
                                new OwnerManifest
                                {
                                    UniqueID = farm.Id
                                }
                                );
                        }

                        expansionPack.MapSize = new Vector2(farmMap.DisplayWidth / Game1.tileSize, farmMap.DisplayHeight / Game1.tileSize);

                        expansionPack.Cost = (int)(expansionPack.MapSize.X * expansionPack.MapSize.Y);
                        expansionPack.BasePropertiesToKeep = new List<string>
                            {
                                "AllowGrassSurviveInWinter", "BrookSounds", "Outdoors", "WarpTotemEntry"
                            };
                        //
                        //  parse the Warp proptery to obtain exit locations
                        //
                        if (farmMap.Properties.TryGetValue("Warp", out var warps))
                        {
                            //
                            //  parse warps and add to list
                            //
                            string[] warpParts = warps.ToString().Replace("  ", " ").Split(' ');
                            List<MapWarp> warpList = new List<MapWarp>();
                            for (int index = 0; index < warpParts.Length; index += 5)
                            {
                                int srcX = int.Parse(warpParts[index]);
                                int srcY = int.Parse(warpParts[index + 1]);
                                int destX = int.Parse(warpParts[index + 3]);
                                int destY = int.Parse(warpParts[index + 4]);

                                warpList.Add(new MapWarp
                                {
                                    FromX = srcX,
                                    FromY = srcY,
                                    ToMap = warpParts[index + 2],
                                    ToX = destX,
                                    ToY = destY
                                });
                            }
                            //
                            //  add required entrances/exits
                            //

                            if (!haveCustom)
                            {
                                //  get North exit
                                var northWarp = warpList.Where(p => p.ToMap == "Backwoods").ToList();
                                if (northWarp.Count > 0)
                                {
                                    int srcX = northWarp.Min(p => p.FromX);
                                    int srcY = northWarp.First().FromY;
                                    expansionPack.EntrancePatches.Add("0", new EntrancePatch
                                    {
                                        PatchSide = EntranceDirection.North,
                                        PathBlockX = srcX,
                                        PathBlockY = srcY + 2,
                                        WarpOrientation = WarpOrientations.Horizontal,
                                        WarpOut = new EntranceWarp
                                        {
                                            NumberofPoints = northWarp.Count,
                                            X = srcX,
                                            Y = srcY
                                        },
                                        WarpIn = new EntranceWarp
                                        {
                                            NumberofPoints = northWarp.Count,
                                            X = srcX,
                                            Y = srcY + 1
                                        }
                                    });
                                    haveNorth = true;
                                }
                                // get east exit
                                var eastWarp = warpList.Where(p => p.ToMap == "BusStop").ToList();
                                if (eastWarp.Count > 0)
                                {
                                    int srcX = eastWarp.Min(p => p.FromX) - 1;
                                    int srcY = eastWarp.Min(p => p.FromY);
                                    expansionPack.EntrancePatches.Add("1", new EntrancePatch
                                    {
                                        PatchSide = EntranceDirection.East,
                                        PathBlockX = srcX - 2,
                                        PathBlockY = srcY,
                                        WarpOrientation = WarpOrientations.Vertical,
                                        WarpOut = new EntranceWarp
                                        {
                                            NumberofPoints = eastWarp.Count,
                                            X = srcX,
                                            Y = srcY
                                        },
                                        WarpIn = new EntranceWarp
                                        {
                                            NumberofPoints = eastWarp.Count,
                                            X = srcX - 1,
                                            Y = srcY
                                        }
                                    });
                                    haveEast = true;
                                }
                                // get sout exit
                                var soutWarp = warpList.Where(p => p.ToMap == "Forest").ToList();
                                if (soutWarp.Count > 0)
                                {
                                    int srcX = soutWarp.Min(p => p.FromX);
                                    int srcY = soutWarp.First().FromY;
                                    expansionPack.EntrancePatches.Add("2", new EntrancePatch
                                    {
                                        PatchSide = EntranceDirection.South,
                                        PathBlockX = srcX,
                                        PathBlockY = srcY - 2,
                                        WarpOrientation = WarpOrientations.Horizontal,
                                        WarpOut = new EntranceWarp
                                        {
                                            NumberofPoints = northWarp.Count,
                                            X = srcX,
                                            Y = srcY
                                        },
                                        WarpIn = new EntranceWarp
                                        {
                                            NumberofPoints = northWarp.Count,
                                            X = srcX,
                                            Y = srcY - 1
                                        }
                                    });
                                    haveSouth = true;
                                }
                                // get warproom entrance (Cave Entrance)
                                var mineWarp = warpList.Where(p => p.ToMap == "FarmCave").ToList();
                                if (mineWarp.Count > 0)
                                {
                                    int srcX = mineWarp.First().FromX;
                                    int srcY = mineWarp.First().FromY;
                                    expansionPack.CaveEntrance = new EntranceDetails
                                    {
                                        WarpIn = new EntranceWarp { X = srcX, Y = srcY + 2, NumberofPoints = 1 },
                                        WarpOut = new EntranceWarp { X = srcX, Y = srcY + 1, NumberofPoints = 1 }
                                    };
                                    haveCave = true;
                                }
                            }

                            // clear Warp map property
                            //
                            farmMap.Properties.Remove("Warp");
                            RemoveBaseFarmProperties(farmMap, expansionPack.BasePropertiesToKeep, key);
                            //
                            //  add custom warps back
                            //
                            List<string> fixedWarps = new List<string>();
                            foreach (var mapWarp in warpList)
                            {
                                switch (mapWarp.ToMap)
                                {
                                    case "BusStop":
                                    case "Backwoods":
                                    case "FarmCave":
                                    case "Forest":
                                        // remove default warps
                                        break;
                                    case "Farm":
                                        // update intra location warp to new location name
                                        fixedWarps.Add($"{mapWarp.FromX} {mapWarp.FromY} {key} {mapWarp.ToX} {mapWarp.ToY}");
                                        break;
                                    default:
                                        //
                                        //  these would be sublocation warps
                                        if (!expansionPack.SubLocations.Contains(mapWarp.ToMap))
                                        {
                                            expansionPack.SubLocations.Add(mapWarp.ToMap);
                                            logger.Log($"Add sub area '{mapWarp.ToMap}' to {expansionPack.DisplayName}", LogLevel.Debug);
                                        }
                                        // re-add custom warps
                                        fixedWarps.Add($"{mapWarp.FromX} {mapWarp.FromY} {mapWarp.ToMap} {mapWarp.ToX} {mapWarp.ToY}");
                                        break;
                                }
                            }
                            if (fixedWarps.Count > 0)
                            {
                                farmMap.Properties.Add("Warp", string.Join(' ', fixedWarps.ToArray()));
                            }
                            MassageAdditionalFarmPack(expansionPack);
                        }
                        if (haveCustom || (haveCave && haveEast && haveNorth && haveSouth))
                        {
                            _utilitiesService.MapUtilities.AdjustMapActions(farmMap, key);
                            Prepare(expansionPack);
                            CheckForLocalWorldMapImage(expansionPack);
                            modDataService.ExpansionMaps.Add(_utilitiesService.GetMapUniqueName(expansionPack), farmMap);
                            modDataService.validContents.Add(key, expansionPack);
                        }
                        else
                        {
                            logger.Log($"Cannot add additional farm '{key}'. It is missing:", LogLevel.Info);
                            if (!haveNorth)
                                logger.Log($"-North exit.", LogLevel.Info);
                            if (!haveEast)
                                logger.Log($"-East exit.", LogLevel.Info);
                            if (!haveSouth)
                                logger.Log($"-South exit.", LogLevel.Info);
                            if (!haveCave)
                                logger.Log($"-Cave exit.", LogLevel.Info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"Error loading additional farm {farm.Id}", LogLevel.Warn);
                    logger.Log($"{ex}", LogLevel.Error);

                }
            }
        }
        private void MassageAdditionalFarmPack(ExpansionPack expansionPack)
        {
            switch (expansionPack.BaseLocationName)
            {
                case "Draylon.ForeverFallsFarm":
                    expansionPack.WorldMapTexture = "Maps/MagdenianMini";
                    expansionPack.InternalWorldMapTexture = false;
                    break;
            }
        }
        private void CheckForLocalWorldMapImage(ExpansionPack expansionPack)
        {
            string imagePath = Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "additionalfarms", "images", $"{expansionPack.OriginalFarmKey.Replace("/", ".")}.png");
            if (File.Exists(imagePath))
            {
                expansionPack.WorldMapTexture = $"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{expansionPack.LocationName}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}worldmaptexture";
                externalReferences.Add(expansionPack.WorldMapTexture, new StardewBitmap(imagePath).Texture());
            }
        }
        /// <summary>
        /// Load all mod content packs
        /// </summary>
        /// <param name="modContentPacks">List of owned content packs to be added</param>
        public void LoadPacks(IEnumerable<IContentPack> modContentPacks)
        {
            //
            //  get a list of content pack file types
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

            foreach (IContentPack contentPack in modContentPacks)
            {
                foreach (ISDRContentPack packType in contentPackTypes)
                {
                    string[] packs = Directory.GetFiles(contentPack.DirectoryPath, packType.PackFileName, SearchOption.AllDirectories);
                    foreach (string pack in packs)
                    {
                        //
                        //  check for disabled directories
                        //
                        string[] pathParts = pack.Replace(gameEnvironmentService.GamePath, "").Split(Path.DirectorySeparatorChar);
                        if (pathParts.Where(p => p.StartsWith(".")).Any())
                        {
                            //disabled
                            logger.Log($"Pack {pack} not loaded, directory disabled.", LogLevel.Debug);
                            continue;
                        }
                        try
                        {
                            ISDRContentPack newPack = packType.ReadContentPack(pack);
                            newPack.translations = contentPack.Translation;
                            newPack.ModPath = Path.GetDirectoryName(pack);
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
            //
            //  add packs to appropriate manager
            //
            foreach (ISDRContentPack newPack in foundPacks)
            {
                try
                {
                    switch (newPack)
                    {
                        case ExpansionPack farmExp:
                            Prepare(farmExp);
                            if (ValidateContentPack(farmExp))
                                modDataService.validContents.Add(farmExp.LocationName, farmExp);

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
        /// <summary>
        /// Verify the content pack is valid
        /// </summary>
        /// <param name="content">Content pack to be verified</param>
        /// <returns>true if the pack is valid</returns>
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
        /// <summary>
        /// Loads an expansion map into IModDataService.ExpansionMaps
        /// -the map is loaded
        /// -it is parsed for autotags
        /// -any auto tags are added to the validContents ExpansionPack record
        /// </summary>
        /// <param name="expansionName">Expansion to load map for</param>
        /// <returns>true if the map is valid</returns>
        public bool LoadExpansionMap(string expansionName)
        {
            FormatManager formatManager = FormatManager.Instance;

            ExpansionPack expansionPack = modDataService.validContents[expansionName];
            Map newMap = null;
            bool result = false;

            try
            {
                //if (expansionPack.LoadMap)
                if (true)
                {
                    bool mapIsValid = true;
                    if (expansionPack.IsBaseFarm)
                    {
                        //
                        //  converting base farm into expansion farm
                        //
                        try
                        {
                            newMap = Game1.content.Load<Map>($"Maps/{expansionPack.BaseFarmMap}");
                            RemoveBaseFarmProperties(newMap, expansionPack.BasePropertiesToKeep);
                            if (expansionPack.BaseMapEdits != null)
                            {
                                foreach (var baseEdit in expansionPack.BaseMapEdits)
                                {
                                    Map sourceMap = expansionPack.Owner.ModContent.Load<Map>($"assets/{baseEdit.PatchMapName}");
                                    _utilitiesService.MapUtilities.PatchInMap(newMap, sourceMap, baseEdit.PatchPoint);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            logger.Log($"Error loading baseMap", LogLevel.Error);
                            logger.Log($"{err}", LogLevel.Error);
                            mapIsValid = false;
                        }
                    }
                    else
                    {
                        newMap = _utilitiesService.MapLoaderService.Loader.LoadMap(gameEnvironmentService.GamePath, Path.Combine(expansionPack.ModPath, "assets", expansionPack.MapName), expansionName, false);
                        //
                        //  parse map for tags
                        //
                        ExpansionPack autoParse = _autoMapperService.ParseMap(newMap);
#if DEBUG_LOG
                        logger.Log($"Auto-mapping: {expansionName}", LogLevel.Info);
#endif
                        //
                        //  add parsed tags to the expansion pack data
                        //
                        mapIsValid = _autoMapperService.MergeAutoMap(autoParse, expansionPack, newMap);

                    }
                    if (mapIsValid)
                    {
#if DEBUG_LOG
                        logger.Log($"Loaded map '{expansionPack.MapName ?? ""}' for {expansionName}", LogLevel.Trace);
#endif
                        expansionPack.MapSize = new Vector2(newMap.DisplayWidth / Game1.tileSize, newMap.DisplayHeight / Game1.tileSize);
                        //
                        //  add custom map properties
                        //  usually only done when editing base maps
                        //                        
                        if (expansionPack.MapProperties != null)
                        {
                            foreach (var property in expansionPack.MapProperties)
                            {
#if DEBUG
                                logger.Log($"  Adding map property: [{property.Key}]='{property.Value}'", LogLevel.Debug);
#endif
                                newMap.Properties[property.Key] = property.Value;
                            }
                        }
                        modDataService.ExpansionMaps[_utilitiesService.GetMapUniqueName(expansionPack)] = newMap;
                        result = true;
                    }
                    else
                    {
                        //failedPacks.Add(packName);
                        logger.Log($"{expansionName} cannot be loaded.  It is missing required map Tokens", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex);
                logger.Log($"Unable to load map file '{expansionName}', unloading mod. Please try re-installing the mod.", LogLevel.Alert);
            }

            return result;
        }
        /// <summary>
        /// Loads all validContents ExpansionPack maps
        /// -clears IModDataService.Expansion maps
        /// -removes any ExpansionPacks that fail from the validContents records
        /// </summary>
        /// <returns>true if any maps were loaded</returns>
        public bool LoadMaps()
        {

            //
            //  preload maps
            //
            modDataService.ExpansionMaps.Clear();
            bool haveActivePacks = false;
            FormatManager formatManager = FormatManager.Instance;

            List<string> failedPacks = new List<string> { };
            foreach (string packName in modDataService.validContents.Keys.ToList())
            {
                try
                {
                    if (LoadExpansionMap(packName))
                        haveActivePacks = true;
                    else
                        failedPacks.Add(packName);

                    //                    if (expansionPack.LoadMap)
                    //                    {
                    //                        bool mapIsValid = true;
                    //                        if (expansionPack.IsBaseFarm)
                    //                        {
                    //                            //
                    //                            //  converting base farm into expansion farm
                    //                            //
                    //                            try
                    //                            {
                    //                                newMap = Game1.content.Load<Map>($"Maps/{expansionPack.BaseFarmMap}");
                    //                                RemoveBaseFarmProperties(newMap, expansionPack.BasePropertiesToKeep);
                    //                                if (expansionPack.BaseMapEdits != null)
                    //                                {
                    //                                    foreach (var baseEdit in expansionPack.BaseMapEdits)
                    //                                    {
                    //                                        Map sourceMap = expansionPack.Owner.ModContent.Load<Map>($"assets/{baseEdit.PatchMapName}");
                    //                                        _utilitiesService.MapUtilities.PatchInMap(newMap, sourceMap, baseEdit.PatchPoint);
                    //                                    }
                    //                                }
                    //                                //var locationData = GameLocation.GetData(oPack.BaseLocationName);
                    //                                //GameLocation baseLocation = Game1.getLocationFromName();
                    //                                //if (locationData != null)
                    //                                //{
                    //                                //    oPack.FishAreas = new Dictionary<string, FishAreaDetails>();
                    //                                //    foreach (var fishArea in locationData.FishAreas)
                    //                                //    {
                    //                                //        oPack.FishAreas.Add(fishArea.Key, new FishAreaDetails(fishArea.Value));
                    //                                //    }
                    //                                //    oPack.Artifacts = new List<ArtifactData>();
                    //                                //    if (locationData.ArtifactSpots != null)
                    //                                //    {
                    //                                //        foreach(var spot in locationData.ArtifactSpots)
                    //                                //        {
                    //                                //            oPack.Artifacts.Add(new ArtifactData {Chance= spot.Chance, ArtifactId=spot.Id,Season=spot.se });
                    //                                //        }
                    //                                //    }                                
                    //                                //}
                    //                            }
                    //                            catch (Exception err)
                    //                            {
                    //                                logger.Log($"Error loading baseMap", LogLevel.Error);
                    //                                logger.Log($"{err}", LogLevel.Error);
                    //                                mapIsValid = false;
                    //                            }
                    //                        }
                    //                        else
                    //                        {
                    //                            newMap = _utilitiesService.MapLoaderService.Loader.LoadMap(gameEnvironmentService.GamePath, Path.Combine(expansionPack.ModPath, "assets", expansionPack.MapName), packName, false);
                    //                            //
                    //                            //  parse map for tags
                    //                            //
                    //                            ExpansionPack autoParse = _autoMapperService.ParseMap(newMap);
                    //#if DEBUG_LOG
                    //                            logger.Log($"Auto-mapping: {packName}", LogLevel.Info);
                    //#endif
                    //                            mapIsValid = _autoMapperService.MergeAutoMap(autoParse, expansionPack, newMap);
                    //                            //oPack.Bushes.AddRange(autoParse.Bushes);
                    //                            //oPack.TreasureSpots = autoParse.TreasureSpots;
                    //                            //if (autoParse.CaveEntrance.WarpIn.X != -1 && autoParse.CaveEntrance.WarpIn.Y != -1)
                    //                            //{
                    //                            //    logger.Log($"   auto-mapping CaveEntrance.WarpIn", LogLevel.Debug);
                    //                            //    if (oPack.CaveEntrance == null) { oPack.CaveEntrance = new EntranceDetails(); }
                    //                            //    oPack.CaveEntrance.WarpIn = autoParse.CaveEntrance.WarpIn;
                    //                            //}
                    //                            //else
                    //                            //{
                    //                            //    logger.Log($"    missing CaveEntrance WarpIn tag", LogLevel.Warn);
                    //                            //}
                    //                            //if (autoParse.CaveEntrance.WarpOut.X != -1 && autoParse.CaveEntrance.WarpOut.Y != -1)
                    //                            //{
                    //                            //    logger.Log($"   auto-mapping CaveEntrance.WarpOut", LogLevel.Debug);
                    //                            //    if (oPack.CaveEntrance == null) { oPack.CaveEntrance = new EntranceDetails(); }
                    //                            //    oPack.CaveEntrance.WarpOut = autoParse.CaveEntrance.WarpOut;
                    //                            //}
                    //                            //else
                    //                            //{
                    //                            //    logger.Log($"    missing CaveEntrance WarpOut tag", LogLevel.Warn);
                    //                            //}
                    //                            //if (oPack.EntrancePatches == null)
                    //                            //{
                    //                            //    oPack.EntrancePatches = new Dictionary<string, EntrancePatch> { };

                    //                            //}
                    //                            //if (autoParse.EntrancePatches.ContainsKey("0"))
                    //                            //{
                    //                            //    logger.Log($"   auto-mapping North Entrance Patch", LogLevel.Debug);
                    //                            //    if (oPack.EntrancePatches.ContainsKey("0"))
                    //                            //        oPack.EntrancePatches.Remove("0");

                    //                            //    oPack.EntrancePatches.Add("0", autoParse.EntrancePatches["0"]);
                    //                            //}
                    //                            //else
                    //                            //{
                    //                            //    mapIsValid = false;
                    //                            //    logger.Log($"    missing North Entrance Patch", LogLevel.Warn);
                    //                            //}
                    //                            //if (autoParse.EntrancePatches.ContainsKey("1"))
                    //                            //{
                    //                            //    logger.Log($"   auto-mapping East Entrance Patch", LogLevel.Debug);
                    //                            //    if (oPack.EntrancePatches.ContainsKey("1"))
                    //                            //        oPack.EntrancePatches.Remove("1");

                    //                            //    oPack.EntrancePatches.Add("1", autoParse.EntrancePatches["1"]);
                    //                            //}
                    //                            //else
                    //                            //{
                    //                            //    mapIsValid = false;
                    //                            //    logger.Log($"    missing East Entrance Patch", LogLevel.Warn);
                    //                            //}
                    //                            //if (autoParse.EntrancePatches.ContainsKey("2"))
                    //                            //{
                    //                            //    logger.Log($"   auto-mapping South Entrance Patch", LogLevel.Debug);
                    //                            //    if (oPack.EntrancePatches.ContainsKey("2"))
                    //                            //        oPack.EntrancePatches.Remove("2");

                    //                            //    oPack.EntrancePatches.Add("2", autoParse.EntrancePatches["2"]);
                    //                            //}
                    //                            //else
                    //                            //{
                    //                            //    mapIsValid = false;
                    //                            //    logger.Log($"    missing South Entrance Patch", LogLevel.Warn);
                    //                            //}
                    //                            //if (autoParse.EntrancePatches.ContainsKey("3"))
                    //                            //{
                    //                            //    logger.Log($"   auto-mapping West Entrance Patch", LogLevel.Debug);
                    //                            //    if (oPack.EntrancePatches.ContainsKey("3"))
                    //                            //        oPack.EntrancePatches.Remove("3");

                    //                            //    oPack.EntrancePatches.Add("3", autoParse.EntrancePatches["3"]);
                    //                            //}
                    //                            //else
                    //                            //{
                    //                            //    mapIsValid = false;
                    //                            //    logger.Log($"    missing West Entrance Patch", LogLevel.Warn);
                    //                            //}
                    //                            //if (autoParse.FishAreas != null)
                    //                            //{
                    //                            //    foreach (var area in autoParse.FishAreas)
                    //                            //    {
                    //                            //        if (oPack.FishAreas.ContainsKey(area.Key))
                    //                            //        {
                    //                            //            oPack.FishAreas.Remove(area.Key);
                    //                            //        }

                    //                            //        oPack.FishAreas.Add(area.Key, area.Value);
                    //                            //    }
                    //                            //}

                    //                            //oPack.FishData = autoParse.FishData;

                    //                            //oPack.MineCarts = autoParse.MineCarts;

                    //                            //if (autoParse.MineCarts.Any())
                    //                            //{
                    //                            //    Layer backLayer = newMap.GetLayer("Buildings");

                    //                            //    if (backLayer == null)
                    //                            //    {
                    //                            //        logger.Log($"    Could not find Buildings layer for minecart mapping.", LogLevel.Debug);
                    //                            //    }
                    //                            //    else
                    //                            //    {
                    //                            //        foreach (ExpansionPack.MineCartSpot mineCartStation in autoParse.MineCarts)
                    //                            //        {
                    //                            //            foreach (Point aTile in mineCartStation.MineCartActionPoints)
                    //                            //            {
                    //                            //                backLayer.Tiles[aTile.X, aTile.Y].Properties.Add("Action", $"MinecartTransport Default {oPack.LocationName}.{mineCartStation.MineCartDisplayName}");
                    //                            //            }
                    //                            //            logger.Log($"    Added minecart Action square(s)", LogLevel.Debug);
                    //                            //        }
                    //                            //    }
                    //                            //}
                    //                            //logger.Log($"    FishData: {(oPack.FishData == null ? 0 : oPack.FishData.Count())}", LogLevel.Debug);
                    //                            //logger.Log($"   FishAreas: {(oPack.FishAreas == null ? 0 : oPack.FishAreas.Count())}", LogLevel.Debug);

                    //                            //oPack.suspensionBridges = autoParse.suspensionBridges;
                    //                            //logger.Log($"       Added {oPack.suspensionBridges.Count()} Suspension Bridges", LogLevel.Debug);
                    //                        }
                    //                        if (mapIsValid)
                    //                        {
                    //#if DEBUG_LOG
                    //                            logger.Log($"Loaded map '{packName}' for {packName}", LogLevel.Trace);
                    //#endif
                    //                            //
                    //                            //  add custom map properties
                    //                            //  usually only done when editing base maps
                    //                            //
                    //                            if (expansionPack.MapProperties != null)
                    //                            {
                    //                                foreach (var property in expansionPack.MapProperties)
                    //                                {
                    //#if DEBUG
                    //                                    logger.Log($"  Adding map property: [{property.Key}]='{property.Value}'", LogLevel.Debug);
                    //#endif
                    //                                    newMap.Properties[property.Key] = property.Value;
                    //                                }
                    //                            }
                    //                            ExpansionMaps.Add(_utilitiesService.GetMapUniqueName(expansionPack), newMap);
                    //                            haveActivePacks = true;
                    //                        }
                    //                        else
                    //                        {
                    //                            failedPacks.Add(packName);
                    //                            logger.Log($"{packName} cannot be loaded.  It is missing required map Tokens", LogLevel.Error);
                    //                        }
                    //                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex);
                    logger.Log($"Unable to load map file '{packName}', unloading mod. Please try re-installing the mod.", LogLevel.Alert);
                    failedPacks.Add(packName);
                }
            }
            foreach (string sFail in failedPacks)
            {
                modDataService.validContents.Remove(sFail);
            }

            return haveActivePacks;
        }
        /// <summary>
        /// Remove Map properties and Action tiles from re-used base Farms
        /// </summary>
        /// <param name="targetMap">Map to be edited</param>
        /// <param name="mapPropertiesToKeep">Properties to keep, if found</param>
        private void RemoveBaseFarmProperties(Map targetMap, List<string> mapPropertiesToKeep, string newLocationName = null)
        {
            //  map properties to keep
            //
            foreach (var mapProperty in targetMap.Properties.ToList())
            {
                if (mapPropertiesToKeep == null || !mapPropertiesToKeep.Contains(mapProperty.Key))
                    targetMap.Properties.Remove(mapProperty.Key);
            }
            // check paths and buildings layer
            //
            //  looking for:
            //      Action tiles
            //      Order tiles
            //      Mail box tiles
            //
            Layer pathsLayer = targetMap.GetLayer("Paths");
            Layer buildingsLayer = targetMap.GetLayer("Buildings");
            Layer backLayer = targetMap.GetLayer("Back");
            Layer frontLayer = targetMap.GetLayer("Front");
            bool editedtMineCart = false;
            if (pathsLayer != null)
            {
                for (int x = 0; x < pathsLayer.LayerWidth; x++)
                {
                    for (int y = 0; y < pathsLayer.LayerHeight; y++)
                    {
                        // check for Order tiles
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
                        //  check for touchaction tiles
                        if (backLayer.Tiles[x, y] != null)
                        {
                            if (backLayer.Tiles[x, y].Properties != null && backLayer.Tiles[x, y].Properties.TryGetValue("TouchAction", out string actionValue))
                            {
                                string[] props = actionValue.Split(' ');
                                switch (props[0])
                                {
                                    case "Warp":
                                        if (props[1] == "Farm" && !string.IsNullOrEmpty(newLocationName))
                                        {
                                            backLayer.Tiles[x, y].Properties["TouchAction"] = actionValue.Replace("Farm", newLocationName);
                                        }
                                        // delete action
                                        break;
                                    default:
#if DEBUG
                                        Console.WriteLine($"New prop: {props[0]}");
#endif
                                        break;
                                }
                            }
                        }
                        // check for action tiles
                        if (buildingsLayer.Tiles[x, y] != null)
                        {
                            if (buildingsLayer.Tiles[x, y].Properties != null && buildingsLayer.Tiles[x, y].Properties.TryGetValue("Action", out string actionValue))
                            {
                                string[] props = actionValue.Split(' ');
                                switch (props[0])
                                {
                                    case "Mailbox":
                                    case "Warp":
                                    case "LumberPile":
                                    case "Message":
                                    case "Jukebox":
                                        // delete action
                                        buildingsLayer.Tiles[x, y].Properties.Remove("Action");
                                        break;
                                    case "MinecartTransport":
                                        // update internal minecarts
                                        if (props[1] != "Default")
                                        {
                                            buildingsLayer.Tiles[x, y].Properties["Action"] = actionValue.Replace(props[1], $"sdr.{props[1]}");
                                            if (!editedtMineCart)
                                            {
#if DEBUG
                                                logger.Log($"Updating Mine Cart Network {props[1]}");
#endif
                                                modDataService.MineCartNetworksToEdit.Add(props[1], newLocationName);
                                                editedtMineCart = true;
                                            }
                                        }
                                        break;
                                    default:
#if DEBUG
                                        Console.WriteLine($"New prop: {props[0]}");
#endif
                                        break;
                                }
                            }
                            if (buildingsLayer.Tiles[x, y].TileIndex == 1955)
                            {
                                buildingsLayer.Tiles[x, y] = null;
                                // remove bottom of mailbox
                            }
                        }
                        if (frontLayer.Tiles[x, y] != null && frontLayer.Tiles[x, y].TileIndex == 1930)
                        {
                            frontLayer.Tiles[x, y] = null;
                            // remov top of mailbox
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
