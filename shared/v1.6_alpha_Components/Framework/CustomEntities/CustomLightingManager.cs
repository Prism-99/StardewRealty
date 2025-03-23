using StardewModdingAPI.Events;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.Framework.CustomEntities.Buildings;
using SDV_Realty_Core.Framework.Buildings;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
namespace SDV_Realty_Core.Framework.CustomEntities
{
    internal class CustomLightingManager
    {
        private bool Enabled;
        private int lightSourceKey = 555;
        private float lightLevel = 1f;
        private ILoggerService logger;
        private Dictionary<string, List<NightLight>> vanillaLights;
        private IUtilitiesService utilitiesService;
        private CustomBuildingManager _buildingManager;
        private Dictionary<Guid, List<int>> addedLights = new();
        private string lightKey = "prism.advize.sdr.buidling.brightness";
        public CustomLightingManager(ILoggerService oLog, IUtilitiesService utilitiesService, ICustomBuildingService customBuildingService)
        {
            logger = oLog;
            this.utilitiesService = utilitiesService;
            _buildingManager = customBuildingService.customBuildingManager;
            PopulateVanillaLights();
            lightLevel = utilitiesService.ConfigService.config.LightLevel;
            Enabled = utilitiesService.ConfigService.config.EnableBuildingLights;
        }
        internal void ReturnedToTitle()
        {
            //
            //  player returned to title, clear added lights list
            //
            addedLights.Clear();
        }
        /// <summary>
        /// Specifies a custom light level for the building
        /// </summary>
        /// <param name="building">Building to be changed</param>
        /// <param name="newLightLevel">New light level 0-1</param>
        public void SetBuildingBrightness(Building building, float newLightLevel, bool isCustom = true)
        {
            if (TryGetNightLights(building, out var rules))
            {
                RemoveLightSource(building, building.GetParentLocation(), rules);
                if (isCustom)
                {
                    // store custom brightness in modData for saving
                    building.modData.Add(lightKey, Math.Clamp(newLightLevel, 0, 1).ToString());
                }
                if (newLightLevel > 0)
                    AddLightSources(building, rules, Math.Clamp(newLightLevel, 0, 1));
            }

        }
        /// <summary>
        /// Check for any config changes
        /// </summary>
        public void ConfigChanged()
        {
            if (Enabled && !utilitiesService.ConfigService.config.EnableBuildingLights)
            {
                // lights disabled, remove all active lights
                Enabled = false;
                RemoveAllLights();
            }
            else if (!Enabled && utilitiesService.ConfigService.config.EnableBuildingLights)
            {
                // lights enabled, add all active lights
                Enabled = true;
                AddAllLights();
            }
            // check for brightness level change
            if (utilitiesService.ConfigService.config.LightLevel != lightLevel)
            {
                Utility.ForEachBuilding(CheckBuildingForChange, true);
                lightLevel = utilitiesService.ConfigService.config.LightLevel;
            }
        }
        private bool CheckBuildingForChange(Building building)
        {
            if (addedLights.ContainsKey(building.id.Value))
            {
                if (!building.modData.ContainsKey(lightKey))
                    SetBuildingBrightness(building, utilitiesService.ConfigService.config.LightLevel, false);
            }
            return true;
        }
        /// <summary>
        /// Adds light source details for vanilla buildings
        /// -Fish Pon
        /// </summary>
        private void PopulateVanillaLights()
        {
            vanillaLights = new Dictionary<string, List<NightLight>> { };

            if (utilitiesService.ConfigService.config.AddFishPondLight)
                //  add fish pond
                vanillaLights.Add("Fish Pond", new List<NightLight> {
                    new NightLight
                    {
                         NightLightPosition="centre",
                         NightLightRadius=2,
                         NightLightType=8
                    }
                });
        }
        /// <summary>
        /// Check for the addition or remvoal of any light enabled buildings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void World_BuildingListChanged(EventArgs eParam)
        {
            BuildingListChangedEventArgs e = (BuildingListChangedEventArgs)eParam;
            if (e.Added != null)
            {
                foreach (Building buildingAdded in e.Added)
                {
                    if (HasNightLight(buildingAdded))
                    {
                        AddBuildingLightSource(buildingAdded);
                    }
                }
            }
            if (e.Removed != null)
            {
                foreach (Building buildingRemoved in e.Removed)
                {
                    if (TryGetNightLights(buildingRemoved, out var rules))
                    {
                        RemoveLightSource(buildingRemoved, e.Location, rules);
                    }
                }
            }
        }

