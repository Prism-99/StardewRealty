using SDV_Realty_Core.Framework.ModMechanics.FarmServiceProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;



namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class IFarmServicesService : IService
    {
       
        protected Dictionary<string, IFarmServiceProvider> services = new();
        public override Type ServiceType => typeof(IFarmServicesService);
        public abstract void AddService(IFarmServiceProvider service);
    }
}
