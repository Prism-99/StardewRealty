using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GUI
{
    internal abstract class IWordMapMenuService : IService
    {
        public override Type ServiceType => typeof(IWordMapMenuService);
    }
}
