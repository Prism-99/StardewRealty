using SDV_Realty_Core.Framework.CustomEntities;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class ICustomLigthingService:IService
{
        public override Type ServiceType => typeof(ICustomLigthingService);
        public CustomLightingManager customLightingManager;
    }
}
