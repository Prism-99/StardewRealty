using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class WeatherManagerService : IWeatherManagerService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IGridManager),
            typeof(IExpansionManager),typeof(IModDataService)
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

            IUtilitiesService utilitiesService= (IUtilitiesService)args[0];
            IGridManager  gridManager= (IGridManager)args[1];
            IExpansionManager expansionManager= (IExpansionManager)args[2];
            IModDataService modDataService= (IModDataService)args[3];

            WeatherManager = new Weather.WeatherManager(logger, utilitiesService, gridManager.MapGrid, expansionManager, modDataService);
        }
    }
}
