using StardewModdingAPI.Events;
using StardewValley.GameData.Locations;
using System.Collections.Generic;
using System.Linq;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.Locations;

namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Version 1.6
    /// Edits Data/Locations to insert SDR Expansion info
    /// FEFramework.farmExpansions provide source data
    /// </summary>

    internal class LocationsDataProvider : IGameDataProvider
    {
        public override string Name => "Data/Locations";
        private ICustomBuildingService _customBuildingService;
        private IModDataService _modDataService;
        private IExpansionManager _expansionManager;
        public LocationsDataProvider(ICustomBuildingService customBuildingService, IModDataService modDataService, IExpansionManager expansionManager)
        {
            //_framework = frameworkService.framework;
            _customBuildingService = customBuildingService;
            _modDataService = modDataService;
            _expansionManager = expansionManager;
        }
        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            //
            //  add active locations and warproom definition, at game start
            //
            List<Expansions.FarmExpansionLocation> locations = _modDataService.farmExpansions.Values.Where(p => p.Active).ToList();

            e.Edit(asset =>
            {
                Dictionary<string, LocationData> locationData = (Dictionary<string, LocationData>)asset.Data;
                //
                //  add active expansion LocationData
                foreach (var data in _expansionManager.expansionManager.LocationDataCache)
                {
                    locationData.Add(data.Key, data.Value);
                }
                //
                //  add gallery data
                //
                locationData.Add(WarproomManager.GalleryLocationName, new LocationData
                {
                     DisplayName="Art Gallery"
                });
              
                //foreach(var location in detail)
                // {
                //     if (string.IsNullOrEmpty(location.Value.DisplayName))
                //         logger.Log($"{location.Key} does not have a displayname", LogLevel.Debug);
                // }

                //
                //  add warproom for game start creation
                //
                //detail.Add(WarproomManager.WarpRoomLoacationName, new LocationData
                //{
                //    CreateOnLoad = new CreateLocationData
                //    {
                //        AlwaysActive = true,
                //        MapPath = $"SDR/{WarproomManager.WarpRoomLoacationName}/warproom.tmx"
                //    },
                //    DisplayName = "Warproom",
                //    ExcludeFromNpcPathfinding = true
                //});
                //Random rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Minute);
                //foreach (Expansions.FarmExpansionLocation location in locations)
                //{
                //    try
                //    {
                //        ExpansionPack contentPack = _modDataService.validContents[location.Name];

                //        ExpansionDetails expansionDetails = _modDataService.expDetails[location.Name];

                //        LocationData newLocationData = new LocationData
                //        {
                //            DisplayName = location.DisplayName,
                //            ExcludeFromNpcPathfinding = false,
                //            FishAreas = new Dictionary<string, FishAreaData>(),
                //            ArtifactSpots = new List<ArtifactSpotDropData> { },
                //            CanPlantHere = true,
                //            CanHaveGreenRainSpawns = true,
                //            ChanceForClay = rand.Next(1, 5) / 100,
                //            FormerLocationNames= contentPack.FormerLocationNames
                //        };
                //        //
                //        //  set weed and forage data
                //        //
                //        newLocationData.MinDailyWeeds = rand.Next(1, 5);
                //        newLocationData.MaxDailyWeeds = rand.Next(newLocationData.MinDailyWeeds, 14);
                //        newLocationData.FirstDayWeedMultiplier = rand.Next(7, 12);
                //        newLocationData.MinDailyForageSpawn = rand.Next(3, 7);
                //        newLocationData.MaxDailyForageSpawn = rand.Next(newLocationData.MinDailyForageSpawn, 12);

                //        if (contentPack.CaveEntrance != null)
                //        {
                //            newLocationData.DefaultArrivalTile = new Point(contentPack.CaveEntrance.WarpIn.X, contentPack.CaveEntrance.WarpIn.Y);
                //        }
                //        //
                //        //  add artifact details
                //        //
                //        if (expansionDetails.Artifacts != null && expansionDetails.Artifacts.Count > 0)
                //        {
                //            foreach (ArtifactData arti in expansionDetails.Artifacts)
                //            {
                //                ArtifactSpotDropData artifactData = new ArtifactSpotDropData
                //                {
                //                    ItemId = arti.ArtifactId,
                //                    Chance = arti.Chance
                //                };
                //                if (!string.IsNullOrEmpty(arti.Season) && arti.Season != "Any")
                //                {
                //                    //
                //                    //  add seasonal condition
                //                    //
                //                    artifactData.Condition = $"LOCATION_SEASON Here {arti.Season}";
                //                }

                //                newLocationData.ArtifactSpots.Add(artifactData);
                //            }
                //        }
                //        //
                //        //  add fishing details
                //        //
                //        if (contentPack.FishAreas != null)
                //        {
                //            foreach (string fishAreaKey in contentPack.FishAreas.Keys)
                //            {
                //                newLocationData.FishAreas.Add(fishAreaKey, FishAreaDetails.GetData(contentPack.FishAreas[fishAreaKey]));
                //                if (expansionDetails.FishAreas != null && expansionDetails.FishAreas.ContainsKey(fishAreaKey))
                //                {
                //                    foreach (FishStockData stockData in expansionDetails.FishAreas[fishAreaKey].StockData)
                //                    {
                //                        Season s;
                //                        SDVUtilities.TryParseEnum(stockData.Season, out s);
                //                        newLocationData.Fish.Add(new SpawnFishData
                //                        {
                //                            FishAreaId = fishAreaKey,
                //                            ItemId = stockData.FishId,
                //                            IgnoreFishDataRequirements = true,
                //                            Season = s
                //                        });
                //                    }
                //                }
                //            }
                //        }
                //        locationData[location.Name] = newLocationData;
                //    }
                //    catch (Exception ex)
                //    {
                //        logger.LogError("Location.HandleEdit", ex);
                //    }
                //}
                //
                //   add custom building interiors
                //
                foreach (KeyValuePair<string, Buildings.ICustomBuilding> building in _customBuildingService.customBuildingManager.CustomBuildings)
                {
                    try
                    {
                        locationData.Add(building.Key, new LocationData
                        {
                            DisplayName = building.Value.DisplayName,
                            ExcludeFromNpcPathfinding = true,
                            CanPlantHere = building.Value.IsGreenhouse
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Locations.HandleEdit2", ex);
                    }
                }
                //
                //  add sub locations
                //
                foreach (var subLocation in _modDataService.SubLocations)
                {
                    try
                    {
                        locationData.Add(subLocation.Key, new LocationData
                        {
                            DisplayName = subLocation.Value.DisplayName,
                            ExcludeFromNpcPathfinding = true,
                            CanPlantHere = subLocation.Value.IsGreenhouse
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Locations.HandleEdit3", ex);
                    }
                }
                //
                //  add stardew meadows
                //
                locationData.Add(WarproomManager.StardewMeadowsLoacationName, new LocationData
                {
                    DisplayName = "Stardew Meadows",
                    DefaultArrivalTile = WarproomManager.StardewMeadowsSoutherEntrancePoint,
                    CreateOnLoad = new CreateLocationData
                    {
                        AlwaysActive = true,
                        MapPath = WarproomManager.StardewMeadowsMapAssetPath,
                        Type= "SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation,StardewRealty"
                    },
                    MaxDailyForageSpawn = 0,
                    MaxDailyWeeds = 0,
                    MaxSpawnedForageAtOnce = 0,
                    FirstDayWeedMultiplier = 0,
                    CanHaveGreenRainSpawns = false,
                    CanPlantHere = false,
                    MinDailyForageSpawn = 0,
                    MinDailyWeeds = 0,
                    ChanceForClay = 0
                });
            });
        }

        public override void OnGameLaunched()
        {

        }
    }
}
