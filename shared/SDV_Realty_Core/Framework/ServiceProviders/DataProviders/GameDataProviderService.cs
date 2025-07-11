using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using System;
using System.Collections.Generic;
using StardewModdingAPI.Events;

namespace SDV_Realty_Core.Framework.ServiceProviders.DataProviders
{
    internal class GameDataProviderService : IGameDataProviderService
    {
        private ChairTilesDataProviders chairTiles;
        private NPCGiftTastesDataProvider NPCTastes;
        private IExpansionManager _expansionManager;
        private IUtilitiesService _utilitiesService;
        private ILandManager _landManager;
        private IGridManager _gridManager;
        private IExitsService _exitsService;
        public override Type ServiceType => typeof(IGameDataProviderService);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IContentManagerService),typeof(IConfigService),
            typeof(IContentPackService),typeof(IModHelperService),
            typeof(ICustomEntitiesServices),typeof(IExitsService),
            typeof(IExpansionManager),typeof(IUtilitiesService),
            typeof(ILandManager), typeof(IGridManager),
            typeof(IModDataService),typeof(ICustomMovieService)

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
            IContentManagerService contentManagerService = (IContentManagerService)args[0];
            SDRContentManager contentManager = contentManagerService.contentManager;
            IConfigService configService = (IConfigService)args[1];
            
            IContentPackService contentPackService = (IContentPackService)args[2];
            IModHelperService modHelperService = ((IModHelperService)args[3]);
            IModHelper helper = modHelperService.modHelper;
            ICustomEntitiesServices customEntitiesServices = (ICustomEntitiesServices)args[4];
            _exitsService = (IExitsService)args[5];
            _expansionManager = (IExpansionManager)args[6];
            _utilitiesService = (IUtilitiesService)args[7];
            _landManager = (ILandManager)args[8];
            _gridManager = (IGridManager)args[9];
            IModDataService modDataService = (IModDataService)args[10];
            ICustomMovieService customMovieService = (ICustomMovieService)args[11];

            ContentPackLoader contentPacks = contentPackService.contentPackLoader;
            chairTiles = new ChairTilesDataProviders(_utilitiesService, modDataService, contentManager);
            NPCTastes = new NPCGiftTastesDataProvider(contentManager);

            //_utilitiesService.ModHelperService.modHelper.Events.Specialized.LoadStageChanged += Specialized_LoadStageChanged; 
            _utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), HandleLoadStageChanged);

            dataProviders = new List<IGameDataProvider>
            {
                chairTiles,
                NPCTastes,
                new MailDataProvider(modDataService),
                new StringsFromMapsDataProviders( contentManager.stringFromMaps),
                new EventsDataProvider(modDataService),
                new BigCraftablesDataProvider(customEntitiesServices.customBigCraftableService),
                new BuildingsDataProvider(modDataService, customEntitiesServices.customBuildingService.CustomBuildings, _utilitiesService),
                new CraftingRecipesDataProvider(modDataService,customEntitiesServices, _utilitiesService),
                new LocationContextsDataProvider(customEntitiesServices.customLocationContextService),
                new LocationsDataProvider(customEntitiesServices.customBuildingService,modDataService,_expansionManager),
                new MachinesDataProvider(customEntitiesServices.customMachineDataService,_utilitiesService),
                new MineCartsDataProvider(modDataService,_utilitiesService),
                new WorldMapDataProvider(modDataService, _utilitiesService, _expansionManager,_landManager,_gridManager,_exitsService,contentManagerService,modHelperService),
                new ObjectsDataProvider(customEntitiesServices.customObjectService),
                new AudioChangesDataProviders(),
                new CropsDataProvider(customEntitiesServices.customCropService,customEntitiesServices.customObjectService,_utilitiesService),
                new MoviesDataProvider(customMovieService),
                new SDRDataProvider(modDataService, helper),
                new MapsDataProvider(contentManager,_utilitiesService,_expansionManager,modDataService)
                //new TrainBoatStations(_utilitiesService,modDataService)
            };
        }

        private void HandleLoadStageChanged(EventArgs e)
        {
            LoadStageChangedEventArgs stageChangedEventArgs = (LoadStageChangedEventArgs)e;

            if(stageChangedEventArgs.NewStage==  StardewModdingAPI.Enums.LoadStage.SaveParsed)
            {
                foreach(var provider in dataProviders)
                {
                    provider.OnGameLaunched();
                }
            }
        }
    }
}