        /// <summary>
        /// Remove the lightsource for a deleted building
        /// </summary>
        /// <param name="building">Building that was deleted</param>
        /// <param name="location">Location the building was deleted from</param>
        private void RemoveLightSource(Building building, GameLocation location, List<NightLight> nightLights)
        {
            if (addedLights.TryGetValue(building.id.Value, out var lights))
            {
                //  remove lightsources from the location
                location.sharedLights.RemoveWhere(p => lights.Contains(p.Key));
            }
            //  remove the building from tracking list
            addedLights.Remove(building.id.Value);

        }
        /// <summary>
        /// Removes all active building light sources
        /// </summary>
        private void RemoveAllLights()
        {
            Utility.ForEachBuilding(building =>
            {
                if (HasNightLight(building))
                {
                    // cleanup modData entry for custom building lightness level
                    if (building.modData.ContainsKey(lightKey))
                        building.modData.Remove(lightKey);

                    RemoveLightSource(building,building.GetParentLocation(),null);
                }
                return true;
            });
        }
        /// <summary>
        /// Returns the configuration of the light sources for the building
        /// </summary>
        /// <param name="building">Building being checked</param>
        /// <param name="nightLights">List of Night parameters</param>
        /// <returns>True if the Building is lit.</returns>
        private bool TryGetNightLights(Building building, out List<NightLight> nightLights)
        {
            nightLights = null;

            if (_buildingManager.CustomBuildings.TryGetValue(building.buildingType.Value, out var buildingData))
            {
                if (buildingData.NightLights != null && buildingData.NightLights.Any())
                {
                    nightLights = buildingData.NightLights;
                    return true;
                }
            }
            if (vanillaLights.TryGetValue(building.buildingType.Value, out nightLights))
                return nightLights != null && nightLights.Any();

            return false;
        }
        /// <summary>
        /// Determines if a Building has NightLight setting
        /// </summary>
        /// <param name="building">Building to be checked</param>
        /// <returns>True if the Building is has NightLight set</returns>
        private bool HasNightLight(Building building)
        {
            if (_buildingManager.CustomBuildings.TryGetValue(building.buildingType.Value, out var buildingData))
            {
                return buildingData.NightLights != null && buildingData.NightLights.Any();
            }

            return vanillaLights.ContainsKey(building.buildingType.Value);
        }

