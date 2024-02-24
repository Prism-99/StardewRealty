using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class IModDataKeysService:IService
{
        public override Type ServiceType => typeof(IModDataKeysService);
    }
}
