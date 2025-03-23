using SDV_Realty_Core.Framework.CustomEntities.Machines;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities
{
    internal abstract class ICustomMachineService:IService
{
        public override Type ServiceType => typeof(ICustomMachineService);
        public CustomMachineManager customMachineManager;
        public abstract void LoadDefinitions();
    }
}
