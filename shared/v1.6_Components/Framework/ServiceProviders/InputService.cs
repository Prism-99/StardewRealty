using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using System;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;

namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class InputService : IInputService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IForSaleMenuService),
            typeof(IMineCartMenuService),typeof(ILandManager)
            
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
            IForSaleMenuService forSaleMenuService = (IForSaleMenuService)args[1];
            IMineCartMenuService mineCartMenuService= (IMineCartMenuService)args[2];
            ILandManager landManager = (ILandManager)args[3];

            inputHandler = new FEInputHandler(logger, utilitiesService, forSaleMenuService, mineCartMenuService, landManager);
         }
    }
}
