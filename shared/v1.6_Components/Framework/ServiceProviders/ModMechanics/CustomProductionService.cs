using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.CustomEntities;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class CustomProductionService : ICustomProductionService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IPatchingService),typeof(ICustomEntitiesServices)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger=logger;
            IPatchingService patchingService = (IPatchingService)args[0];
            ICustomEntitiesServices customEntitiesServices = (ICustomEntitiesServices)args[1];

            customProductionManager = new CustomProductionManager();
            customProductionManager.Initialize((SDVLogger)logger.CustomLogger, patchingService.patches, customEntitiesServices);
        }
    }
}
