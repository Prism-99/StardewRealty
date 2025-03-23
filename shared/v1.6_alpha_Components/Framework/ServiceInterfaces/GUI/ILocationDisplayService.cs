using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;


namespace SDV_Realty_Core.Framework.ServiceInterfaces.GUI
{
    internal abstract class ILocationDisplayService : IService
    {
        public override Type ServiceType => typeof(ILocationDisplayService);
    }
}
