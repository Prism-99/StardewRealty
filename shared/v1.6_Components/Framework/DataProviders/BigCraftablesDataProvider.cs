using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;

namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Edit Data/BigCraftables to support custom machines.
    /// Uses CustomBigCraftableManager as source data
    /// </summary>
    internal class BigCraftablesDataProvider : IGameDataProvider
    {
        ICustomBigCraftableService customBigCraftableService;
        public BigCraftablesDataProvider(ICustomBigCraftableService customBigCraftableService)
        {
            this.customBigCraftableService = customBigCraftableService;
        }     
        public override string Name => "Data/BigCraftables";

        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                IAssetDataForDictionary<string, BigCraftableData> bigCraftables = asset.AsDictionary<string, BigCraftableData>();
                //
                //  bigcraftables are always added, their crafting recipe
                //  is gated
                //
                foreach (CustomBigCraftableData bigCraftable in customBigCraftableService.BigCraftables.Values)
                {
                    bigCraftables.Data.Add(bigCraftable.Id, bigCraftable.BigCraftableData);
                }
            });
        }

        public override void OnGameLaunched()
        {

        }
    }
}
