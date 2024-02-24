using StardewModdingAPI.Events;
using StardewValley.GameData.LocationContexts;
using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;

namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class LocationContextsDataProvider : IGameDataProvider
    {
        public override string Name => "Data/LocationContexts";
        private ICustomLocationContextService customLocationContextService;
        public LocationContextsDataProvider(ICustomLocationContextService customLocationContextService)
        {
            this.customLocationContextService = customLocationContextService;
        }
        public override void CheckForActivations()
        {
            
        }

        /// <summary>
        /// Edits Data/LocationContexts to apply required Contexts
        /// Also adds all SDR Expansions to the 'Default' context to support
        /// Tree and Crop planting
        /// </summary>
        /// <param name="e">The game data to be to be edited</param>
        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                var contextData = asset.AsDictionary<string, LocationContextData>().Data;
                //
                //  fix winter condition
                //  didn't do anything
                //var winter= contextData["Default"].WeatherConditions.Where(p => p.Condition.StartsWith("SEASON winter")).First();

                //winter.Condition = winter.Condition.Replace("SEASON winter", "LOCATION_SEASON Here winter");
                //
                //  add all expansions to the Default context
                //
                //foreach (string exp in FEFramework.farmExpansions.Keys)
                //{
                //    contextData["Default"].PlantableLocations.Add(exp);
                //}
                //
                //  add greenhouses
                //
                //foreach (var building in CustomBuildingManager.CustomBuildings)
                //{
                //    if (building.Value.IsGreenhouse)
                //    {
                //        contextData["Default"].PlantableLocations.Add(building.Value.Name);
                //    }
                //}
                //
                //  add custom LocationContexts
                //
                foreach (string contextKey in customLocationContextService.locationContexts.Keys)
                {
                    //
                    //  use Default as the base for custom contexts
                    //
                    LocationContextData baseContext;

                    if (customLocationContextService.locationContexts[contextKey].CopyDefaultFields)
                    {
                        baseContext = contextData["Default"].Clone();
                        //
                        //  remove revive in mine from Default
                        //                   
                        baseContext.ReviveLocations.RemoveAll(x => x.Location != null && x.Location.Contains("Mine"));
                        //
                        //    add required customizations
                        //
                        //  currently supports:
                        //  WeatherConditions
                        //  SeasonOverride
                        //
                        baseContext.WeatherConditions = customLocationContextService.locationContexts[contextKey].LocationContextData.WeatherConditions;
                        baseContext.SeasonOverride = customLocationContextService.locationContexts[contextKey].LocationContextData.SeasonOverride;
                    }
                    else
                    {
                        baseContext = customLocationContextService.locationContexts[contextKey].LocationContextData;
                    }

                    contextData.Add(contextKey, baseContext);
                }
            });
        }

        public override void OnGameLaunched()
        {
            
        }
    }
}
