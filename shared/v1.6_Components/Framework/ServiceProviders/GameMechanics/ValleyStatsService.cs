using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using StardewModdingAPI.Events;


namespace SDV_Realty_Core.Framework.ServiceProviders.GameMechanics
{
    internal class ValleyStatsService : IValleyStatsService
    {
        //private ValleyStats ValleyStats;
        private IConfigService configService;
        //
        //  dummy object for locking stat records
        //
        private object JunimoLock = new object();

        public override Type[] InitArgs => new Type[]
        {
            typeof(IConfigService),typeof(IGameEventsService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            configService = (IConfigService)args[0];
            IGameEventsService gameEventsService = (IGameEventsService)args[1];

            //ValleyStats = new ValleyStats(configService.config);

            gameEventsService.AddSubscription(new DayEndingEventArgs(), DayEnding);
        }

        public void DayEnding(EventArgs e)
        {
            if (!configService.config.RealTimeMoney)
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
        public override void CropHarvested(int quantity, int costper)
        {
            lock (JunimoLock)
            {
                JunimoTodayCropsHarvested += quantity;
                JunimoTodayHarvestCost += quantity * costper;
            }
        }
        public override void CropSeeded(int quantity, int costper)
        {
            lock (JunimoLock)
            {
                JunimoTodayCropsSeeded += quantity;
                JunimoTodaySeedCost += quantity * costper;
            }
        }
    }
}
