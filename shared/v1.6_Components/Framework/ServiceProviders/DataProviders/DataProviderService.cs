using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;


namespace SDV_Realty_Core.Framework.ServiceProviders.DataProviders
{
    internal class DataProviderService : IDataProviderService
    {
        public override Type ServiceType => typeof(IDataProviderService);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IContentPackService),typeof(IContentManagerService),
            typeof(IConfigService),typeof(IUtilitiesService),
            typeof(IGameDataProviderService)
        };
        public override Type[] Dependants => new Type[] {  };
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(IDataProviderService))
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            IContentPackService contentPackService = (IContentPackService)args[0];
            IContentManagerService contentManager = (IContentManagerService)args[1];
            IConfigService configService = (IConfigService)args[2];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[3];
            IGameDataProviderService gameDataProviderService = (IGameDataProviderService)args[4];

            dataProviderManager = new DataProviderManager();
            dataProviderManager.Initialize(contentPackService.contentPackLoader, contentManager.contentManager, utilitiesService, logger, gameDataProviderService);

            utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), dataProviderManager.CheckForActivations);
            utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), dataProviderManager.CheckForActivations);
            utilitiesService.CustomEventsService.AddModEventSubscription(ICustomEventsService.ModEvents.ConfigChanged, dataProviderManager.ConfigurationChanged);
        }
    }
}
