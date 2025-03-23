using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using System;
using System.Collections.Generic;
using System.Linq;


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
            IContentPackService contentPackService = (IContentPackService)args[1];
            IGridManager gridManager = (IGridManager)args[2];
            IContentManagerService contentManagerService = (IContentManagerService)args[3];
            ILandManager landManager = (ILandManager)args[4];
            IExitsService exitsService = (IExitsService)args[5];
            IForSaleSignService forSaleSignService = (IForSaleSignService)args[6];
            IModDataService modDataService = (IModDataService)args[7];

            IModHelper helper = utilitiesService.ModHelperService.modHelper;


            expansionManager = new ExpansionManager(logger, utilitiesService, contentPackService, gridManager, contentManagerService, landManager, exitsService, forSaleSignService, modDataService);

            utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), expansionManager.DayStarted);
            utilitiesService.GameEventsService.AddSubscription(new ReturnedToTitleEventArgs(), expansionManager.ResetForNewGame);
            utilitiesService.GameEventsService.AddProxyServer("IsExpansion", IsExpansion);
            utilitiesService.CustomEventsService.AddCustomSubscription("ActivateExpansion", HandleActivateExpansionEvent);
            //RegisterEventHandler("ActivateExpansion", HandleActivateExpansionEvent);
        }
        private object IsExpansion(object[] objs)
        {
            return expansionManager.farmExpansions.ContainsKey(objs[0].ToString());
        }
        /// <summary>
        /// Handles any ActivateExpansion custom event request.
        /// </summary>
        /// <param name="args">
        ///      0  - ExpansionId
        ///     [1] - GridId
        /// </param>
        private void HandleActivateExpansionEvent(object[] args)
        {
            if (args.Length == 2 && int.TryParse(args[1].ToString(), out int gridId))
                expansionManager.ActivateExpansion(args[0].ToString(), gridId);
            else if (args.Length == 1)
                expansionManager.ActivateExpansion(args[0].ToString());
            else
                logger.Log($"Invalid ActivateExpansion trigger request. {args.ToList()}", LogLevel.Debug);
        }
        public override void ActivateExpansionOnRemote(string expansionName, int gridId)
        {
            expansionManager.ActivateExpansionOnRemote(expansionName, gridId);
        }
        public override bool ActivateExpansion(string expansionName, int gridId = -1)
        {
            return expansionManager.ActivateExpansion(expansionName, gridId);
        }
    }
}
