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
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using StardewModdingAPI.Events;
using System;


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
            typeof(IModDataKeysService),typeof(ISaveManagerService),
            typeof(IExpansionManager),typeof(ICustomEntitiesServices),
            typeof(ICustomLigthingService),typeof(IMultiplayerService),
            typeof(IStringService),typeof(IUtilitiesService),
            typeof(IMapLoaderService),typeof(IConsoleCommandsService),
            typeof(ITranslationService),typeof(IDataProviderService),
            typeof(IContentPackService),typeof(IExpansionCustomizationService),
            typeof(IContentManagerService),typeof(IForSaleSignService),
            typeof(ISeasonUtilsService),typeof(IFileManagerService),
            typeof(IInputService),typeof(IForSaleMenuService),
            typeof(IModAPIService),
            typeof(ICustomObjectService),
            typeof(ICustomMachineDataService),typeof(ICustomLocationContextService),
            typeof(IExpansionCustomizationService),typeof(IMineCartMenuService),
            typeof(ICustomCropService),typeof(ICustomMovieService),
            typeof(IThreadSafeLoaderService),typeof(IAutoMapperService),
            typeof(IConfigService),typeof(ICustomEventsService),
            typeof(IMapRendererService),typeof(IChatBoxCommandsService),
            typeof(ILocationDisplayService),typeof(ILocationDataProvider),
            typeof(IWordMapMenuService),typeof(IWorldMapPatch),
            // integration services
            typeof(ICentralStationIntegrationService),
            typeof(ILocationTunerIntegrationService),
            //typeof(IQuickSaveIntegration),
            typeof(IGMCMIntergrationService),
            // game mechanics
            typeof(IMiniMapService),
            typeof(ICustomTrainService),typeof(IAutoGrabberService),
            typeof(ICustomCropService),typeof(IGameFixesService),
            typeof(IJunimoHarvesterService),typeof(IJunimoHutService),
            typeof(IValleyStatsService),typeof(ICropGracePeriodService),
            //mod mechanics
            typeof(IFarmServicesService),typeof(IExitsService),
            typeof(ICustomProductionService),//typeof(IExpansionBridgingService),
            typeof(IFishStockService),typeof(IGridManager),typeof(ILandManager),
            typeof(ITreasureManager),typeof(IWarproomService),
            typeof(IWeatherManagerService)

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
