using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class IExpansionBridgingService : IService
    {
        public override Type ServiceType => typeof(IExpansionBridgingService);
       
    }
}
