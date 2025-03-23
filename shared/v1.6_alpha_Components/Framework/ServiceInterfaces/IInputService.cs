using SDV_Realty_Core.Framework.Objects;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IInputService:IService
{
        public FEInputHandler inputHandler;
        public override Type ServiceType => typeof(IInputService);
    }
}
