using StardewModdingAPI.Events;
using StardewValley.GameData.Minecarts;
using System.Linq;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

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
            var mineCartLocs = modDataService.validContents.Where(p => p.Value.MineCartEnabled).ToList();

            e.Edit(asset =>
            {
                var carts = (Dictionary<string, MinecartNetworkData>)asset.Data;

                foreach (var contentPack in mineCartLocs)
                {
                    if (modDataService.farmExpansions[contentPack.Key].Active)
                    {
                        carts["Default"].Destinations.Add( new MinecartDestinationData
                        {
                            Id= contentPack.Key,
                            TargetLocation=contentPack.Key,
                            TargetDirection = contentPack.Value.MineCartDirection,
                            DisplayName = contentPack.Value.MineCartDisplayName ?? contentPack.Key,
                            TargetTile = new Point(contentPack.Value.MineCart.X, contentPack.Value.MineCart.Y)
                        });
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
