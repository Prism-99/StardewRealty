using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceProviders.ModMechanics;
using SDV_Realty_Core.Framework.Utilities;
using StardewValley.BellsAndWhistles;
using System;
using System.Diagnostics;
using xTile.ObjectModel;

namespace SDV_Realty_Core.Framework.Patches
{

    internal  class GameLocationPatches
    {
        private static ILoggerService logger;
        // call this method from your Entry class
        public GameLocationPatches(ILoggerService monitor)
        {
            logger = monitor;
        }

        public static bool DayUpdate(GameLocation __instance,ref int dayOfMonth, ref Stopwatch __state)
        {
            //
            //  passively log DayUpdate calls
            //
            __state = new Stopwatch();
            __state.Start();
            logger.Log($"DayUpdate, location { __instance.Name},day {dayOfMonth}", LogLevel.Debug);
            foreach(var obj in __instance.Objects.Values)
            {
                if (obj.name == null)
                {
                    obj.name = "Error Item";
                }
            }
            return true;
        }
       
        public static void DayUpdate_Post(GameLocation __instance, ref int dayOfMonth, Stopwatch __state)
        {
            //
            //  passively log DayUpdate calls
            //
         
            __state.Stop();
            logger.Log($"DayUpdate, location {__instance.Name},day {dayOfMonth} took {__state.ElapsedMilliseconds}ms", LogLevel.Debug);

           
        }
        public static bool seasonUpdate(GameLocation __instance, ref string season, ref bool onLoad)
        {
            if (season == null) { season = Game1.currentSeason; }
            //logger.Log($"seasonUpate, loc name {__instance.Name}, season {season},onload {onLoad}", LogLevel.Debug);
            return true;
        }
        public static bool updateWarps(GameLocation __instance)
        {
            if (!__instance.modData.ContainsKey(IModDataKeysService.FELocationName))
            {
                return true;
            }
            __instance.warps.Clear();

            __instance.map.Properties.TryGetValue("NPCWarp", out PropertyValue value);

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
                    string locname = array2[j + 2] == "Farm" || array2[j + 2] == "Farmhouse" ? __instance.modData[IModDataKeysService.FELocationName] : array2[j + 2];
                    __instance.warps.Add(new Warp(Convert.ToInt32(array2[j]), Convert.ToInt32(array2[j + 1]), locname, Convert.ToInt32(array2[j + 3]), Convert.ToInt32(array2[j + 4]), flipFarmer: false));
                }
            }

            return false;
        }
    }
}
