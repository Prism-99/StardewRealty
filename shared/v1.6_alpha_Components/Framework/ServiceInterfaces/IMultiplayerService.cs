using System;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IMultiplayerService:IService
{
        public SDRMultiplayer Multiplayer;
        public override Type ServiceType => typeof(IMultiplayerService);
    }
}
