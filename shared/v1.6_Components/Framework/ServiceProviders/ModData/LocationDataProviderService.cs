using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewRealty.SDV_Realty_Interface;
using StardewValley.GameData.Locations;
using System;
using System.Collections.Generic;
using System.Text;
using xTile.Dimensions;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    /// <summary>
    /// Provide LocationData for expansions
    /// </summary>
    internal class LocationDataProviderService : ILocationDataProvider
    {
        private IModDataService _modDataService;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IModDataService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            _modDataService = (IModDataService)args[0];
        }

        public override LocationData GetData(string locationName)
        {
            LocationData locationData;
            try
            {
                if (Game1.getLocationFromName(locationName) != null)
                    return Game1.getLocationFromName(locationName).GetData();

                ExpansionPack contentPack = _modDataService.validContents[locationName];
                ExpansionDetails expansionDetails = _modDataService.expDetails[locationName];

                locationData = new LocationData
                {
                    DisplayName = _modDataService.validContents[locationName].DisplayName,
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
            }
            catch (Exception ex)
            {
                logger.LogError("LocationDataProviderService.GetData", ex);
                locationData = null;
            }

            return locationData;
        }
    }
}
