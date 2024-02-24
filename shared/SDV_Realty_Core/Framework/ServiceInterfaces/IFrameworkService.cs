using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;


namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IFrameworkService:IService
    {
        public override Type ServiceType => typeof(IFrameworkService);

        //public FEFramework framework;

    }
}
