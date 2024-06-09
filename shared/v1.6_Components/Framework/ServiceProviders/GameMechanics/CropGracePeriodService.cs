using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.GameMechanics;

namespace SDV_Realty_Core.Framework.ServiceProviders.GameMechanics
{
    internal class CropGracePeriodService : ICropGracePeriodService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IPatchingService),typeof(IUtilitiesService)
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

            IPatchingService patchingService = (IPatchingService)args[0];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[1];

            CropGracePeriod cropGracePeriod = new CropGracePeriod(logger,patchingService, utilitiesService);
        }
    }
}
