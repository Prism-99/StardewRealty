using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;
using StardewValley.TerrainFeatures;
using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.Objects;

namespace Framework.Patches.TerrainFeature
{
    internal static class FEFruitTree
    {
        private static FEConfig config;
        public static void Initialize(FEConfig oconfig)
        {
            config = oconfig;
        }
        public static bool IsInSeasonHere(FruitTree __instance, GameLocation location)
        {
            if (!config.UseGracePeriod || !config.GraceForFruit|| !location.modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                //
                //  not enable or not a Farm expansion, fall to base class
                //
                return true;
            }

            return SeasonalUtils.IsCropInGracePeriod(location,new List<string> { __instance.fruitSeason.Value });
        }
    }
}
