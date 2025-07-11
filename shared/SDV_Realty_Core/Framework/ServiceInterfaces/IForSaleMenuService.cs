using System;
using SDV_Realty_Core.Framework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces
{
    internal abstract class IForSaleMenuService:IService
{
        public FEForSaleMenu forSaleMenu;
        public abstract FEForSaleMenu GetMenu();
        public override Type ServiceType => typeof(IForSaleMenuService);
    }
}
