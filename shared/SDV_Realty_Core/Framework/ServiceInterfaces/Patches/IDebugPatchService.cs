using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.Patches
{
    internal abstract class IDebugPatchService:IService
{
        public override Type ServiceType => typeof(IDebugPatchService);
    }
}
