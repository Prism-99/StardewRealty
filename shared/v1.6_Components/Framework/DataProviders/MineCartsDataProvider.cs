using StardewModdingAPI.Events;
using StardewValley.GameData.Minecarts;
using System.Linq;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;

namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edits Data/MineCarts to add SDR expansion mine cart points
    /// Uses FEFramework.farmExpansions for source data
    /// </summary>
    internal class MineCartsDataProvider : IGameDataProvider
    {
        public override string Name => "Data/Minecarts";
        private IModDataService modDataService;
        private IUtilitiesService _utilitiesService;

        public MineCartsDataProvider(IModDataService modDataService,IUtilitiesService utilitiesService)
        {
            this.modDataService = modDataService;
            _utilitiesService= utilitiesService;
        }
        public override void CheckForActivations()
        {
            
        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            List<KeyValuePair<string, ExpansionPack>> mineCartLocs = modDataService.validContents.Where(p => p.Value.MineCarts.Any()).ToList();

            e.Edit(asset =>
            {
                Dictionary<string, MinecartNetworkData> carts = (Dictionary<string, MinecartNetworkData>)asset.Data;

                foreach (KeyValuePair<string, ExpansionPack> contentPack in mineCartLocs)
                {
                    if (modDataService.farmExpansions[contentPack.Key].Active)
                    {
                        foreach (ExpansionPack.MineCartSpot mineCartDesitination in contentPack.Value.MineCarts)
                        {
                            carts["Default"].Destinations.Add(new MinecartDestinationData
                            {
                                Id = contentPack.Key + "." + mineCartDesitination.MineCartDisplayName,
                                TargetLocation = contentPack.Key,
                                Condition=mineCartDesitination.Condition,
                                TargetDirection = mineCartDesitination.MineCartDirection,
                                DisplayName = mineCartDesitination.MineCartDisplayName ?? contentPack.Key,
                                TargetTile = new Point(mineCartDesitination.Exit.X, mineCartDesitination.Exit.Y)
                            });
                        }
                    }
                }
            });
        }

        public override void OnGameLaunched()
        {
            _utilitiesService.InvalidateCache(Name);
        }
    }
}
