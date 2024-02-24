using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.CustomEntities
{
    internal class CustomBigCraftableService : ICustomBigCraftableService
    {
        private IUtilitiesService _UtilitiesService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService),typeof(IUtilitiesService)
        };

        public override Dictionary<string, object> ExternalReferences => customBigCraftableManager.ExternalReferences;
        public override Dictionary<string, CustomBigCraftableData> BigCraftables => customBigCraftableManager.BigCraftables;
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger=logger;
            IModHelperService modHelperService = (IModHelperService)args[0];
            _UtilitiesService = (IUtilitiesService)args[1];

            customBigCraftableManager =new CustomBigCraftableManager(logger, modHelperService);
         }
        public override void LoadDefinitions()
        {
            customBigCraftableManager.LoadDefinitions();
            _UtilitiesService.InvalidateCache("Data/BigCraftables");

        }
    }
}
