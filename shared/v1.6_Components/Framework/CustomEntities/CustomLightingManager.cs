﻿using StardewModdingAPI.Events;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.Framework.CustomEntities.Buildings;
using SDV_Realty_Core.Framework.Buildings;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
namespace SDV_Realty_Core.Framework.CustomEntities
{
    internal class CustomLightingManager
    {
        private bool buildingEnabled;
        private bool pondsEnabled;
        private int lightSourceKey = 555;
        private float lightLevel = 1f;
        private ILoggerService logger;
        private Dictionary<string, List<NightLight>> vanillaLights;
         private IModDataService modDataService;
        private CustomBuildingManager _buildingManager;
#if v169
        private Dictionary<Guid, List<string>> addedLights = new();
#else
        private Dictionary<Guid, List<int>> addedLights = new();
#endif
        private string lightKey = "prism.advize.sdr.buidling.brightness";
        public CustomLightingManager(ILoggerService oLog, IModDataService modDataService,  ICustomBuildingService customBuildingService)
        {
            logger = oLog;
            this.modDataService = modDataService;
            _buildingManager = customBuildingService.customBuildingManager;
            PopulateVanillaLights();
            lightLevel = modDataService.Config.LightLevel;
            buildingEnabled = modDataService.Config.EnableBuildingLights;
            pondsEnabled = modDataService.Config.AddFishPondLight;
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
            //
            //  check if custom buildings have changed
            //
            if (buildingEnabled && !modDataService.Config.EnableBuildingLights)
            {
                // lights disabled, remove all active lights
                buildingEnabled = false;
                RemoveAllCustomBuildingLights();
            }
            else if (!buildingEnabled && modDataService.Config.EnableBuildingLights)
            {
                // lights enabled, add all active lights
                buildingEnabled = true;
                AddAllCustomBuildingLights();
            }
            //
            //  check is fish ponds changed
            //
            if (pondsEnabled && !modDataService.Config.AddFishPondLight)
            {
                //  fish pond lights disabled, remove all active lights
                pondsEnabled = false;
                RemoveAllFishPondLights();
            }
            else if (!pondsEnabled && modDataService.Config.AddFishPondLight)
            {
                // lights enabled, add all active lights
                pondsEnabled = true;
                AddAllPondLights();
            }
            // check for brightness level change
            if (modDataService.Config.LightLevel != lightLevel)
            {
                Utility.ForEachBuilding(CheckBuildingForChange, true);
                lightLevel = modDataService.Config.LightLevel;
            }
        }
        private bool CheckBuildingForChange(Building building)
        {
            if (addedLights.ContainsKey(building.id.Value))
            {
                if (!building.modData.ContainsKey(lightKey))
                    SetBuildingBrightness(building, modDataService.Config.LightLevel, false);
            }
            return true;
        }
        /// <summary>
        /// Adds light source details for vanilla buildings
        /// -Fish Pond
        /// </summary>
        private void PopulateVanillaLights()
        {
            vanillaLights = new Dictionary<string, List<NightLight>> { };

            if (modDataService.Config.AddFishPondLight)
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
            if (e.Added != null)
            {
                foreach (Building buildingAdded in e.Added)
                {
                    if (buildingAdded is FishPond && string.IsNullOrEmpty(buildingAdded.parentLocationName.Value))
                    {
                        buildingAdded.parentLocationName.Value = e.Location.NameOrUniqueName;
                    }
                    if (HasNightLight(buildingAdded))
                    {
                        AddBuildingLightSource(buildingAdded);
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
        private void RemoveAllFishPondLights()
        {
            Utility.ForEachBuilding(building =>
            {
                if (HasNightLight(building))
                {
                    if (building.buildingType.Value != "Fish Pond")
                        return true;

                    // cleanup modData entry for custom building lightness level
                    if (building.modData.ContainsKey(lightKey))
                        building.modData.Remove(lightKey);

                    RemoveLightSource(building, building.GetParentLocation(), null);
                }
                return true;
            });
        }
        /// <summary>
        /// Removes all active building light sources
        /// </summary>
        private void RemoveAllCustomBuildingLights()
        {
            Utility.ForEachBuilding(building =>
            {
                if (HasNightLight(building))
                {
                    if (building.buildingType.Value == "Fish Pond")
                        return true;

                    // cleanup modData entry for custom building lightness level
                    if (building.modData.ContainsKey(lightKey))
                        building.modData.Remove(lightKey);

                    RemoveLightSource(building, building.GetParentLocation(), null);
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
                if (modDataService.Config.EnableBuildingLights)
                    AddLightSources(building, buildingData.NightLights, buildingBrightness);
            }
            else if (vanillaLights.TryGetValue(building.buildingType.Value, out var lightList))
            {
                if (modDataService.Config.AddFishPondLight)
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
        private string GetFormattedKey(int index)
        {
            return $"StardewRealty.{index}";
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
            if (building.GetParentLocation() != null)
            {
                foreach (NightLight nightLight in nightLights)
                {
#if v169
                    while (building.GetParentLocation().sharedLights.ContainsKey(GetFormattedKey(lightSourceKey)))
#else
 while (building.GetParentLocation().sharedLights.ContainsKey(lightSourceKey))
#endif
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
                    // 7 - spotlight facing up (movie screen)
                    // 8 - normal level rectangle

                    Vector2 lightPosition = GetLightPosition(building, nightLight);

                    if (!BuildingHasActiveLightSource(building, lightPosition))
                    {
#if v169
                        string key = GetFormattedKey(lightSourceKey++);

                        lightSource = new LightSource
        (key,
        nightLight.NightLightType,
        lightPosition,
        nightLightRadius,
        lightColor,
        LightSource.LightContext.MapLight,
        0L);
                        building.GetParentLocation().sharedLights.Add(key, lightSource);
                        if (!building.modData.ContainsKey(lightKey))
                            building.modData[lightKey] = key;

                        if (addedLights.ContainsKey(building.id.Value))
                        {

                            addedLights[building.id.Value].Add(key);
                        }
                        else
                        {
                            addedLights.Add(building.id.Value, new() { { key } });
                        }
#else
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
#endif
                    }
                }
            }
            else
            {
                logger.Log($"Building missing parent: {building.buildingType}, no lights added.", LogLevel.Warn);
            }
        }
        private void AddAllPondLights()
        {
            if (modDataService.Config.AddFishPondLight)
            {
                Utility.ForEachBuilding(building =>
                {
                    if (!building.buildingType.Value.Contains("Pond", StringComparison.CurrentCultureIgnoreCase))
                        return true;

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
        /// Adds all active building lights.
        /// </summary>
        private void AddAllCustomBuildingLights()
        {
            if (modDataService.Config.EnableBuildingLights)
            {
                Utility.ForEachBuilding(building =>
                {
                    if (building.buildingType.Value == "Fish Pond")
                        return true;

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
            AddAllCustomBuildingLights();
            AddAllPondLights();
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
