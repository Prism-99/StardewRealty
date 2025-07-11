using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class ExpansionCustomizationService : IExpansionCustomizationService
    {
        public override Type[] InitArgs => new Type[]
        {
          typeof(IModHelperService),typeof(IModDataService),
          typeof(ICustomEventsService),typeof(IUtilitiesService)
            
        };
        private ExpansionCustomizations _expansionCustomizations;
        private IModDataService _modDataService;
        public override Dictionary<string, CustomizationDetails> CustomDefinitions => _modDataService.CustomDefinitions;

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;

            IModHelperService modHelperService = (IModHelperService)args[0];
            _modDataService = (IModDataService)args[1];
            ICustomEventsService eventsService = (ICustomEventsService)args[2];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[3];

            _expansionCustomizations = new ExpansionCustomizations(logger, modHelperService, _modDataService, utilitiesService);
            LoadDefinitions();
            eventsService.AddCustomSubscription("SaveExpansionCustomizations", SaveExpansionCustomizations);
        }
        private void SaveExpansionCustomizations(object[] args)
        {
            _expansionCustomizations.Save();
        }
        public override void LoadDefinitions()
        {
            _expansionCustomizations.Load();
        }

        public override void SaveDefinitions()
        {
            _expansionCustomizations.Save();
        }
    }
}
