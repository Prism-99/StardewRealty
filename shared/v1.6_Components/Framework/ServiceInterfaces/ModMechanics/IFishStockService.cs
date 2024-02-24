using StardewRealty.SDV_Realty_Interface;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics
{
    internal abstract class IFishStockService:IService
{
        public override Type ServiceType => typeof(IFishStockService);
        internal abstract void ResetFishSeasonStock(string season = null);
        internal abstract void SetSeasonalStock(string season, FishAreaDetails area, int maxPrice, List<string> newFish, Dictionary<string, int> prices);
        internal abstract void SetFishAreaStocks();


    }
}
