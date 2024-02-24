using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Characters;
using StardewValley.Buildings;
using StardewModdingAPI;
using HarmonyLib;
using Netcode;
using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.Objects;
using StardewValley.Locations;

namespace SDV_Realty_Core.Framework.Patches.Characters
{
 
    internal static class FEJunimoHarvesterHomeSetter
    {
        private static FEFramework framework;

        public static void Initialize(FEFramework oFramework)
        {
            framework = oFramework;
        }
 
        public static bool SetHome(JunimoHarvester __instance, JunimoHut value)
        {
#if TRACE_LOG
            FEFramework.monitor.Log("home.setter called.", LogLevel.Info);
#endif

            if (value.modData.ContainsKey(FEModDataKeys.FELocationName))
            {
#if TRACE_LOG
                FEFramework.monitor.Log($"JunimoHut as value: {value.modData.ContainsKey(FEModDataKeys.FELocationName)}");
#endif
#if v16
                Guid gBuilding = (FEFramework.farmExpansions[value.modData[FEModDataKeys.FELocationName]]).buildings.GuidOf(value);
#else
                Guid gBuilding =(FEFramework.farmExpansions[value.modData[FEModDataKeys.FELocationName]]).buildings.GuidOf(value);
#endif
                //framework.monitor.Log($"Guid: {gBuilding}", LogLevel.Info);
                //
                //  get new NetGuid to update the JuminoHarvester home
                //
                NetGuid nGuid = new NetGuid(gBuilding);
                Traverse.Create(__instance).Field("netHome").SetValue(nGuid);
                //
                //  set JuminoHarvester location name in ModData
                //
                __instance.modData.Add(FEModDataKeys.FELocationName, value.modData[FEModDataKeys.FELocationName]);
                //
                //  handled by framework, do not continue to the base call
                //
                return false;

            }
            //
            //  not handled by the framework, continue to the base call
            return true;
        }
    }
}
