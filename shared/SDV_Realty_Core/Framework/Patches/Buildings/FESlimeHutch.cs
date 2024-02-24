using StardewValley.Buildings;
using SDV_Realty_Core.Framework.Utilities;

namespace SDV_Realty_Core.Framework.Patches.Buildings
{
    internal class FESlimeHutch
    {
        public static bool getBuilding(SlimeHutch __instance, ref Building __result)
        {
            if (__instance.modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                Farm fm = Game1.getLocationFromName(__instance.modData[FEModDataKeys.FELocationName]) as Farm;
                foreach (Building building in fm.buildings)
                {
                    if (building.indoors.Value != null && building.indoors.Value.Equals(__instance))
                    {
                        __result = building;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
