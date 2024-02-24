using SDV_Realty_Core.Framework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class ForSaleMenuService : IForSaleMenuService
    {
        private ILandManager landManager;
        private IModDataService modDataService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(ILandManager),typeof(IGameEventsService),
            typeof(IModDataService)
        };

        public override FEForSaleMenu GetMenu()
        {
            //forSaleMenu.InitializeGUI();
            FEForSaleMenu newMenu = new FEForSaleMenu(logger,landManager,modDataService);
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

            landManager = (ILandManager)args[0];
            IGameEventsService eventsService = (IGameEventsService)args[1];
            modDataService = (IModDataService)args[2];

            forSaleMenu = new FEForSaleMenu(logger, landManager, modDataService);

            //eventsService.AddSubscription(IGameEventsService.EventTypes.GUILoaded, GUILoaded);
        }
        private void GUILoaded()
        {
            forSaleMenu.InitializeGUI();
        }
    }
}
