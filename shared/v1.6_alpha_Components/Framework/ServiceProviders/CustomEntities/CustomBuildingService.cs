using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.CustomEntities.Buildings;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.Buildings;

namespace SDV_Realty_Core.Framework.ServiceProviders.CustomEntities
{
    internal class CustomBuildingService : ICustomBuildingService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IModDataService)
   
        };

        public override Dictionary<string, ICustomBuilding> CustomBuildings => customBuildingManager.CustomBuildings;
        public override Dictionary<string, object> ExternalReferences => customBuildingManager.ExternalReferences;
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger=logger;
            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            IModDataService modDataService = (IModDataService)args[1];

            customBuildingManager = new CustomBuildingManager(logger, utilitiesService, modDataService);
         }
        public override void LoadDefinitions()
        {
            customBuildingManager.LoadDefinitions();
        }
    }
}
