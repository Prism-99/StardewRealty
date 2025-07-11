using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    /// <summary>
    /// Manages adding Treasure tiles to expansions
    /// </summary>
    internal abstract class ITreasureManager:IService
{
        public override Type ServiceType => typeof(ITreasureManager);
    }
}
