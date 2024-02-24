using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using System;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class ExpansionManagerService : IExpansionManager
    {

        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IContentPackService),
            typeof(IGridManager),typeof(IContentManagerService),
            typeof(ILandManager),typeof(IExitsService),
            typeof(IForSaleSignService),typeof(IModDataService)
        };
        public override List<string> CustomServiceEventSubscripitions => new List<string>
        {
            "ActivateExpansion"
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
            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            IContentPackService contentPackService= (IContentPackService)args[1];
            IGridManager gridManager = (IGridManager)args[2];
            IContentManagerService contentManagerService = (IContentManagerService)args[3];
            ILandManager landManager = (ILandManager)args[4];
            IExitsService exitsService= (IExitsService)args[5];
            IForSaleSignService forSaleSignService= (IForSaleSignService)args[6];
            IModDataService modDataService = (IModDataService)args[7];

            IModHelper helper = utilitiesService.ModHelperService.modHelper;


            expansionManager = new ExpansionManager(logger, utilitiesService, contentPackService, gridManager, contentManagerService, landManager, exitsService, forSaleSignService, modDataService);

            utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), expansionManager.DayStarted);
            utilitiesService.GameEventsService.AddSubscription(new ReturnedToTitleEventArgs(), expansionManager.ResetForNewGame);
            utilitiesService.GameEventsService.AddProxyServer("IsExpansion", IsExpansion);
            //utilitiesService.EventsService.AddCustomSubscription("ActivateExpansion", AddExpansion);
            RegisterEventHandler("ActivateExpansion", AddExpansion);
        }
        private object IsExpansion(object[] objs)
        {
            return expansionManager.farmExpansions.ContainsKey(objs[0].ToString());
        }
        private void AddExpansion(object[] args)
        {
            expansionManager.ActivateExpansion(args[0].ToString());
        }
    }
}
