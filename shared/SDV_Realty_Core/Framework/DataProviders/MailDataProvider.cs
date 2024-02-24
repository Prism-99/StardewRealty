using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using StardewModdingAPI.Events;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class MailDataProvider : IGameDataProvider
    {
        private ContentPackLoader ContentPacks;
        public override string Name => "Data/mail";

        public MailDataProvider(ContentPackLoader contentPacks)
        {
            ContentPacks = contentPacks;
        }
        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                foreach (ExpansionPack contentPack in ContentPacks.ValidContents.Values)
                {
                    if (!string.IsNullOrEmpty(contentPack.MailId) && !string.IsNullOrEmpty(contentPack.MailContent))
                    {
                        logger?.Log("Adding mail id:" + contentPack.MailId, LogLevel.Debug);
                        asset.AsDictionary<string, string>().Data.Add(contentPack.MailId, contentPack.MailContent);
                    };

                    if (!string.IsNullOrEmpty(contentPack.VendorMailId) && !string.IsNullOrEmpty(contentPack.VendorMailContent))
                    {
                        logger?.Log("Adding vendor mail id:" + contentPack.VendorMailId, LogLevel.Debug);
                        asset.AsDictionary<string, string>().Data.Add(contentPack.VendorMailId, contentPack.VendorMailContent);
                    }
                }
            }
                   );
        }

        public override void CheckForActivations()
        {
            
        }

        public override void OnGameLaunched()
        {
            
        }
    }
}
