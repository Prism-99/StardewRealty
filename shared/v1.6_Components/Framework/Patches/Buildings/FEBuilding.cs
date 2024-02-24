using Prism99_Core.Utilities;
using StardewValley.Buildings;


namespace SDV_Realty_Core.Framework.Patches.Buildings
{
    internal class FEBuilding
{
        private static SDVLogger logger;
        //
        //  ver 1.6
        //
        public static void Initialize(SDVLogger olog)
        {
            logger = olog;
        }
        internal static void load(Building __instance)
        {
            logger.Log($"load called {__instance.buildingType}", LogLevel.Debug);
        }
       
    }
}
