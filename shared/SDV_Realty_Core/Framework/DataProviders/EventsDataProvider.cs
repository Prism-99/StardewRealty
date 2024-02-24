using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class EventsDataProvider : IGameDataProvider
    {
        public override string Name => "Data/Events";
        private IModDataService _modDataService;
        public EventsDataProvider(IModDataService modDataService)
        {
            _modDataService = modDataService;
        }

        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            string[] arParts = e.NameWithoutLocale.Name.Split('/');
            if (_modDataService.farmExpansions.ContainsKey(arParts[arParts.Length - 1]))
            {
                e.Edit(asset =>
                {

                });
            }
        }
        public override bool Handles(string assetName)
        {
            return assetName.StartsWith("Data/Events/");
        }
        public override void OnGameLaunched()
        {

        }
    }
}
