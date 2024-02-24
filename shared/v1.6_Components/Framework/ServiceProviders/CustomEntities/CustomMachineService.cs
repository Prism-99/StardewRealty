using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using SDV_Realty_Core.Framework.CustomEntities.Machines;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.CustomEntities
{
    internal class CustomMachineService : ICustomMachineService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService),typeof(ICustomBigCraftableService),
            typeof(ICustomMachineDataService)
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

            IModHelperService modHelper = (IModHelperService)args[0];
            ICustomBigCraftableService customBigCraftableService = (ICustomBigCraftableService)args[1];
            ICustomMachineDataService customMachineDataService= (ICustomMachineDataService)args[2];

            customMachineManager = new CustomMachineManager(logger, modHelper, customBigCraftableService, customMachineDataService);
        }

        public override void LoadDefinitions()
        {
            customMachineManager.LoadDefinitions();
        }
    }
}
