using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class ModAPIService : IModAPIService
    {
        private IContentManagerService contentManagerService;
        private ICustomEntitiesServices customEntitiesServices;
        private IModDataService modDataService;
        private ICustomEventsService eventsService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IContentManagerService), typeof(ICustomEntitiesServices),
            typeof(IModDataService),typeof(ICustomEventsService)
        };

        public override ModApi GetAPI()
        {
            return new ModApi(logger, contentManagerService.contentManager, customEntitiesServices, modDataService, eventsService);
        }

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            contentManagerService = (IContentManagerService)args[0];
            customEntitiesServices = (ICustomEntitiesServices)args[1];
            modDataService = (IModDataService)args[2];
            eventsService = (ICustomEventsService)args[3];

            modAPI = new ModApi(logger, contentManagerService.contentManager, customEntitiesServices, modDataService, eventsService);
        }
    }
}
