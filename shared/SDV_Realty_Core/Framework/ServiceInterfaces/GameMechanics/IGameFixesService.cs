using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics
{
    internal abstract class IGameFixesService:IService
{
        public override Type ServiceType => typeof(IGameFixesService);
    }
}
