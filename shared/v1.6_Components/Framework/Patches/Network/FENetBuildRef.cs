using System.Collections.Generic;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Network;
using System.Linq;
using SDV_Realty_Core.Framework.Objects;
using StardewModdingAPI;
using Prism99_Core.Utilities;

namespace SDV_Realty_Core.Framework.Patches.Network
{

    public static class FENetBuildingRef
    {
        private static SDVLogger logger;


        internal static void Initialize(SDVLogger olog)
        {
            logger = olog;
        }
        public static void Value(NetBuildingRef __instance, NetString ___nameOfIndoors, ref Building __result)
        {
            if (__result != null ||string.IsNullOrEmpty( ___nameOfIndoors.Value))
                return;

            if (!string.IsNullOrEmpty(___nameOfIndoors.Value))
            {
                foreach (GameLocation fm in FEFramework.farmExpansions.Values)
                {
                    foreach (Building oBuilding in fm.buildings)
                    {
                        if (oBuilding.GetIndoorsName() == ___nameOfIndoors.Value)
                        {
                            //
                            //  very chatty call
                            //
                            //logger.Log($"Found Building {___nameOfIndoors.Value} in {fm.Name}", LogLevel.Debug);

                            __result = oBuilding;
                            return;
                        }
                    }
                }
            }

            foreach (GameLocation gl in Game1.locations)
            {
                //if (gl is Farm f)
                //{
                    var oCheck = gl.buildings.Where(p => p.GetIndoorsName() == ___nameOfIndoors.Value);
                    if (oCheck.Count() == 1)
                    {
                        logger.Log($"Found building '{___nameOfIndoors?.Value??""}' in base Farm", LogLevel.Debug);
                        __result = oCheck.First();
                        return;
                    }
                //}
            }
            if (!string.IsNullOrEmpty(___nameOfIndoors.Value))
            {
                logger.LogOnce($"FENetBuildingRef Did not find Building '{___nameOfIndoors.Value}'", LogLevel.Trace);
                //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                //logger.Log($"StackTrace: {t}", LogLevel.Debug);
            }
        }
    }
}
