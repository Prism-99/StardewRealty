using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics
{
    internal abstract class IAutoGrabberService:IService
{
        public override Type ServiceType => typeof(IAutoGrabberService);
    }
}