        /// <summary>
        /// Adds a light source to a Building
        /// </summary>
        /// <param name="building">Building to add light source to</param>
        private void AddBuildingLightSource(Building building)
        {
            float buildingBrightness = lightLevel;
            //
            //  check for custom building light level
            //
            if (building.modData.TryGetValue(lightKey, out string customBright))
            {
                float.TryParse(customBright, out buildingBrightness);
            }

            if (_buildingManager.CustomBuildings.TryGetValue(building.buildingType.Value, out var buildingData))
            {
                AddLightSources(building, buildingData.NightLights, buildingBrightness);
            }
            else if (vanillaLights.TryGetValue(building.buildingType.Value, out var lightList))
            {
                AddLightSources(building, lightList, buildingBrightness);
            }
        }
        /// <summary>
        /// Return the vector position of the light source
        /// </summary>
        /// <param name="building">Building to add the lights to</param>
        /// <param name="nightLight">The light source definition</param>
        /// <returns></returns>
        private Vector2 GetLightPosition(Building building, NightLight nightLight)
        {
            //
            //  valid positions:
            //
            //  rearleft, rearcentre, rearright
            //  middleleft,centre,middleright
            //  frontleft,frontcentre,frontright
            //
            //  default position to centre
            //
            Vector2 lightPosition = new Vector2(building.tileX.Value + building.tilesWide.Value / 2F + nightLight.OffsetX, building.tileY.Value + building.tilesHigh.Value / 2F + nightLight.OffsetY) * 64;

            if (!string.IsNullOrEmpty(nightLight.NightLightPosition))
                switch (nightLight.NightLightPosition.ToLower())
                {
                    case "rearleft":
                        lightPosition = new Vector2(building.tileX.Value + nightLight.OffsetX, building.tileY.Value + nightLight.OffsetY) * 64;
                        break;
                    case "rearcentre":
                    case "rearcenter":
                        lightPosition = new Vector2(building.tileX.Value + building.tilesWide.Value / 2F + nightLight.OffsetX, building.tileY.Value + nightLight.OffsetY) * 64;
                        break;
                    case "rearright":
                        lightPosition = new Vector2(building.tileX.Value + building.tilesWide.Value + nightLight.OffsetX, building.tileY.Value + nightLight.OffsetY) * 64;
                        break;
                    case "middleleft":
                        lightPosition = new Vector2(building.tileX.Value + nightLight.OffsetX, building.tileY.Value + building.tilesHigh.Value / 2F + nightLight.OffsetY) * 64;
                        break;
                    case "middleright":
                        lightPosition = new Vector2(building.tileX.Value + building.tilesWide.Value + nightLight.OffsetX, building.tileY.Value + building.tilesHigh.Value / 2F + nightLight.OffsetY) * 64;
                        break;
                    case "frontleft":
                        lightPosition = new Vector2(building.tileX.Value + nightLight.OffsetX, building.tileY.Value + building.tilesHigh.Value + nightLight.OffsetY) * 64;
                        break;
                    case "frontcentre":
                    case "frontcenter":
                        lightPosition = new Vector2(building.tileX.Value + building.tilesWide.Value / 2F + nightLight.OffsetX, building.tileY.Value + building.tilesHigh.Value + nightLight.OffsetY) * 64;
                        break;
                    case "frontright":
                        lightPosition = new Vector2(building.tileX.Value + building.tilesWide.Value + nightLight.OffsetX, building.tileY.Value + building.tilesHigh.Value + nightLight.OffsetY) * 64;
                        break;
                    default:
                        //
                        //  default to centre
                        //

                        break;
                }

            return lightPosition;
        }
        /// <summary>
        /// Add lightSources to the parent location of the building
        /// </summary>
        /// <param name="building">Building to add lights to.</param>
        /// <param name="nightLights">List of night light source parameters</param>
        /// <param name="buildingBrightness">Custom brightness level for the building lights</param>
        private void AddLightSources(Building building, List<NightLight> nightLights, float buildingBrightness)
        {
            buildingBrightness = Math.Clamp(buildingBrightness, 0, 1);

            Color lightColor = Color.Black * Math.Clamp(buildingBrightness, 0, 1);
            foreach (NightLight nightLight in nightLights)
            {
                while (building.GetParentLocation().sharedLights.ContainsKey(lightSourceKey))
                    lightSourceKey++;

                float nightLightRadius = nightLight.NightLightRadius;
                if (nightLightRadius == 0)
                    nightLightRadius = Math.Max(building.tilesHigh.Value, building.tilesWide.Value) / 2F;

                LightSource lightSource;
                // 1 - normal level circular
                // 2 - bright square
                // 3 - invalid
                // 4 - very bright circular
                // 5 - bright circular with shadow ring
                // 6 - bright oval with shadow ring
                // 7 - spotlight facing up
                // 8 - normal level rectangle

                Vector2 lightPosition = GetLightPosition(building, nightLight);

                if (!BuildingHasActiveLightSource(building, lightPosition))
                {
                    lightSource = new LightSource
                            (nightLight.NightLightType,
                            lightPosition,
                            nightLightRadius,
                            lightColor,
                            LightSource.LightContext.MapLight,
                            0L);
                    building.GetParentLocation().sharedLights.Add(lightSourceKey++, lightSource);
                    if (addedLights.ContainsKey(building.id.Value))
                    {
                        addedLights[building.id.Value].Add(lightSourceKey - 1);
                    }
                    else
                    {
                        addedLights.Add(building.id.Value, new() { { lightSourceKey - 1 } });
                    }
                }
            }

        }
        /// <summary>
        /// Adds all active building lights.
        /// </summary>
        private void AddAllLights()
        {
            if (utilitiesService.ConfigService.config.EnableBuildingLights)
            {
                Utility.ForEachBuilding(building =>
                {
                    if (HasNightLight(building))
                    {
                        if (building.modData.ContainsKey(lightKey))
                            building.modData.Remove(lightKey);
                        AddBuildingLightSource(building);
                    }
                    return true;
                });
            }
        }
        /// <summary>
        /// Called on game loaded to set lightsources for all
        /// buildings with defined light sources
        /// </summary>
        public void SaveLoaded(EventArgs e)
        {
            AddAllLights();
        }
        /// <summary>
        /// Checks to see if the Building already has a lightsource
        /// </summary>
        /// <param name="building">Building to be checked</param>
        /// <returns>True if the building has a lightsource defined</returns>
        private bool BuildingHasActiveLightSource(Building building, Vector2 lightLocation)
        {
            var lightSources = building.GetParentLocation().sharedLights.Values.Where(
                p => p.position.X == lightLocation.X && p.position.Y == lightLocation.Y
                );

            logger.Log($"Building {building.buildingType} has light source: {lightSources.Any()}", LogLevel.Debug);
            return lightSources.Any();
        }
    }
}
