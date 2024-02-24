using StardewValley.TerrainFeatures;
using System.Linq;

namespace SDV_Realty_Core.Framework.Utilities
{
    internal static class GameLocation_Ext
    {
        public static void WaterMe(this GameLocation location)
        {
            var hoeDirt = location.terrainFeatures.Pairs.Where(p => p.Value is HoeDirt).Select(p => p.Value as HoeDirt);

            foreach (var dirt in hoeDirt)
            {
                if (dirt.state.Value != 2)
                {
                    dirt.state.Value = 1;
                }
            }
        }
    }
}
