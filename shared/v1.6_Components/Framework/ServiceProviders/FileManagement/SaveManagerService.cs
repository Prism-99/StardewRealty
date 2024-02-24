using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.Saves;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using StardewModdingAPI.Events;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.FileManagement
{
    internal class SaveManagerService : ISaveManagerService
    {
        public override Type ServiceType => typeof(ISaveManagerService);

        public override Type[] InitArgs => new Type[] 
        {
            typeof(IModHelperService),typeof(IGameEventsService),
            typeof(IWarproomService),typeof(IUtilitiesService),
            typeof(IGridManager),typeof(IExpansionManager),
            typeof(IContentPackService),typeof(IExitsService),
            typeof(IModDataService)
        };
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger= logger;
            IModHelper helper = ((IModHelperService)args[0]).modHelper; 
            IGameEventsService eventsService= (IGameEventsService)args[1];
            IWarproomService warproomService = (IWarproomService)args[2];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[3];
            IGridManager gridManager = (IGridManager)args[4];
            IExpansionManager expansionManager = (IExpansionManager)args[5];
            IContentPackService contentPackService= (IContentPackService)args[6];
            IExitsService exitsService= (IExitsService)args[7];
            IModDataService modDataService = (IModDataService)args[8];

            saveManager = new SaveManagerV2();

            saveManager.Initialize((SDVLogger)logger.CustomLogger, utilitiesService, warproomService, gridManager, expansionManager, contentPackService, exitsService, modDataService);

            eventsService.AddSubscription(new SaveCreatedEventArgs(), saveManager.SaveCreated) ;
            eventsService.AddSubscription(new SaveCreatingEventArgs(), saveManager.GameLoop_SaveCreating);
            eventsService.AddSubscription(new LoadStageChangedEventArgs(0,0), saveManager.Specialized_LoadStageChanged,20);
            eventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), saveManager.Specialized_LoadStageChanged_Step2,5);
            eventsService.AddSubscription(new ReturnedToTitleEventArgs(), saveManager.ExitedToTitle);
            eventsService.AddSubscription(new DayStartedEventArgs(), saveManager.DoDayStartedTasks);
            eventsService.AddSubscription(new SavingEventArgs(), saveManager.PreSaveProcessing);
 
        }
    }
}
