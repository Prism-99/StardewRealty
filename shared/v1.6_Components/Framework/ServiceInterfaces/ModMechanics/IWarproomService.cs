using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.Locations;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class IWarproomService:IService
    {
        public override Type ServiceType => typeof(IWarproomService);

        public WarproomManager warproomManager;
    }
}
