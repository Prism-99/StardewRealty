using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModData
{
    /// <summary>
    /// Provides strings for maps
    /// </summary>
    internal abstract class IStringService:IService
{
        public override Type ServiceType => typeof(IStringService);
    }
}
