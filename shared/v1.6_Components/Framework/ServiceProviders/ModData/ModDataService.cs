using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceProviders.ModMechanics;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class ModDataService : IModDataService
    {
        private IUtilitiesService _UtilitiesService;
        public override Type[] InitArgs => new Type[]
        {
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
            _UtilitiesService = (IUtilitiesService)args[0];

            _UtilitiesService.GameEventsService.AddSubscription(new ReturnedToTitleEventArgs(), HandleReturnToTitle);
         }

        private void HandleReturnToTitle(EventArgs e)
        {
            SubLocations.Clear();
            MapGrid.Clear();
        }
    }
}
