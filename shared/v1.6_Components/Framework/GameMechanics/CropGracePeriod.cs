using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;

namespace SDV_Realty_Core.Framework.GameMechanics
{
    /// <summary>
    /// Version 1.6 logic
    /// </summary>
    internal class CropGracePeriod
    {
        private static ILoggerService logger;
        private static IConfigService config;
        public CropGracePeriod(ILoggerService oLog, IPatchingService Patches,  IConfigService oConfig)
        {
            logger = oLog;
            config = oConfig;
            //
            //  Add required harmony patches
            //
            Patches.patches.AddPatch(true, 1000, typeof(Crop), "IsInSeason",
              new Type[] { typeof(GameLocation) }, typeof(CropGracePeriod), nameof(IsInSeason_Prefix),
             "Add grace period to crops in Farm expansions.", "Crops");

            Patches.patches.AddPatch(true, 1000, typeof(Crop), "IsInSeason",
              new Type[] { typeof(GameLocation), typeof(string) }, typeof(CropGracePeriod), nameof(IsInSeasonWithSeedId_Prefix),
             "Add grace period to crops in Farm expansions.", "Crops");
        }
        public static bool IsInSeasonWithSeedId_Prefix(GameLocation location, string seedId, Crop __instance, ref bool __result)
        {
            if (location.SeedsIgnoreSeasonsHere())
                __result = true;
            else
            {
                if (Crop.TryGetData(seedId, out var data))
                {
                    __result = CheckIsInSeason(data.Seasons, location.GetSeason());
                }
                else
                __result = false;
            }

            return false;
        }
         public static bool IsInSeason_Prefix(GameLocation location, Crop __instance, ref bool __result)
        {
            if (location.SeedsIgnoreSeasonsHere())
                return true;

            __result = CheckIsInSeason(__instance.GetData().Seasons, location.GetSeason());

            return false;
        }
        public static bool CheckIsInSeason(List<Season> seasons, Season currentSeason)
        {
            if (seasons == null)
                return true;

            bool? gameInSeason = seasons.Contains(currentSeason);

            if (!gameInSeason.HasValue || (gameInSeason.HasValue && !gameInSeason.Value))
            {
                int gracePeriod = Game1.season switch
                {
                    Season.Spring =>config.config.SpringGracePeriod,
                    Season.Summer => config.config.SummerGracePeriod,
                    Season.Fall => config.config.FallGracePeriod,
                    Season.Winter => config.config.WinterGracePeriod,
                    _ => config.config.SpringGracePeriod
                };
                return Game1.dayOfMonth < gracePeriod;
            }

            return gameInSeason.Value;
        }
    }
}
