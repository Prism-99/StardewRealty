using System;
using System.Linq;
using StardewValley.Buildings;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Characters;
using StardewValley;
using Netcode;
using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.Objects;
using StardewValley.Locations;
using System.Collections.Generic;
using Prism99_Core.Utilities;

namespace SDV_Realty_Core.Framework.Patches.Characters
{

    internal static class FEJunimoHarvesterHomeGetter
    {

        private static SDVLogger logger;

        public static void Initialize(SDVLogger olog)
        {
            logger = olog;
        }

        public static bool GetHome(JunimoHarvester __instance, ref JunimoHut __result)
        {
#if TRACE_LOG
            logger.Log("home.getter called.", LogLevel.Info);
            logger.Log("Check for hut", LogLevel.Info);
            logger.Log($"gethome type={__instance.GetType().Name}", LogLevel.Info);
#endif
            //if (__instance.GetType() == typeof(CustomJunimoHarvester)) return true;
            //
            //  check for framework ModData
            //
            NetGuid hutGuid = Traverse.Create(__instance).Field("netHome").GetValue() as NetGuid;
            if (__instance.modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                //
                //  found expansion info, get Guid from there
#if TRACE_LOG
                FEFramework.monitor.Log($"Home.getter has exp mod data, loc='{__instance.modData[FEModDataKeys.FELocationName]}', hutGuid='{hutGuid.Value}'", LogLevel.Info);
#endif
                //  __instance.modData[FEModDataKeys.FELocationName]].buildings[hutGuid.Value] as JunimoHut;
                Farm oTarget = Game1.getLocationFromName(__instance.modData[FEModDataKeys.FELocationName]) as Farm;
#if TRACE_LOG
                logger.Log($"Have Farm '{oTarget.name.Value}'", LogLevel.Info);
                logger.Log($"Have building {oTarget.buildings.ContainsGuid(hutGuid.Value)}", LogLevel.Info);
                Building oBuilding = oTarget.buildings[hutGuid.Value];
                logger.Log($"Building is JunimoHut {oBuilding is JunimoHut}", LogLevel.Info);
#endif
                try
                {
                    __result = oTarget.buildings[hutGuid.Value] as JunimoHut;
                }
                catch ( KeyNotFoundException) { }
                catch (Exception ex)
                {
                    logger.Log($"Error: {ex}", LogLevel.Error);
                }

#if TRACE_LOG
                logger.Log($"Set Value, exiting", LogLevel.Info);
#endif
                return false;
            }
            else
            {
                //
                //  did not find mod location data, do deep search
                //
                //
                //  search expansions for guid
                //
#if v16
                var oList = FEFramework.farmExpansions.Where(p => p.Value.IsBuildableLocation() && p.Value.buildings.ContainsGuid(hutGuid.Value));
#else
                var oList = FEFramework.farmExpansions.Where(p => p.Value is BuildableGameLocation && ((BuildableGameLocation)p.Value).buildings.ContainsGuid(hutGuid));
#endif

                //
                //  return the JuminoHarvester's JuminoHut
                //
                if (oList.Count() == 1)
                {
#if v16
                    __result = (oList.First().Value).buildings[hutGuid.Value] as JunimoHut;
#else
                    __result = ((BuildableGameLocation)oList.First().Value).buildings[hutGuid] as JunimoHut;
#endif
#if TRACE_LOG
                    logger.Log("Found Hut", LogLevel.Info);
#endif
                    //
                    //  handled by the framework, do not conitnue to the base call
                    //
                    return false;
                }
            }
            //else
            //{
            //    framework.monitor.Log("Junimo.GetHome did not find Hut", LogLevel.Info);
            //}
            //}
            //}
            //
            //  not handled by the framework, continue to the base call
            return true;
        }

    }
}
