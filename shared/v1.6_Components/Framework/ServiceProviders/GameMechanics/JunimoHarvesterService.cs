using SDV_Realty_Core.Framework.Patches.Characters;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;
using StardewValley.Characters;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.GameMechanics
{
    internal class JunimoHarvesterService : IJunimoHarvesterService
    {
        private float JunimoSeedDiscount;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IExpansionManager),
            typeof(IPatchingService),typeof(IValleyStatsService)
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

            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            IExpansionManager expansionManager = (IExpansionManager)args[1];
            IPatchingService patchingService = (IPatchingService)args[2];
            IValleyStatsService valleyStatsService = (IValleyStatsService)args[3];

            JunimoHarvester = new FEJuminoHarvester();
            JunimoHarvester.Initialize(logger, utilitiesService.ConfigService.config, expansionManager, valleyStatsService);

            JunimoSeedDiscount = utilitiesService.ConfigService.config.JunimoSeedDiscount;

            utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), SaveLoaded);

            patchingService.patches.AddPatch(false, typeof(JunimoHarvester), "update",
              new Type[] { typeof(GameTime), typeof(GameLocation) }, typeof(FEJuminoHarvester),
              nameof(FEJuminoHarvester.update_post), "Patch for Junimo Harvestor update, for fees and re-seeding.",
              "JunimoHarvester");


        }

        public void SaveLoaded(EventArgs e)
        {
            JunimoHarvester.LoadSeeds(JunimoSeedDiscount);

        }

    }
}
