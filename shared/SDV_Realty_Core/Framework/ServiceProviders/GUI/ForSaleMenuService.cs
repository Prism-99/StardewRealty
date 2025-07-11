using SDV_Realty_Core.Framework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.GUI
{
    internal class ForSaleMenuService : IForSaleMenuService
    {
        private IModDataService modDataService;
        private IUtilitiesService utilitiesService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService),
            typeof(IModDataService),typeof(IUtilitiesService)
        };

        public override FEForSaleMenu GetMenu()
        {
            //forSaleMenu.InitializeGUI();
            FEForSaleMenu newMenu = new FEForSaleMenu(logger,modDataService, utilitiesService);
            newMenu.InitializeGUI();
            return newMenu;
        }

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
            modDataService = (IModDataService)args[1];
            utilitiesService = (IUtilitiesService)args[2];

            forSaleMenu = new FEForSaleMenu(logger,  modDataService, utilitiesService);

            //eventsService.AddSubscription(IGameEventsService.EventTypes.GUILoaded, GUILoaded);
        }
        private void GUILoaded()
        {
            forSaleMenu.InitializeGUI();
        }
    }
}
