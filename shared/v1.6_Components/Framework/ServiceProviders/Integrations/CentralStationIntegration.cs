using Pathoschild.Stardew.CentralStation;
using SDV_Realty_Core.Framework.ServiceInterfaces.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Linq;
using static System.Collections.Specialized.BitVector32;


namespace SDV_Realty_Core.Framework.ServiceProviders.Integrations
{
    internal class CentralStationIntegration : ICentralStationIntegrationService
    {
        private IUtilitiesService utilitiesService;
        private IModDataService modDataService;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IModDataService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            utilitiesService = (IUtilitiesService)args[0];
            modDataService = (IModDataService)args[1];

            utilitiesService.GameEventsService.AddSubscription(ServiceInterfaces.Events.IGameEventsService.EventTypes.AllModsLoaded, AddStations);
        }
        public void AddExpansionStations(string expansionName)
        {
            ICentralStationApi? api = utilitiesService.ModHelperService.modHelper.ModRegistry.GetApi<ICentralStationApi>("Pathoschild.CentralStation");
            if (api != null)
            {
                if (modDataService.validContents.TryGetValue(expansionName, out var expansion))
                {
                    if (expansion.TrainStationIn.HasValue)
                    {
                        api.RegisterStop(
                            id: $"{expansionName}.train",
                            displayName: () => $"{expansion.DisplayName} - {I18n.TrainStation()}",
                            toLocation: expansion.LocationName,
                            toTile: expansion.TrainStationIn,
                            toFacingDirection: Game1.down,
                            cost: 0,
                            network: "Train",
                            condition: null
                        );
                    }
                    if (expansion.BoatDockIn.HasValue)
                    {
                        api.RegisterStop(
                            id: $"{expansionName}.boat",
                            displayName: () => $"{expansion.DisplayName} - {I18n.BoatDock()}",
                            toLocation: expansion.LocationName,
                            toTile: expansion.BoatDockIn,
                            toFacingDirection: Game1.down,
                            cost: 0,
                            network: "Boat",
                            condition: null
                        );
                    }
                    if (expansion.BusIn.HasValue)
                    {
                        api.RegisterStop(
                            id: $"{expansionName}.bus",
                            displayName: () => $"{expansion.DisplayName} - {I18n.BusStop()}",
                            toLocation: expansion.LocationName,
                            toTile: expansion.BusIn,
                            toFacingDirection: Game1.down,
                            cost: 0,
                            network: "Bus",
                            condition: null
                        );
                    }
                }
            }
        }
        public void AddStations()
        {
            ICentralStationApi? api = utilitiesService.ModHelperService.modHelper.ModRegistry.GetApi<ICentralStationApi>("Pathoschild.CentralStation");
            if (api != null)
            {
                //
                //  add expansion stations
                //
                var stations = modDataService.validContents.Where(p =>(p.Value.BusIn.HasValue|| p.Value.BoatDockIn.HasValue|| p.Value.TrainStationIn.HasValue) && p.Value.Added);
                if (stations.Any())
                {
                    foreach (var station in stations)
                    {
                        AddExpansionStations(station.Key);
                    }
                }
                //
                //  add expansion boat docks
                //
                //stations = modDataService.validContents.Where(p => p.Value.BoatDockIn != null && p.Value.Added);
                //if (stations.Any())
                //{
                //    foreach (var station in stations)
                //    {
                //        api.RegisterStop(
                //           id: $"{station.Key}.boat",
                //           displayName: () => station.Value.DisplayName,
                //           toLocation: station.Value.LocationName,
                //           toTile: station.Value.TrainStationIn,
                //           toFacingDirection: Game1.down,
                //           cost: 0,
                //           network: "Boat",
                //           condition: null
                //       );
                //    }
                //}
                //var busStops = modDataService.validContents.Where(p => p.Value.BusIn != null && p.Value.Added);
                //if (stations.Any())
                //{
                //    foreach (var station in stations)
                //    {
                //        api.RegisterStop(
                //           id: $"{station.Key}.boat",
                //           displayName: () => station.Value.DisplayName,
                //           toLocation: station.Value.LocationName,
                //           toTile: station.Value.TrainStationIn,
                //           toFacingDirection: Game1.down,
                //           cost: 0,
                //           network: "Boat",
                //           condition: null
                //       );
                //    }
                //}
                //
                //  add expansion bus stops
                //

                //
                //  add internal train and boat stations
                //
                var stations2 = modDataService.Stations;
                if (stations2.Any())
                {
                    foreach (var station in stations2)
                    {
                        bool isTrain = station.Type == Objects.StationDetails.StationType.Train;
                        string key = station.Key;
                        if (string.IsNullOrEmpty(key))
                        {
                            key = $"{station.LocationName}.{station.Network}";
                        }
                        api.RegisterStop(
                           id: key,
                           displayName: () => station.DisplayName,
                           toLocation: station.LocationName,
                           toTile: station.InPoint,
                           toFacingDirection: station.FacingDirection,
                           cost: station.Cost,
                           network: station.Network,
                           condition: station.Condition
                       );
                    }
                }
            }
        }

    }
}
