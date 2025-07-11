using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;


namespace SDV_Realty_Core.Framework.ServiceProviders.GameMechanics
{
    internal class ValleyStatsService : IValleyStatsService
    {
        //private ValleyStats ValleyStats;
        private IModDataService modDataService;
        private IUtilitiesService utilitiesService;
        //
        //  dummy object for locking stat records
        //
        private object JunimoLock = new object();

        public override Type[] InitArgs => new Type[]
        {
            typeof(IModDataService),typeof(IGameEventsService),
            typeof(IUtilitiesService)
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
            modDataService = (IModDataService)args[0];
            IGameEventsService gameEventsService = (IGameEventsService)args[1];
            utilitiesService= (IUtilitiesService)args[2];
            //ValleyStats = new ValleyStats(configService.config);

            gameEventsService.AddSubscription(new DayEndingEventArgs(), DayEnding);
        }

        public void DayEnding(EventArgs e)
        {
            if (!modDataService.Config.RealTimeMoney)
            {
                //
                //  junimo costs are not applied in real time
                //  apply yesterdays charges 
                //
                Game1.MasterPlayer.Money -= JunimoTodayHarvestCost;
                Game1.MasterPlayer.Money -= JunimoTodaySeedCost;
                if(JunimoTodayHarvestCost>0 )
                {
                    string message = $"Junimo harvesting charge: {JunimoTodayHarvestCost:N0} for {JunimoTodayCropsHarvested} crops.";
                    logger.Log(message,LogLevel.Debug);
                    utilitiesService.PopMessage(message,0,5000);
                }
                if (  JunimoTodaySeedCost > 0)
                {
                    string message = $"Junimo seeding charge: {JunimoTodaySeedCost:N0} for {JunimoTodayCropsSeeded} seeds.";
                    logger.Log(message, LogLevel.Debug);
                    utilitiesService.PopMessage(message, 0, 5000);
                }
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
