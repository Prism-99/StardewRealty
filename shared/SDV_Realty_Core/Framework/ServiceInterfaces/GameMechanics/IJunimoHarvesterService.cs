using SDV_Realty_Core.Framework.Patches.Characters;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics
{
    internal abstract class IJunimoHarvesterService:IService
{
        public FEJuminoHarvester JunimoHarvester;
        public override Type ServiceType => typeof(IJunimoHarvesterService);
    }
}
