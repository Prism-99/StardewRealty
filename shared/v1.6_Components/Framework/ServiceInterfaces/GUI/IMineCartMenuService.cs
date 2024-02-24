using System;
using SDV_Realty_Core.Framework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GUI
{
    internal abstract class IMineCartMenuService:IService
{
        public override Type ServiceType => typeof(IMineCartMenuService);
        public FEMineCartMenu MineCartMenu;
    }
}
