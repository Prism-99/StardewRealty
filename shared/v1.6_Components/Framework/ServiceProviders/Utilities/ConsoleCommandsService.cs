using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;

namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class ConsoleCommandsService : IConsoleCommandsService
    {

        public override Type[] InitArgs => new Type[] 
        {
            typeof(IModHelperService),typeof(IGameEventsService),
            typeof(IUtilitiesService),typeof(IFishStockService),
            typeof(ILandManager),typeof(IModDataService),
            typeof(IJunimoHarvesterService),typeof(IContentManagerService),
            typeof(IValleyStatsService),typeof(IMapRendererService)
        };
        private IModHelper helper;
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger=logger;
            helper = ((IModHelperService)args[0]).modHelper;
            IGameEventsService eventsService = (IGameEventsService)args[1];
            IUtilitiesService utilities = (IUtilitiesService)args[2];
            IFishStockService fishStockService= (IFishStockService)args[3];
            ILandManager landManager = (ILandManager)args[4];
            IModDataService modDataService = (IModDataService)args[5];
            IJunimoHarvesterService junimoHarvesterService = (IJunimoHarvesterService)args[6];
            IContentManagerService contentManager = (IContentManagerService)args[7];
            IValleyStatsService valleyStatsService = (IValleyStatsService)args[8];
            IMapRendererService  mapRendererService = (IMapRendererService)args[9];

            eventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, HandleContentLoaded);

            consoleCommands=new ConsoleCommands(logger, utilities, fishStockService, landManager, junimoHarvesterService, modDataService, contentManager, valleyStatsService, mapRendererService);
        }

        private void HandleContentLoaded()
        {
            consoleCommands.AddCommands(helper);
        }
    }
}
