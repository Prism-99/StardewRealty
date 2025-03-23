using SDV_Realty_Core.Framework.CustomEntities.Machines;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.CustomEntities
{
    internal class CustomMachineDataService : ICustomMachineDataService
    {
        public override Dictionary<string, CustomMachineData> Machines => machineDataManager.Machines;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService),typeof(IContentManagerService)
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
            IModHelperService modHelperService= (IModHelperService)args[0];
            IContentManagerService contentManagerService= (IContentManagerService)args[1];

            machineDataManager = new CustomMachineDataManager(logger, modHelperService, contentManagerService);
        }
        public override void LoadDefinitions()
        {
            machineDataManager.LoadDefinitions();
        }

        public override void AddMachine(CustomMachineData machine)
        {
            machineDataManager.AddMachine(machine);
        }
    }
}
