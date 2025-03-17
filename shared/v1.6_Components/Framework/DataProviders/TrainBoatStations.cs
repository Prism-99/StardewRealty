using HarmonyLib;
using SDV_Realty_Core.Framework.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathoschild.Stardew.CentralStation;

namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class TrainBoatStations : IGameDataProvider
    {
        IUtilitiesService utilitiesService;
        IModDataService modDataService;
        public TrainBoatStations(IUtilitiesService utilitiesService, IModDataService modDataService)
        {
            this.utilitiesService = utilitiesService;
            this.modDataService = modDataService;
            utilitiesService.GameEventsService.AddSubscription(ServiceInterfaces.Events.IGameEventsService.EventTypes.AllModsLoaded, AddStations);
        }
        //public override string Name => "Mods/Cherry.TrainStation/Destinations";
        public override string Name => "Mods/Pathoschild.CentralStation/Stops";
        public void AddStations()
        {

            ICentralStationApi? api = utilitiesService.ModHelperService.modHelper.ModRegistry.GetApi<ICentralStationApi>("Pathoschild.CentralStation");
            if (api != null)
            {
                //
                //  add expansion train stations
                //
                var stations = modDataService.validContents.Where(p => p.Value.TrainStationIn != null && p.Value.Added);
                if (stations.Any())
                {
                    foreach (var station in stations)
                    {
                        api.RegisterStop(
                            id: station.Key,
                            displayName: () => station.Value.DisplayName,
                            toLocation: station.Value.LocationName,
                            toTile: station.Value.TrainStationIn,
                            toFacingDirection: Game1.down,
                            cost: 0,
                            network: "Train",
                            condition: null
                        );
                    }
                }
                //
                //  add expansion boat docks
                //
                stations = modDataService.validContents.Where(p => p.Value.BoatDockIn != null && p.Value.Added);
                if (stations.Any())
                {
                    foreach (var station in stations)
                    {
                        api.RegisterStop(
                           id: station.Key,
                           displayName: () => station.Value.DisplayName,
                           toLocation: station.Value.LocationName,
                           toTile: station.Value.TrainStationIn,
                           toFacingDirection: Game1.down,
                           cost: 0,
                           network: "Boat",
                           condition: null
                       );
                    }
                }
                //
                //  add internal train and boat stations
                //
                var stations2 = modDataService.Stations;
                if (stations2.Any())
                {
                    foreach (var station in stations2)
                    {
                        bool isTrain = station.Type == Objects.StationDetails.StationType.Train;
                        api.RegisterStop(
                           id: $"{station.LocationName}.{(isTrain ? "train" : "boat")}",
                           displayName: () => station.DisplayName,
                           toLocation: station.LocationName,
                           toTile: station.InPoint,
                           toFacingDirection: station.FacingDirection,
                           cost: station.Cost,
                           network: isTrain ? "Train" : "Boat",
                           condition: station.Condition
                       );
                        //if (station.Type == Objects.StationDetails.StationType.Train)
                        //{
                        //    api.RegisterTrainStation($"{station.LocationName}.train", station.LocationName, new Dictionary<string, string>
                        //    {{ "en",station.DisplayName }}, station.InPoint.X,
                        //        station.InPoint.Y,
                        //        station.Cost, station.IntFacingDirection, null, null
                        //    );
                        //}else if(station.Type== Objects.StationDetails.StationType.Boat) 
                        //{
                        //    api.RegisterBoatStation($"{station.LocationName}.boat", station.LocationName, new Dictionary<string, string>
                        //    {{ "en",station.DisplayName }}, station.InPoint.X,
                        //        station.InPoint.Y,
                        //        station.Cost, station.IntFacingDirection, null, null
                        //    );
                        //}
                    }
                }
            }
        }
        public override void CheckForActivations()
        {
            //
        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            //e.Edit(asset =>
            //{
            //    Type? t = Type.GetType("TrainStation.Framework.ContentModels.StopModel, TrainStation");
            //    if (t != null)
            //    {
            //        //Convert.ChangeType(asset.Data, typeof(List<>).MakeGenericType(t));
            //        //var data = typeof(List<>).MakeGenericType(t)asset.Data;
            //        var stations = modDataService.validContents.Where(p => p.Value.TrainStationIn != null && p.Value.Added);
            //        if (stations.Any())
            //        {
            //            foreach (var station in stations)
            //            {
            //                var stationDetails = Activator.CreateInstance(t);
            //                Traverse.Create(stationDetails).Field("Id").SetValue(station.Key);
            //                Traverse.Create(stationDetails).Field("Conditions").SetValue(null);
            //                Traverse.Create(stationDetails).Field("DisplayName").SetValue(station.Value.DisplayName);
            //                Traverse.Create(stationDetails).Field("ToLocation").SetValue(station.Value.LocationName);
            //                Traverse.Create(stationDetails).Field("ToTile").SetValue(station.Value.TrainStationIn.Value);
            //                Traverse.Create(stationDetails).Field("ToFacingDirection").SetValue("down");
            //                Traverse.Create(stationDetails).Field("Cost").SetValue(0);
            //                Traverse.Create(stationDetails).Field("Network").SetValue("Train");

            //            }
            //        }
            //        var stations2 = modDataService.Stations.Where(p => p.Type == Objects.StationDetails.StationType.Train);
            //        if (stations2.Any())
            //        {
            //            foreach (var station in stations2)
            //            {
            //                var stationDetails = Activator.CreateInstance(t);
            //                Traverse.Create(stationDetails).Field("Id").SetValue(station.LocationName);
            //                Traverse.Create(stationDetails).Field("Conditions").SetValue(station.Condition);
            //                Traverse.Create(stationDetails).Field("DisplayName").SetValue(station.DisplayName);
            //                Traverse.Create(stationDetails).Field("ToLocation").SetValue(station.LocationName);
            //                Traverse.Create(stationDetails).Field("ToTile").SetValue(station.InPoint);
            //                Traverse.Create(stationDetails).Field("ToFacingDirection").SetValue(station.FacingDirection);
            //                Traverse.Create(stationDetails).Field("Cost").SetValue(station.Cost);
            //                Traverse.Create(stationDetails).Field("Network").SetValue(station.Network);

            //                //Traverse.Create(asset.Data).Method("Add").;
            //            }
            //        }
            //    }
            //});
        }

        public override void OnGameLaunched()
        {
            //utilitiesService.InvalidateCache(Name);
        }
    }
}
