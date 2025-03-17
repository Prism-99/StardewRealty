using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.Framework.GameMechanics
{
    /// <summary>
    /// Version 1.6 logic
    /// </summary>
    internal class CropGracePeriod
    {
        private static ILoggerService logger;
        private static IModDataService _modDataService;
        public CropGracePeriod(ILoggerService oLog, IModDataService modDataService, IPatchingService Patches)
        {
            logger = oLog;
            _modDataService = modDataService;
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
            if (!_modDataService.Config.UseGracePeriod || location.SeedsIgnoreSeasonsHere())
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
            if (!_modDataService.Config.UseGracePeriod || location.SeedsIgnoreSeasonsHere())
                return true;

            if (!__instance.forageCrop.Value)
            {
                __result = CheckIsInSeason(__instance.GetData().Seasons, location.GetSeason());
                return false;
            }

            return true;
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
                    Season.Spring => _modDataService.Config.SpringGracePeriod,
                    Season.Summer => _modDataService.Config.SummerGracePeriod,
                    Season.Fall => _modDataService.Config.FallGracePeriod,
                    Season.Winter => _modDataService.Config.WinterGracePeriod,
                    _ => _modDataService.Config.SpringGracePeriod
                };
                return Game1.dayOfMonth < gracePeriod;
            }

            return gameInSeason.Value;
        }
    }
}
