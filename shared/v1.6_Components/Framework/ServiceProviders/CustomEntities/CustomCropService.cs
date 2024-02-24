using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.CustomEntities.Crops;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.CustomEntities
{
    internal class CustomCropService : ICustomCropService
    {
        private CustomCropManager customCropManager;

        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService),typeof(IModHelperService)
            
        };

        public override Dictionary<string, CustomCropData> Crops => customCropManager.Crops;

        public override Dictionary<string, object> ExternalReferences => customCropManager.ExternalReferences;

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;

            IGameEventsService eventsService = (IGameEventsService)args[0];
            IModHelperService modHelper= (IModHelperService)args[1];
 
            customCropManager = new CustomCropManager(logger, modHelper);

         }

        public override void LoadDefinitions()
        {
            customCropManager.LoadObjectDefinitions();
        }

        public override void AddCropDefinition(CustomCropData cropData)
        {
            customCropManager.AddCropDefinition(cropData);
        }
    }
}
