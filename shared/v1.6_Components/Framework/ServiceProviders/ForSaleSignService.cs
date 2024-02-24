using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;
using SDV_Realty_Core.Framework.Objects;


namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class ForSaleSignService : IForSaleSignService
    {
         private IUtilitiesService utilitiesService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IModDataService),
            typeof(ILandManager)
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
            utilitiesService= (IUtilitiesService)args[0];
            IModDataService modDataService = (IModDataService)args[1];
            ILandManager landManager= (ILandManager)args[2];

            forsaleManager = new FEForSale();
            forsaleManager.Initialize(logger, utilitiesService, modDataService, landManager);
        }
    }
}
