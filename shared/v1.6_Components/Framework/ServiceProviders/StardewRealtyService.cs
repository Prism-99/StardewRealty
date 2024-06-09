using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;


namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class StardewRealtyService : IStardewRealty
    {
        private IGameEventsService eventsService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService)
        };
        public override Type[] Dependants => new Type[]
        {
            typeof(IModDataKeysService),typeof(IWarproomService),
            typeof(ISaveManagerService),typeof(IExpansionManager),
            typeof(IGridManager),typeof(ICustomEntitiesServices),
            typeof(ICustomLigthingService),typeof(IMultiplayerService),
            typeof(IStringService),typeof(IUtilitiesService),
            typeof(IMapLoaderService),typeof(IConsoleCommandsService),
            typeof(ITranslationService),typeof(IDataProviderService),
            typeof(IContentPackService),typeof(IExpansionCustomizationService),
            typeof(IContentManagerService),typeof(IExitsService),
            typeof(IFishStockService),typeof(IForSaleSignService),
            typeof(ISeasonUtilsService),typeof(IFileManagerService),
            typeof(IInputService),typeof(IForSaleMenuService),
            typeof(IWeatherManagerService),typeof(IModAPIService),
            typeof(ICGCMIntergrationService),typeof(IAutoGrabberService),
            typeof(ICustomObjectService),typeof(ICustomCropService),
            typeof(ICustomMachineDataService),typeof(ICustomLocationContextService),
            typeof(IExpansionCustomizationService),typeof(IMineCartMenuService),
            typeof(ICustomCropService),typeof(ICustomMovieService),
            typeof(IJunimoHutService),typeof(IJunimoHarvesterService),
            typeof(IThreadSafeLoaderService),typeof(IAutoMapperService),
            typeof(IConfigService),typeof(ICustomEventsService),
            typeof(IValleyStatsService),typeof(IMapRendererService),
            typeof(IGameFixesService),typeof(IChatBoxCommandsService),
            typeof(ILocationDisplayService),typeof(ILocationDataProvider),
            typeof(ITreasureManager),typeof(IWordMapMenuService),
            typeof(IWorldMapPatch)
#if DEBUG
            ,typeof(IDebugPatchService)
#endif

        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return false;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;

            eventsService = (IGameEventsService)args[0];
            //
            //  use game loaded as trigger for gui loaded mod event
            //
            eventsService.AddSubscription(new GameLaunchedEventArgs(), GameLaunched);
        }
        private void GameLaunched(EventArgs e)
        {
            eventsService.TriggerEvent(IGameEventsService.EventTypes.ContentLoaded);
            eventsService.TriggerEvent(IGameEventsService.EventTypes.GUILoaded);
        }
    }
}
