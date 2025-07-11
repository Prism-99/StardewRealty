using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;
using SDV_Realty_Core.Framework.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.Framework.ServiceProviders.Integrations
{
    internal class GMCMIntergrationService : IGMCMIntergrationService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IModDataService)
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

            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            IModDataService modDataService = (IModDataService)args[1];

            CGCMIntegration = new GMCM_Integration
                (modDataService,utilitiesService.ModHelperService,utilitiesService.ManifestService.manifest,utilitiesService.ConfigService,logger);

            utilitiesService.GameEventsService.AddSubscription(new GameLaunchedEventArgs() , RegisterMenu);
        }
        private void RegisterMenu(EventArgs e)
        {
            CGCMIntegration.RegisterMenu();
        }
    }
}
