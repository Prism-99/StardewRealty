using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;


namespace SDV_Realty_Core.Framework.ServiceProviders.DataProviders
{
    internal class DataProviderService : IDataProviderService
    {
        public override Type ServiceType => typeof(IDataProviderService);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IContentPackService),typeof(IContentManagerService),
            typeof(IConfigService),typeof(IModHelperService),
            typeof(IGameDataProviderService),typeof(IGameEventsService)
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
            IModHelperService modHelperService = (IModHelperService)args[3];
            IGameDataProviderService gameDataProviderService = (IGameDataProviderService)args[4];
            IGameEventsService eventsService = (IGameEventsService)args[5];

            dataProviderManager = new DataProviderManager();
            dataProviderManager.Initialize(contentPackService.contentPackLoader, contentManager.contentManager, configService.config, modHelperService.modHelper, logger, gameDataProviderService);

            eventsService.AddSubscription(new SaveLoadedEventArgs(), dataProviderManager.CheckForActivations);
            eventsService.AddSubscription(new DayStartedEventArgs(), dataProviderManager.CheckForActivations);
        }
    }
}
