using Prism99_Core.Abstractions;
using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Utilities;
using StardewValley.Mods;
using System.Collections.Generic;
using System.Linq;

namespace SDV_Realty_Core.Framework.Utilities
{
    internal  class SeasonalUtils
    {
        private  FEConfig config;
        private  SDVLogger logger;
        private IExpansionManager expansionManager;
        public  void Initialize(FEConfig oconfig,SDVLogger olog, IExpansionManager expansionManager)
        {
            config = oconfig;
            logger = olog;
            this.expansionManager = expansionManager;
        }
        public  bool isWinter(ModDataDictionary modData)
        {
            bool isWinter = Game1.IsWinter;
            if (modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                isWinter = Game1.getLocationFromName(modData[FEModDataKeys.FELocationName]).GetSeason() == Season.Winter;
            }

            return isWinter;
        }
        public  int GetSeasonIndex(string season)
        {
            //
            //  convert text season to numeric season
            //
            return season.ToLower() switch
            {
                "spring" => 0,
                "summer" => 1,
                "fall" => 2,
                "winter" => 3,
                _ => -1
            };
        }
        public  int GetMaxSeason(List<string> seasonsToGrowIn)
        {
            //
            //  get the latest growing season
            //  for the supplied crop
            //
            int maxSeason = -1;
            foreach (string season in seasonsToGrowIn)
            {
                int seaVal = GetSeasonIndex(season);
                if (seaVal > maxSeason)
                    maxSeason = seaVal;
            }

            return maxSeason;
        }
        public  int GetSeasonOffset(int seasonIndex)
        {
            //
            //  get the defined grace period for the 
            //  supplied season
            //
            return seasonIndex switch
            {
                0 => config.SpringGracePeriod,
                1 => config.SummerGracePeriod,
                2 => config.FallGracePeriod,
                3 => config.WinterGracePeriod,
                _ => 0
            };
        }
        public  bool IsCropInGracePeriod(GameLocation location, Crop crop)
        {

#if v16
            return crop.IsInSeason(location);
#else
            if (crop.seasonsToGrowIn != null && crop.seasonsToGrowIn.Count > 0)
            {
                return IsCropInGracePeriod(location, crop.seasonsToGrowIn.ToList());
            }
            
            return true;
#endif
            
        }
        public  bool IsCropInGracePeriod(GameLocation loc, List<string> seasonsToGrowIn)
        {
            //
            //  verify if a crop can keep growing
            //
            //
            //  check for greenhouse
            //
            if (loc.IsGreenhouse || loc.SeedsIgnoreSeasonsHere() || loc.modData.ContainsKey(FEModDataKeys.FEGreenhouse) || loc.modData.ContainsKey(FEModDataKeys.FELargeGreenhouse))
            {
                return true;
            }

            prism_GameLocation ploc = new prism_GameLocation(loc);
            string locName = loc.NameOrUniqueName;
            if (loc.modData.ContainsKey(FEModDataKeys.FELocationName))
            {
                locName = loc.modData[FEModDataKeys.FELocationName];
            }
            string customSeason = expansionManager.expansionManager.GetExpansionSeasonalOverride(locName);

            //logger.Log($"    IsCropInGracePeriod: {locName},{customSeason},{string.Join(',', seasonsToGrowIn) }", StardewModdingAPI.LogLevel.Debug);

            string curSeason = string.IsNullOrEmpty(customSeason) ? Game1.currentSeason : customSeason;

            //
            //  check if current season is valid
            //
            if (seasonsToGrowIn.Where(p=>p.ToLower()==curSeason.ToLower()).Any()) { return true; }
            //
            //  if grace period is not enabled
            //  crop is out of season
            //
            if (!config.UseGracePeriod) { return false; }

            int maxGrowingSeason = GetMaxSeason(seasonsToGrowIn);
            int curSeasonIndex = GetSeasonIndex(curSeason);

            bool checkRange = maxGrowingSeason switch
            {
                int mx when mx == 3 && curSeasonIndex == 0 => true,
                int my when my < 3 && curSeasonIndex == my + 1 => true,
                _ => false
            };

            if (checkRange)
            {
                //
                //  if within season range, check day range
                //
                SDate today = SDate.Now();
                return GetSeasonOffset(curSeasonIndex) >= today.Day;
            }

            return false;
        }
    }
}
