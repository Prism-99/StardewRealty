using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class ILandManager:IService
{
        public override Type ServiceType => typeof(ILandManager);
        public List<string> LandForSale = new List<string>();
        internal abstract bool PurchaseLand(string expansionName, bool withPopup, long purchasedBy);
        internal abstract void LandBought(string sExpansionName, bool withPopup, long purchasedBy);
        internal abstract void CheckIfCanBePutForSale(string sExpansionName);

    }
}
