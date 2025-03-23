using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;

namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IForSaleSignService:IService
{
        public FEForSale forsaleManager;

        public override Type ServiceType => typeof(IForSaleSignService);
    }
}
