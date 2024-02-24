using StardewModdingAPI.Events;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class ObjectInformation : IGameDataProvider
    {
        public override string Name => "Data/ObjectInformation";

        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {

            });
        }

        public override void OnGameLaunched()
        {
        }
    }
}
