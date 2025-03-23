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
        private static IUtilitiesService _utilitiesService;
        public CropGracePeriod(ILoggerService oLog, IPatchingService Patches, IUtilitiesService utilitiesService)
        {
            logger = oLog;
            _utilitiesService = utilitiesService;
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
            if (!_utilitiesService.ConfigService.config.UseGracePeriod || location.SeedsIgnoreSeasonsHere())
                return true;

            if (Crop.TryGetData(seedId, out var data))
            {
                __result = CheckIsInSeason(data.Seasons, location.GetSeason());
            }
            else
                __result = false;

            return false;
        }
        public static bool IsInSeason_Prefix(GameLocation location, Crop __instance, ref bool __result)
        {
            if (!_utilitiesService.ConfigService.config.UseGracePeriod || location.SeedsIgnoreSeasonsHere())
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
                    Season.Spring =>_utilitiesService.ConfigService.config.SpringGracePeriod,
                    Season.Summer => _utilitiesService.ConfigService.config.SummerGracePeriod,
                    Season.Fall => _utilitiesService.ConfigService.config.FallGracePeriod,
                    Season.Winter => _utilitiesService.ConfigService.config.WinterGracePeriod,
                    _ => _utilitiesService.ConfigService.config.SpringGracePeriod
                };
                return Game1.dayOfMonth < gracePeriod;
            }

            return gameInSeason.Value;
        }
    }
}
