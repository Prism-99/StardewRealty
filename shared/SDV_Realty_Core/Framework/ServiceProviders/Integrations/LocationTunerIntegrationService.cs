using SDV_Realty_Core.Framework.ServiceInterfaces.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Integrations;
using StardewModdingAPI.Events;
using System;

namespace SDV_Realty_Core.Framework.ServiceProviders.Integrations
{
    internal class LocationTunerIntegrationService : ILocationTunerIntegrationService
    {
        private IUtilitiesService utilitiesService;
        private ILocationTunerAPI locationTunerAPI;
        private bool modLoaded = false;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;

            utilitiesService = (IUtilitiesService)args[0];
            utilitiesService.GameEventsService.AddSubscription(new GameLaunchedEventArgs(), RegisterMenu);
        }
        private void RegisterMenu(EventArgs e)
        {
            AddAPI();
        }
        private void AddAPI()
        {
            if (utilitiesService.ModHelperService.ModRegistry.IsLoaded("prism99.locTuner"))
            {
                logger.Log("LocationTuner is loaded", LogLevel.Debug);
                locationTunerAPI = utilitiesService.ModHelperService.ModRegistry.GetApi<ILocationTunerAPI>("prism99.locTuner");
                modLoaded = true;
                logger.Log("LocationTuner API loaded", LogLevel.Debug);
            }
        }

        public override bool NoCrows(string locationName, out bool value)
        {
            value = false;
            if (modLoaded)
                return locationTunerAPI.NoCrows(locationName, out value);
            else
                return false;
        }
        public override bool NoLightning(string locationName, out bool value)
        {
            value = false;
            if (modLoaded)
                return locationTunerAPI.NoLightning(locationName, out value);
            else
                return false;
        }
    }
}
