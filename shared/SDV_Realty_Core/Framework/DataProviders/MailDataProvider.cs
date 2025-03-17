using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class MailDataProvider : IGameDataProvider
    {
        private IModDataService ModDataService;
        public override string Name => "Data/mail";

        public MailDataProvider( IModDataService modDataService)
        {
            ModDataService = modDataService;
        }
        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            logger.Log("Editing Data/Mail",LogLevel.Debug);
            e.Edit(asset =>
            {
                foreach (ExpansionPack contentPack in ModDataService.validContents.Values)
                {
                    if (!string.IsNullOrEmpty(contentPack.MailId) && !string.IsNullOrEmpty(contentPack.MailContent))
                    {
                        logger.Log("Adding mail id:" + contentPack.MailId, LogLevel.Debug);
                        asset.AsDictionary<string, string>().Data.Add(contentPack.MailId, contentPack.MailContent);
                    };

                    if (!string.IsNullOrEmpty(contentPack.VendorMailId) && !string.IsNullOrEmpty(contentPack.VendorMailContent))
                    {
                        logger.Log("Adding vendor mail id:" + contentPack.VendorMailId, LogLevel.Debug);
                        asset.AsDictionary<string, string>().Data.Add(contentPack.VendorMailId, contentPack.VendorMailContent);
                    }
                }
                foreach (var customMail in ModDataService.CustomMail)
                {
                    asset.AsDictionary<string, string>().Data.Add(customMail.Key, customMail.Value);
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
