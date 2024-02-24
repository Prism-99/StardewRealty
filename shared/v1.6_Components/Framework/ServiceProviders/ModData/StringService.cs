using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class StringService : IStringService
    {
        private IContentManagerService contentManager;
        private ICustomEntitiesServices customEntitiesServices;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService),typeof(IContentManagerService),
            typeof(ICustomEntitiesServices)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger=logger;
            IGameEventsService eventsService = (IGameEventsService)args[0];
            contentManager = (IContentManagerService)args[1];
            customEntitiesServices= (ICustomEntitiesServices)args[2];

            eventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, PostContentLoaded);
        }
        internal void PostContentLoaded()
        {
            //
            //  add custom entity strings
            contentManager.contentManager.localizationStrings.AddStrings(customEntitiesServices.customBigCraftableService.customBigCraftableManager.BigCraftables);
            contentManager.contentManager.localizationStrings.AddStrings(customEntitiesServices.customBuildingService.customBuildingManager.CustomBuildings);
            contentManager.contentManager.localizationStrings.AddStrings(customEntitiesServices.customObjectService.objects);

            contentManager.contentManager.localizationStrings.GenerateDictionary();

        }
    }
}
