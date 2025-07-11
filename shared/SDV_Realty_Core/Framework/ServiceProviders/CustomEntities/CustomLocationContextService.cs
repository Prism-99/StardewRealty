using SDV_Realty_Core.Framework.CustomEntities.LocationContexts;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.CustomEntities
{
    internal class CustomLocationContextService : ICustomLocationContextService
    {
        public override Dictionary<string, CustomLocationContext> locationContexts => locationContextDataManager.LocationContexts;

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
            IUtilitiesService utilitiesService= (IUtilitiesService)args[0];

            locationContextDataManager = new LocationContextDataManager(logger,  utilitiesService);
        }
        public override void LoadDefinitions()
        {
            locationContextDataManager.LoadDefinitions();
        }
    }
}
