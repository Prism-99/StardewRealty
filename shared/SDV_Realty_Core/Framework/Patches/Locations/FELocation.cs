using System;
using xTile.ObjectModel;
using SDV_Realty_Core.Framework.Utilities;

#if !v16
using StardewValley;
using SDObject = StardewValley.Object;
using Microsoft.Xna.Framework;
#endif

namespace SDV_Realty_Core.Framework.Patches.Locations
{
    public class FELocation
    {
        public static bool getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, GameLocation __instance,SDObject __result)
        {
#if v16
            __result = new SDObject("186", 1);
#else
            __result= new SDObject(186, 1);
#endif
            return false;
        }
        public static bool updateWarps(GameLocation __instance)
        {
            if (!__instance.modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                return true;
            }
            __instance.warps.Clear();
#if v16
            __instance.map.Properties.TryGetValue("NPCWarp", out PropertyValue value);
#else
            __instance.map.Properties.TryGetValue("NPCWarp", out PropertyValue value);
#endif
            if (value != null)
            {
                string[] array = value.ToString().Split(' ');
                for (int i = 0; i < array.Length; i += 5)
                {
                    __instance.warps.Add(new Warp(Convert.ToInt32(array[i]), Convert.ToInt32(array[i + 1]), array[i + 2], Convert.ToInt32(array[i + 3]), Convert.ToInt32(array[i + 4]), flipFarmer: false, npcOnly: true));
                }
            }

            value = null;
            __instance.map.Properties.TryGetValue("Warp", out value);
            if (value != null)
            {
                string[] array2 = value.ToString().Split(' ');
                for (int j = 0; j < array2.Length; j += 5)
                {
                    string locname = array2[j + 2] == "Farm" || array2[j + 2] == "Farmhouse" ? __instance.modData[FEModDataKeys.FELocationName] : array2[j + 2];
                    __instance.warps.Add(new Warp(Convert.ToInt32(array2[j]), Convert.ToInt32(array2[j + 1]), locname, Convert.ToInt32(array2[j + 3]), Convert.ToInt32(array2[j + 4]) + 1, flipFarmer: false));
                }
            }

            return false;
        }
    }
}
