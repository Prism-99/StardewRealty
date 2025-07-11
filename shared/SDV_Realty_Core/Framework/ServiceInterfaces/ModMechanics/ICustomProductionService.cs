using System;
using SDV_Realty_Core.Framework.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class ICustomProductionService:IService
{
        public override Type ServiceType => typeof(ICustomProductionService);
        public CustomProductionManager customProductionManager;
    }
}
