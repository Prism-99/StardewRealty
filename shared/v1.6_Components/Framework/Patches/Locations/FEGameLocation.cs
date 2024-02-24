using Microsoft.Xna.Framework.Graphics;
using Prism99_Core.Utilities;

using System.Linq;

namespace SDV_Realty_Core.Framework.Patches.Locations
{
    internal static class FEGameLocation
    {
        private static SDVLogger logger;
        public static void Initialize(SDVLogger olog)
        {
            logger= olog;
        }
      
        /// <summary>
        /// Fix for getting hay from anywhere, now in game code
        /// </summary>
        /// <param name="currentLocation"></param>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool GetHayFromAnySilo(GameLocation currentLocation, GameLocation __instance,ref SDObject __result)
        {
            logger.Log($"Getting hay for {currentLocation.Name}", StardewModdingAPI.LogLevel.Debug);
            if (TryGetHayFrom(currentLocation, out var hay))
            {
                __result= hay;
                return false;
            }
            if (currentLocation.Name != "Farm" && TryGetHayFrom(Game1.getFarm(), out hay))
            {
                __result= hay;
                return false;
            }
            logger.Log($"Location count {Game1.locations.Count()}", StardewModdingAPI.LogLevel.Debug);
            logger.Log($"MegaFarm exists {Game1.getLocationFromName("MegaFarm")!=null}", StardewModdingAPI.LogLevel.Debug);
            Utility.ForEachLocation((GameLocation location) => !TryGetHayFrom(location, out hay), includeInteriors: false);
            __result= hay;
            return false;

            static bool TryGetHayFrom(GameLocation location, out SDObject foundHay)
            {
                logger.Log($"TryGetHayFrom: {location.Name}, pieces: {location.piecesOfHay.Value}",StardewModdingAPI.LogLevel.Debug);
                if (location.piecesOfHay.Value < 1)
                {
                    foundHay = null;
                    return false;
                }
                foundHay = ItemRegistry.Create<SDObject>("(O)178");
                logger.Log($"Found hay in {location.Name}", StardewModdingAPI.LogLevel.Debug);
                location.piecesOfHay.Value--;
                return true;
            }
        }
        public static void draw(SpriteBatch b)
        {
            SpriteFont font = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont);
            b.DrawString(font, "Hello", new Vector2(10, 10), Color.White);
        }
        //public static bool CheckItemPlantRules(List<PlantableRule> rules, bool isGardenPot, bool defaultAllowed, out string deniedMessage, GameLocation __instance, ref bool __result)
        //{
        //    if (rules != null && rules.Count > 0)
        //    {
        //        foreach (PlantableRule rule in rules)
        //        {
        //            if (rule.ShouldApplyWhen(isGardenPot) && GameStateQuery.CheckConditions(rule.Condition, __instance))
        //            {
        //                switch (rule.Result)
        //                {
        //                    case PlantableResult.Allow:
        //                        deniedMessage = null;
        //                        __result = true;
        //                        return false;
        //                    case PlantableResult.Deny:
        //                        deniedMessage = TokenParser.ParseText(rule.DeniedMessage);
        //                        __result = false;
        //                        return false;
        //                    default:
        //                        deniedMessage = ((!defaultAllowed) ? TokenParser.ParseText(rule.DeniedMessage) : null);
        //                        __result = defaultAllowed;
        //                        return false;
        //                }
        //            }
        //        }
        //    }
        //    deniedMessage = null;
        //    if (__instance is FarmExpansionLocation)
        //    {
        //        __result = true;
        //    }
        //    else
        //    {
        //        __result = defaultAllowed;
        //    }
        //    return false;
        //}

  
    }
}
