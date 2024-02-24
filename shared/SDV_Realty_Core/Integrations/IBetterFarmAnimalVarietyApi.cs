using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace SDV_Realty_Core.Framework.Integrations
{
    public interface IBetterFarmAnimalVarietyApi
    {/// <param name="farm">StardewValley.Farm</param>
     /// <returns>Returns List<StardewValley.Object></returns>
        List<StardewValley.Object> GetAnimalShopStock(Farm farm);

        /// <summary>Determine if the mod is enabled.</summary>
        Dictionary<string, Texture2D> GetAnimalShopIcons();

        /// <param name="category">string</param>
        /// <param name="farmer">StardewValley.Farmer</param>
        /// <returns>Returns string</returns>
        string GetRandomAnimalShopType(string category, Farmer farmer);
    }
}
