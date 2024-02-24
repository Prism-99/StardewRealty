using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Prism99_Core.Utilities;
using StardewValley;
//using SDObject = StardewValley.Object;

namespace SDV_Realty_Core.Framework.Patches.Integrations
{
    internal class FEBFAV
    {
        private static SDVLogger logger;
        //this is the only section of code that needs tuning to work with SDR
        //
        //	the 2nd last line GetFarm needs to look for the correct Farm
        //
        public static void Initialize(SDVLogger olog)
        {
            logger = olog;
        }
        public static bool AttemptToSpawnProduce(ref object moddedAnimal, Farmer who)
        {
            logger.Log($"$$$ BFAV.AttemptToSpawnProduce $$$", StardewModdingAPI.LogLevel.Debug);
            //BetterFarmAnimalVariety.Framework.Decorators.FarmAnimal
            //Type tBFAV = Type.GetType("BetterFarmAnimalVariety.Framework.Decorators.FarmAnimal, BetterFarmAnimalVariety");
            FarmAnimal oAnimal = Traverse.Create(moddedAnimal).Method("GetOriginal").GetValue<FarmAnimal>();
            logger.Log($"   {oAnimal.displayName}({oAnimal.displayType}), {oAnimal.home.indoors.Name}", StardewModdingAPI.LogLevel.Debug);
 
            return true;
        }
    }
}
