using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using System.Diagnostics;
using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewModHelpers;
using System.IO;
using xTile.Tiles;
using xTile;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewValley.Locations;
using System.Linq;
using System.Collections.Generic;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.Expansions;
using StardewValley.Buildings;
using SDV_Realty_Core.Framework.ServiceInterfaces.Integrations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Crops;



namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    /// <summary>
    /// Container for the main utilitie Services
    /// </summary>
    internal abstract class IUtilitiesService : IService
    {
        public enum PopupType
        {
            HUD,
            Dialogue
        }
        public enum LocationType
        {
            Vanilla,
            Decoratable,
            FarmExpansion
        }
        private const string baseFarmName = "Farm";

        public override Type ServiceType => typeof(IUtilitiesService);
        public abstract IModHelperService ModHelperService { get; }
        public abstract IManifestService ManifestService { get; }
        public abstract IMonitorService MonitorService { get; }
        public abstract IConfigService ConfigService { get; }
        public abstract IPatchingService PatchingService { get; }
        public abstract IGameEventsService GameEventsService { get; }
        public abstract ICustomEventsService CustomEventsService { get; }
        public abstract IMapLoaderService MapLoaderService { get; }
        public abstract IMapUtilities MapUtilities { get; }
        public abstract IGameEnvironmentService GameEnvironment { get; }
        public abstract IModDataService ModDataService { get; }
        public abstract IPlayerComms PlayerComms { get; }
        public bool HaveExpansions = false;

        public string GetCaller()
        {
            StackTrace stackTrace = new StackTrace();
            // Get calling method name
            return $"{stackTrace.GetFrame(2).GetMethod().DeclaringType.Name}.{stackTrace.GetFrame(2).GetMethod().Name}";
        }
        public void WarpPlayerHome()
        {
            MapWarp homeWarp = GetPlayerHomeWarp();

            DelayedAction.warpAfterDelay(homeWarp.ToMap, new Point(homeWarp.ToX, homeWarp.ToY - 2), 100);
        }
        public MapWarp GetPlayerHomeWarp()
        {

            GameLocation glHome = Game1.getLocationFromName(Game1.player.homeLocation.Value);

            var oWarps = glHome.warps.Where(p => p.TargetName == "Farm");

            if (oWarps.Any())
            {
                Warp homeWarp = oWarps.First();
                return new MapWarp
                {
                    ToMap = Game1.player.homeLocation.Value,
                    ToX = homeWarp.X,
                    ToY = homeWarp.Y - 1
                };

            }

            return null;
        }
        public void InvalidateCache(string cacheName)
        {
            if (string.IsNullOrEmpty(cacheName)) return;

            try
            {
                ModHelperService.modHelper.GameContent.InvalidateCache(cacheName);
            }
            catch (Exception ex)
            {
                logger.Log($"Error invalidating cache {cacheName}", LogLevel.Error);
                logger.LogError("InvalidateCache", ex);
            }
        }
        public bool IsMasterGame()
        {
            return (Game1.IsMultiplayer && Game1.IsMasterGame) || !Game1.IsMultiplayer;
        }
        public bool IsTrainOpen()
        {
            return Game1.stats.DaysPlayed > 31;
        }
        public string RemoveMapExtensions(string mapNme)
        {
            return mapNme.Replace(".tmx", "", StringComparison.OrdinalIgnoreCase).Replace(".xnb", "", StringComparison.OrdinalIgnoreCase).Replace(".tbin", "", StringComparison.OrdinalIgnoreCase);
        }
        public string RemoveTextureExtensions(string mapNme)
        {
            return mapNme.Replace(".png", "", StringComparison.OrdinalIgnoreCase).Replace(".xnb", "", StringComparison.OrdinalIgnoreCase).Replace(".tbin", "", StringComparison.OrdinalIgnoreCase);
        }
        public string GetMapUniqueName(ExpansionPack expansionPack)
        {
            return $"{expansionPack.LocationName}.{RemoveMapExtensions(expansionPack.MapName)}";
        }
        public bool TryAddGameLocation(string locationName, string mapFilename, string locationDisplayName, IContentManagerService ContentManager, IExpansionManager expansionManager, ILocationTunerIntegrationService? locationTuner, out GameLocation location, out Map map, LocationType locationType = LocationType.Vanilla, bool addLocation = true)
        {
            bool result = true;
            location = null;

            map = MapLoaderService.LoadMap(Path.Combine(GameEnvironment.ModPath, "data", "Maps", locationName, mapFilename).Replace(GameEnvironment.GamePath, ""), locationName, false, false);
            string assetPath = RemoveMapExtensions(SDVPathUtilities.AssertAndNormalizeAssetName(Path.Combine("SDR", "Expansion", locationName, mapFilename)));

            ContentManager.ExternalReferences[assetPath] = map;
            ////
            ////  add warproom tilesheet sources
            ////
            foreach (TileSheet tileSheet in map.TileSheets)
            {
                tileSheet.ImageSource = tileSheet.ImageSource.Replace("\\", FEConstants.AssetDelimiter);
                if (!string.IsNullOrEmpty(tileSheet.ImageSource) && tileSheet.ImageSource.StartsWith($"SDR{FEConstants.AssetDelimiter}{locationName}"))
                {
                    string sFilename = Path.GetFileName(tileSheet.ImageSource);
                    string sFullname = Path.Combine(GameEnvironment.ModPath, "data", "Maps", locationName, sFilename);

                    if (!ContentManager.ExternalReferences.ContainsKey(tileSheet.ImageSource))
                    {
                        ContentManager.AddExternalReference(tileSheet.ImageSource, new StardewBitmap(sFullname).Texture());

                        if (sFilename.StartsWith("spring_"))
                        {
                            // add seasonal tilesheets
                            //
                            // summer
                            sFullname = Path.Combine(GameEnvironment.ModPath, "data", "Maps", locationName, sFilename.Replace("spring_", "summer_"));
                            if (File.Exists(sFullname))
                                ContentManager.AddExternalReference(tileSheet.ImageSource.Replace("spring_", "summer_"), new StardewBitmap(sFullname).Texture());
                            else
                                logger.Log($"Missing 'summer' tihesheet for {tileSheet.ImageSource}" , LogLevel.Warn);
                            // fall
                            sFullname = Path.Combine(GameEnvironment.ModPath, "data", "Maps", locationName, sFilename.Replace("spring_", "fall_"));
                            if (File.Exists(sFullname))
                                ContentManager.AddExternalReference(tileSheet.ImageSource.Replace("spring_", "fall_"), new StardewBitmap(sFullname).Texture());
                            else
                                logger.Log($"Missing 'fall' tihesheet for {tileSheet.ImageSource}", LogLevel.Warn);
                            // winter
                            sFullname = Path.Combine(GameEnvironment.ModPath, "data", "Maps", locationName, sFilename.Replace("spring_", "winter_"));
                            if (File.Exists(sFullname))
                                ContentManager.AddExternalReference(tileSheet.ImageSource.Replace("spring_", "winter_"), new StardewBitmap(sFullname).Texture());
                            else
                                logger.Log($"Missing 'winter' tihesheet for {tileSheet.ImageSource}", LogLevel.Warn);
                        }


                        //if (Path.GetFileName(tileSheet.ImageSource) != Path.GetFileNameWithoutExtension(tileSheet.ImageSource))
                        //{
                        //    int end = tileSheet.ImageSource.LastIndexOf('.');
                        //    if (end > -1)
                        //    {
                        //        ContentManager.ExternalReferences.Add(tileSheet.ImageSource.Substring(0, end ), new StardewBitmap(sFullname).Texture());
                        //    }
                        //}
                    }
                }
            }
            List<IMapUtilities.MapLightSource> lights = MapUtilities.ReadLightSources(map);
            if (lights.Any())
            {
                List<string> lightProps = new();
                List<string> dayNight = new();
                foreach (var light in lights)
                {
                    lightProps.Add($"{light.x} {light.y + light.offset} {light.lightType}");
                    if (light.lightType == 4)
                        lightProps.Add($"{light.x} {light.y} {light.lightType}");

                    dayNight.Add($"Front {light.x} {light.y}");
                }

                if (map.Properties.TryGetValue("Light", out var lightProp))
                    map.Properties["Light"] = $"{lightProp.ToString().Trim()} {string.Join(" ", lightProps)}";
                else
                    map.Properties["Light"] = string.Join(" ", lightProps);

                map.Properties["DayTiles"] = string.Join(" 907 ", dayNight) + " 907";
                map.Properties["NightTiles"] = string.Join(" 908 ", dayNight) + " 908";
            }
            if (addLocation)
            {
                switch (locationType)
                {
                    case LocationType.Decoratable:
                        location = new DecoratableLocation(assetPath, locationName);
                        break;
                    case LocationType.Vanilla:
                        location = new GameLocation(assetPath, locationName);
                        break;
                    case LocationType.FarmExpansion:
                        location = new FarmExpansionLocation(map, assetPath, locationName, (SDVLogger)logger.CustomLogger, this, ModDataService, locationTuner);
                        break;
                }
                location.mapPath.Set(assetPath);
                location.uniqueName.Value = locationName;
                location.name.Value = locationName;

                location.modData.Add(IModDataKeysService.FEExpansionDisplayName, locationDisplayName);
                expansionManager.expansionManager.AddGameLocation(location, "TryAddGameLocation");
            }

            return result;
        }
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
        internal void RepairAllLocationBuildingWarps(FarmExpansionLocation expansionLocation)
        {
            logger.Log($"    Repairing building warps for location {expansionLocation.NameOrUniqueName}, buildings: {expansionLocation.buildings.Count()}", LogLevel.Debug);
            foreach (Building building in expansionLocation.buildings)
            {
                FixBuildingWarps(building, expansionLocation.NameOrUniqueName);
            }
        }
        internal Rectangle GetGridLocationCoordinates(int gridId, Rectangle container)
        {
            int row;
            int col;
            if (gridId < 0)
            {
                row = 0;
                col = 0;
            }
            else
            {
                row = (gridId - 1) / 3;
                col = (gridId - 1) % 3;
            }
            int topMargin = 30;
            int leftMargin = 5;
            int boxPadding = 4;
            int imageBoxWidth = (container.Width - boxPadding * 5) / 4;
            int imageBoxHeight = (container.Height - boxPadding * 2 - topMargin) / 3;
            int imageWidth = imageBoxWidth - boxPadding * 2;
            int imageHeight = imageBoxHeight - boxPadding * 2;
            int left = container.Left - leftMargin * 2 + (imageBoxWidth + boxPadding) * 3;
            int top = container.Top;
            //switch (IGridManager.MaxFarms)
            //{
            //    case 13:
            imageBoxWidth = (container.Width - boxPadding * 6) / 5;
            imageWidth = imageBoxWidth - boxPadding * 2;
            left = container.Left - leftMargin * 2 + (imageBoxWidth + boxPadding) * 4;
            //break;
            //}
            switch (gridId)
            {
                case int n when n < 0: // Home square
                    int miniRow = gridId * -1 - 1;
                    int heightDivsor = 4;
                    //return new Rectangle(left + boxPadding, top + topMargin + imageBoxHeight + (imageHeight / heightDivsor + boxPadding / heightDivsor) * miniRow, imageWidth , imageHeight / heightDivsor - boxPadding / heightDivsor);
                    return new Rectangle(left + boxPadding, top + topMargin + imageBoxHeight + (imageHeight / heightDivsor + boxPadding / heightDivsor) * miniRow, imageWidth, imageHeight / heightDivsor);
                case 0: //by the Backwoods
                    return new Rectangle(left + boxPadding, top + topMargin + boxPadding, imageWidth, imageHeight);
                //case 1:
                //    return new Rectangle(63, 64 + iMapIndex * 14, iImageWidth, iImageHeight);
                default:
                    return new Rectangle(left - row * imageBoxWidth - imageBoxWidth + boxPadding, top + topMargin + col * imageBoxHeight + boxPadding, imageWidth, imageHeight);
            }

        }
        internal void PopMessage(string message, PopupType type = PopupType.HUD, int delay = 3000)
        {
            switch (type)
            {
                case PopupType.Dialogue:
                    Game1.drawObjectDialogue(message);
                    break;
                default:
                    Game1.addHUDMessage(new HUDMessage(message) { noIcon = true, timeLeft = delay });
                    break;
            }
        }
        internal int GetSeedPrice(string seedId)
        {
            int price = 25;
            if (Game1.objectData.TryGetValue(seedId, out ObjectData seed))
            {
                if (seed.Price > 0)
                    price = seed.Price;
                else
                {
                    if (Game1.cropData.TryGetValue(seedId, out CropData cropData))
                    {
                        if (Game1.objectData.TryGetValue(cropData.HarvestItemId, out ObjectData crop))
                        {
                            price = (int)(crop.Price * 0.4f);
                        }
                    }
                }
            }

            return price;
        }
    }
}
