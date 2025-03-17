using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.Utilities;
using StardewValley.Mods;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class SeasonUtilsService : ISeasonUtilsService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModDataService),typeof(IExpansionManager)
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

            IModDataService modDataService = (IModDataService)args[0];
            IExpansionManager expansionManager= (IExpansionManager)args[1];

            SeasonalUtils = new SeasonalUtils();
            SeasonalUtils.Initialize(modDataService.Config, logger,expansionManager);
        }
        public override bool isWinter(ModDataDictionary modData)
        {
            return SeasonalUtils.isWinter(modData);
        }
    }
}
