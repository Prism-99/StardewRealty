using StardewModdingAPI.Events;
using StardewValley.GameData.Locations;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using StardewRealty.SDV_Realty_Interface;
using Prism99_Core.Utilities;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using StardewValley.Buildings;

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
        public LocationsDataProvider(ICustomBuildingService customBuildingService, IModDataService modDataService)
        {
            //_framework = frameworkService.framework;
            _customBuildingService = customBuildingService;
            _modDataService = modDataService;
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
                Dictionary<string, LocationData> detail = (Dictionary<string, LocationData>)asset.Data;

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

                foreach (Expansions.FarmExpansionLocation location in locations)
                {
                    try
                    {
                        ExpansionPack contentPack = _modDataService.validContents[location.Name];

                        ExpansionDetails expansionDetails = _modDataService.expDetails[location.Name];

                        LocationData locationData = new LocationData
                        {
                            DisplayName = location.DisplayName,
                            ExcludeFromNpcPathfinding = false,
                            FishAreas = new Dictionary<string, FishAreaData>(),
                            ArtifactSpots = new List<ArtifactSpotDropData> { },
                            CanPlantHere = true
                        };
                        if (contentPack.CaveEntrance != null)
                        {
                            locationData.DefaultArrivalTile = new Point(contentPack.CaveEntrance.WarpIn.X, contentPack.CaveEntrance.WarpIn.Y);
                        }
                        //
                        //  add artifact details
                        //
                        if (expansionDetails.Artifacts != null && expansionDetails.Artifacts.Count > 0)
                        {
                            foreach (ArtifactData arti in expansionDetails.Artifacts)
                            {
                                ArtifactSpotDropData artifactData = new ArtifactSpotDropData
                                {
                                    ItemId = arti.ArtifactId,
                                    Chance = arti.Chance
                                };
                                if (!string.IsNullOrEmpty(arti.Season) && arti.Season != "Any")
                                {
                                    //
                                    //  add seasonal condition
                                    //
                                    artifactData.Condition = $"LOCATION_SEASON Here {arti.Season}";
                                }

                                locationData.ArtifactSpots.Add(artifactData);
                            }
                        }
                        //
                        //  add fishing details
                        //
                        if (contentPack.FishAreas != null)
                        {
                            foreach (string fishAreaKey in contentPack.FishAreas.Keys)
                            {
                                locationData.FishAreas.Add(fishAreaKey, FishAreaDetails.GetData(contentPack.FishAreas[fishAreaKey]));
                                if (expansionDetails.FishAreas != null && expansionDetails.FishAreas.ContainsKey(fishAreaKey))
                                {
                                    foreach (FishStockData stockData in expansionDetails.FishAreas[fishAreaKey].StockData)
                                    {
                                        Season s;
                                        SDVUtilities.TryParseEnum(stockData.Season, out s);
                                        locationData.Fish.Add(new SpawnFishData
                                        {
                                            FishAreaId = fishAreaKey,
                                            ItemId = stockData.FishId,
                                            IgnoreFishDataRequirements = true,
                                            Season = s
                                        });
                                    }
                                }
                            }
                        }

                        if (detail.ContainsKey(location.Name))
                            detail.Remove(location.Name);

                        detail.Add(location.Name, locationData);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Location.HandleEdit", ex);
                    }
                }
                //
                //   add custom building interiors
                //
                foreach (KeyValuePair<string, Buildings.ICustomBuilding> building in _customBuildingService.customBuildingManager.CustomBuildings)
                {
                    try
                    {
                        detail.Add(building.Key, new LocationData
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
                foreach(var subLocation in _modDataService.SubLocations)
                {
                    try
                    {
                        detail.Add(subLocation.Key, new LocationData
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
            });
        }

        public override void OnGameLaunched()
        {

        }
    }
}
