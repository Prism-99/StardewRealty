using StardewModdingAPI.Events;
using StardewValley.GameData.Minecarts;
using System.Linq;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.Locations;
using System;


namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edits Data/MineCarts to add SDR expansion mine cart points
    /// Uses IModDataService.farmExpansions for source data
    /// </summary>
    internal class MineCartsDataProvider : IGameDataProvider
    {
        public override string Name => "Data/Minecarts";
        private IModDataService modDataService;
        private IUtilitiesService _utilitiesService;

        public MineCartsDataProvider(IModDataService modDataService, IUtilitiesService utilitiesService)
        {
            this.modDataService = modDataService;
            _utilitiesService = utilitiesService;
            utilitiesService.GameEventsService.AddSubscription("AssetReadyEventArgs", Content_AssetLoaded);
        }
        public override void CheckForActivations()
        {

        }
        private void Content_AssetLoaded(EventArgs eventArgs)
        {
            AssetReadyEventArgs assetReadyEventArgs = (AssetReadyEventArgs)eventArgs;

            Dictionary<string, MinecartNetworkData> carts;
            if (assetReadyEventArgs.NameWithoutLocale.Name == "Data/Minecarts")
            {
                carts = DataLoader.Minecarts(Game1.content);
                var networks = modDataService.validContents.Where(p => p.Value.InternalMineCarts != null && (p.Value.InternalMineCarts.Destinations?.Any() ?? false));
                foreach (var pair in networks)
                {
                    pair.Value.InternalMineCarts.Destinations.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));
                    carts.Add(pair.Key, pair.Value.InternalMineCarts);
                }
            }
            else if (false && assetReadyEventArgs.NameWithoutLocale.Name == "Data/Minecarts" && modDataService.MineCartNetworksToEdit.Count > 0)
            {
                //
                //  edit additional farm networks
                //

                foreach (var keyValue in modDataService.MineCartNetworksToEdit)
                {
                    //
                    //  clone network
                    //
                    if (carts.TryGetValue(keyValue.Key, out var mineCartNetwork))
                    {
                        carts.Add($"sdr.{keyValue.Key}", new MinecartNetworkData
                        {
                            UnlockCondition = mineCartNetwork.UnlockCondition,
                            LockedMessage = mineCartNetwork.LockedMessage,
                            ChooseDestinationMessage = mineCartNetwork.ChooseDestinationMessage,
                            BuyTicketMessage = mineCartNetwork.BuyTicketMessage,
                            Destinations = new List<MinecartDestinationData>()
                        }
                        );

                        foreach (var dest in mineCartNetwork.Destinations.OrderBy(p => p.DisplayName))
                        {
                            carts[$"sdr.{keyValue.Key}"].Destinations.Add(
                                new MinecartDestinationData
                                {
                                    BuyTicketMessage = dest.BuyTicketMessage,
                                    Condition = dest.Condition,
                                    TargetDirection = dest.TargetDirection,
                                    CustomFields = dest.CustomFields,
                                    DisplayName = dest.DisplayName,
                                    Id = dest.Id,
                                    Price = dest.Price,
                                    TargetLocation = dest.TargetLocation == "Farm" ? keyValue.Value : dest.TargetLocation,
                                    TargetTile = dest.TargetTile
                                }
                                );
                        }
                    }
                }
            }
        }
        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            List<KeyValuePair<string, ExpansionPack>> mineCartLocs = modDataService.validContents.Where(p => p.Value.MineCarts.Any()).ToList();

            e.Edit(asset =>
            {
                Dictionary<string, MinecartNetworkData> carts = (Dictionary<string, MinecartNetworkData>)asset.Data;
                //
                //  add expansion networks
                //
                foreach (KeyValuePair<string, ExpansionPack> contentPack in mineCartLocs)
                {
                    if (modDataService.farmExpansions.TryGetValue(contentPack.Key, out var _))
                    {
                        if (modDataService.farmExpansions[contentPack.Key].Active)
                        {
                            foreach (ExpansionPack.MineCartSpot mineCartDesitination in contentPack.Value.MineCarts)
                            {
                                carts["Default"].Destinations.Add(new MinecartDestinationData
                                {
                                    Id = contentPack.Value.LocationName + "." + mineCartDesitination.MineCartDisplayName.Replace(" ", "_"),
                                    TargetLocation = contentPack.Key,
                                    Condition = mineCartDesitination.Condition,
                                    TargetDirection = mineCartDesitination.MineCartDirection,
                                    DisplayName = mineCartDesitination.MineCartDisplayName ?? contentPack.Key,
                                    TargetTile = new Point(mineCartDesitination.Exit.X, mineCartDesitination.Exit.Y)
                                });
                            }
                        }
                    }
                }

                //
                //  add StardewMeadows
                //
                carts["Default"].Destinations.Add(new MinecartDestinationData
                {
                    Id = WarproomManager.StardewMeadowsLoacationName,
                    TargetLocation = WarproomManager.StardewMeadowsLoacationName,
                    //TargetDirection= 
                    DisplayName = WarproomManager.StardewMeadowsDisplayName,
                    TargetTile = new Point(4, 44)
                });
            });
        }

        public override void OnGameLaunched()
        {
            _utilitiesService.InvalidateCache(Name);
        }
    }
}
