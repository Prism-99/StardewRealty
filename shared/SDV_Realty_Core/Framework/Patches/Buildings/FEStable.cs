using StardewValley.Characters;
using StardewValley.Buildings;
using SDV_Realty_Core.Framework.Utilities;
using Prism99_Core.Utilities;


namespace SDV_Realty_Core.Framework.Patches.Buildings
{
    internal static class FEStable
    {
        //public static bool dayUpdate(Stable __instance, int __dayOfMonth)
        //{
        //   __instance.

        //    return false;
        //}
        private static SDVLogger logger;
        public static void Initialize(SDVLogger olog)
        {
            logger = olog;
        }
        public static void grabHorsePost(Stable __instance)
        {
            logger.Log($" grabHorsePost called", LogLevel.Debug);
        }
        public static bool grabHorse(Stable __instance)
        {
            GameLocation glStable=null;
            logger.Log($"grabHorse called for {__instance.buildingType.Value}", LogLevel.Debug);
            if (__instance.modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                glStable = Game1.getLocationFromName(__instance.modData[FEModDataKeys.FELocationName]);
            }
            if(glStable == null) 
            {
                glStable = Game1.getLocationFromName("Farm");
            }
            logger.Log($"   found Stable in {glStable.Name}", LogLevel.Debug);

            if (__instance.daysOfConstructionLeft.Value <= 0)
            {
                Horse horse = Utility.findHorse(__instance.HorseId);
                if (horse == null)
                {
                    logger.Log($"   adding new horse.", LogLevel.Debug);
                    horse = new Horse(__instance.HorseId, __instance.tileX.Value + 1, __instance.tileY.Value + 1);
                    glStable.characters.Add(horse);
                }
                else
                {
                    logger.Log($"   found existing horse. horse={horse.Name}, {glStable.Name}", LogLevel.Debug);
                    Game1.warpCharacter(horse, glStable.Name, new Point(__instance.tileX.Value + 1, __instance.tileY.Value + 1));
                }
                horse.ownerId.Value = __instance.owner.Value;
            }

            return false;
        }
    }
}
