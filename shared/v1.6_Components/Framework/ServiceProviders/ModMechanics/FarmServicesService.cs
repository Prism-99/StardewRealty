using CustomMenuFramework.Menus;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ModMechanics.FarmServiceProviders;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class FarmServicesService : IFarmServicesService
    {
        private IModDataService modDataService;
        private IUtilitiesService utilitiesService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModDataService),typeof(IUtilitiesService)
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
            modDataService = (IModDataService)args[0];
            utilitiesService = (IUtilitiesService)args[1];

            GameLocation.RegisterTileAction(ModKeyStrings.Action_FarmServices, HandleFarmServicesAction);
            LoadVanillaServices();
 
        }
        public override void AddService(IFarmServiceProvider service)
        {
            services.Add(service.Key, service);
        }
       
        private void LoadVanillaServices()
        {
            AddService(new WeedFarm(modDataService, utilitiesService));
            AddService(new ClearFarm(modDataService,utilitiesService));
            AddService(new HoeFarm(modDataService,utilitiesService,logger));
            AddService(new ApplyFertilizer(modDataService, utilitiesService));
            AddService(new PlantCrops(modDataService,utilitiesService, logger));
        }
        private bool HandleFarmServicesAction(GameLocation loctaion, string[] args, Farmer who, Point pos)
        {
            List<KeyValuePair<string, string>> choices = new List<KeyValuePair<string, string>>();

            foreach (var service in services) 
            { 
                choices.Add(new KeyValuePair<string, string>(service.Key,service.Value.DisplayValue));
            }

            GenericPickListMenu pickMode = new GenericPickListMenu();
            pickMode.ShowPagedResponses("What service are you looking for?", choices, delegate (string value)
            {
                if(services.TryGetValue(value,out IFarmServiceProvider service))
                {
                    service.PerformAction(loctaion,args,who,pos);
                }
            }, auto_select_single_choice: false);
            return true;
        }       
    }
}
