using SDV_Realty_Core.Framework.Objects;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Utilities
{
    internal abstract class IModAPIService:IService
{
        public override Type ServiceType => typeof(IModAPIService);
        public ModApi modAPI;
        public abstract ModApi GetAPI();
    }
}
