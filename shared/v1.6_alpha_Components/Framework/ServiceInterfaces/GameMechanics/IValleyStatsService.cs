using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics
{
    internal abstract class IValleyStatsService:IService
{
        public override Type ServiceType => typeof(IValleyStatsService);
        public abstract void CropHarvested(int quantity, int costper);
        public abstract void CropSeeded(int quantity, int costper);

        //
        //  tracked stats
        //
        public int JunimoTodayCropsHarvested { get; protected set; }
        public int JunimoTodayHarvestCost { get; protected set; }
        public int JunimoTodayCropsSeeded { get; protected set; }
        public  int JunimoTodaySeedCost { get; protected set; }

        public  int JunimoTotalCropsHarvested { get; protected set; }
        public  int JunimoTotalHarvestCost { get; protected set; }
        public  int JunimoTotalCropsSeeded { get; protected set; }
        public  int JunimoTotalSeedCost { get; protected set; }
        //
        //  public methods
        //
        public  void DumpStats()
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
    }
}
