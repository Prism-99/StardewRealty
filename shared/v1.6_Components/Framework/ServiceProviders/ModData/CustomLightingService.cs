using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.CustomEntities;
using StardewModdingAPI.Events;
using StardewValley.Buildings;
using System;
using SDV_Realty_Core.Framework.ServiceProviders.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class CustomLightingService : ICustomLigthingService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(ICustomBuildingService),
            typeof(IModDataService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }
       
        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger= logger;
            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            ICustomBuildingService customBuildingService = (ICustomBuildingService)args[1];
            IModDataService modDataService = (IModDataService)args[2];

            customLightingManager = new CustomLightingManager(logger, modDataService, customBuildingService);

            //
            //  add/remove lightSource when a building is removed
            utilitiesService.GameEventsService.AddSubscription(typeof( BuildingListChangedEventArgs).Name, customLightingManager.World_BuildingListChanged);
            //
            //  change lightsource when a buidling is moved
            
            //
            //  set building lightsources once the save is loaded
            utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), customLightingManager.SaveLoaded);
            //
            //  reset added light list
            utilitiesService.GameEventsService.AddSubscription(new ReturnedToTitleEventArgs(), ReturnedToToTitle);
            //  future for adding custom building level brightness
            utilitiesService.CustomEventsService.AddCustomSubscription("SetBuildingBrightness", SetCustomBrightness);
            //
            //  conf changed, adjust gloabl brightness
            utilitiesService.CustomEventsService.AddModEventSubscription(ICustomEventsService.ModEvents.ConfigChanged, ConfigChanged);
        }
        private void ReturnedToToTitle(EventArgs e)
        {
            customLightingManager.ReturnedToTitle();
        }
        private void SetCustomBrightness(object[] args)
        {
            customLightingManager.SetBuildingBrightness((Building)args[0], (float)args[1],true);
        }
        private void ConfigChanged(object[] args)
        {
            customLightingManager.ConfigChanged();
        }
    }
}
