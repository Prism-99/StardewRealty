using locationDataDisplay;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.GUI
{
    internal class LocationDisplayService : ILocationDisplayService
    {
        private LocationDataDisplay _locationDataDisplay;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService),typeof(IGameEventsService)
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
            IModHelperService modHelperService= (IModHelperService)args[0];
            IGameEventsService gameEventsService= (IGameEventsService)args[1];

           _locationDataDisplay = new LocationDataDisplay(modHelperService, logger, gameEventsService);
        }
    }
}
