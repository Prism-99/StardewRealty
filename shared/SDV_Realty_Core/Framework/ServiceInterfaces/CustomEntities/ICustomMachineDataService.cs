using SDV_Realty_Core.Framework.CustomEntities.Machines;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities
{
    internal abstract class ICustomMachineDataService:IService
{
        public override Type ServiceType => typeof(ICustomMachineDataService);
        public abstract Dictionary<string, CustomMachineData> Machines { get; }
        public CustomMachineDataManager machineDataManager;
        public abstract void LoadDefinitions();
        public abstract void AddMachine(CustomMachineData machine);


    }
}
