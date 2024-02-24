using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.Objects;
using StardewModdingAPI;
using StardewValley;

namespace SDV_Realty_Core.ContentPackFramework.Utilities
{
    internal  class ValleyStats
    {
        //
        //  dummy object for locking stat records
        //
        private static object JunimoLock=new object();
        //
        //  tracked stats
        //
        public static int JunimoTodayCropsHarvested { get; private set; }
        public static int JunimoTodayHarvestCost { get; private set; }
        public static int JunimoTodayCropsSeeded { get; private set; }
        public static int JunimoTodaySeedCost { get; private set; }

        public static int JunimoTotalCropsHarvested { get; private set; }
        public static int JunimoTotalHarvestCost { get; private set; }
        public static int JunimoTotalCropsSeeded { get; private set; }
        public static int JunimoTotalSeedCost { get; private set; }

        public static FEConfig config;

        public ValleyStats(FEConfig oconfig)
        {
            config = oconfig;
        }
        //
        //  events
        //
        public static void DayEnding()
        {
            if (!config.RealTimeMoney)
            {
                //
                //  junimo costs are not applied in real time
                //  apply yesterdays charges 
                //
                Game1.MasterPlayer.Money -= JunimoTodayHarvestCost;
                Game1.MasterPlayer.Money -= JunimoTodaySeedCost;
            }

            JunimoTotalCropsHarvested += JunimoTodayCropsHarvested;
            JunimoTodayCropsHarvested = 0;

            JunimoTotalHarvestCost += JunimoTodayHarvestCost;
            JunimoTodayHarvestCost = 0;

            JunimoTotalCropsSeeded += JunimoTodayCropsSeeded;
            JunimoTodayCropsSeeded = 0;

            JunimoTotalSeedCost += JunimoTodaySeedCost;
            JunimoTodaySeedCost = 0;
        }
        //
        //  public methods
        //
        public static void DumpStats(SDVLogger logger)
        {
            logger.Log($"Crops harvested by Junimos today: {JunimoTodayCropsHarvested}", LogLevel.Info);
            logger.Log($"       Cost for harvesting today: {JunimoTodayHarvestCost}g", LogLevel.Info);
            logger.Log($"   Crops seeded by Junimos today: {JunimoTodayCropsSeeded}", LogLevel.Info);
            logger.Log($"          Cost for seeding today: {JunimoTodaySeedCost}g", LogLevel.Info);
            logger.Log("", LogLevel.Trace);
            logger.Log($"Total Crops harvested by Junimos: {JunimoTotalCropsHarvested}", LogLevel.Info);
            logger.Log($"        Total Cost for havesting: {JunimoTotalHarvestCost}g", LogLevel.Info);
            logger.Log($"   Total Crops seeded by Junimos: {JunimoTotalCropsSeeded}", LogLevel.Info);
            logger.Log($"          Total Cost for seeding: {JunimoTotalSeedCost}g", LogLevel.Info);
        }
        public static void CropHarvested(int quantity, int costper)
        {
            lock(JunimoLock)
            {
                JunimoTodayCropsHarvested += quantity;
                JunimoTodayHarvestCost += quantity * costper;
            }
        }
        public static void CropSeeded(int quantity, int costper)
        {
            lock (JunimoLock)
            {
                JunimoTodayCropsSeeded += quantity;
                JunimoTodaySeedCost += quantity * costper;
            }
        }
    }
}
