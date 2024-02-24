using System;
using StardewValley.Locations;
using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.Objects;
using System.Reflection;

namespace SDV_Realty_Core.Framework.Patches.Locations
{
    internal static class FECellar
    {
      
        public static bool resetLocalState(Cellar __instance)
        {
 
            if (!__instance.modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                return true;
            }
            string target = __instance.modData[FEModDataKeys.FELocationName];

            Type myType = (typeof(GameLocation));
            var ptr = myType.GetMethod("resetLocalState", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).MethodHandle.GetFunctionPointer();
            //var ptr = typeof(GameLocation).GetMethod("resetLocalState").MethodHandle.GetFunctionPointer();
            var grandparentDayUpdate = (Action)Activator.CreateInstance(typeof(Action), __instance, ptr);
            grandparentDayUpdate();


            FEFramework.FixWarps(__instance,target);

            return false;
         
        }
        public static bool updateWarps(Cellar __instance)
        {
            if (!__instance.modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                return true;
            }
            __instance.updateWarps();
            FEFramework.FixWarps(__instance,__instance.Name);
            //__instance.warps.Clear();
            //__instance.map.Properties.TryGetValue("NPCWarp", out PropertyValue value);
            //if (value != null)
            //{
            //    string[] array = value.ToString().Split(' ');
            //    for (int i = 0; i < array.Length; i += 5)
            //    {
            //        __instance.warps.Add(new Warp(Convert.ToInt32(array[i]), Convert.ToInt32(array[i + 1]), array[i + 2], Convert.ToInt32(array[i + 3]), Convert.ToInt32(array[i + 4]), flipFarmer: false, npcOnly: true));
            //    }
            //}

            //value = null;
            //__instance.map.Properties.TryGetValue("Warp", out value);
            //if (value != null)
            //{
            //    string[] array2 = value.ToString().Split(' ');
            //    for (int j = 0; j < array2.Length; j += 5)
            //    {
            //        string locname = array2[j + 2] == "Farm" || array2[j + 2] == "Farmhouse" ? __instance.modData[FEModDataKeys.FELocationName] : array2[j + 2];
            //        __instance.warps.Add(new Warp(Convert.ToInt32(array2[j]), Convert.ToInt32(array2[j + 1]), locname, Convert.ToInt32(array2[j + 3]), Convert.ToInt32(array2[j + 4])+1, flipFarmer: false));
            //    }
            //}

            return false;
        }
    }
}
