using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.DataProviders;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.DataProviders;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using System;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.ServiceProviders.DataProviders
{
    internal class GameDataProviderService : IGameDataProviderService
    {
        private ChairTilesDataProviders chairTiles;
        private NPCGiftTastesDataProvider NPCTastes;
        //private IFrameworkService _frameworkService;
        private IExpansionManager _expansionManager;
        private IUtilitiesService _UtilitiesService;
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
            SDRContentManager contentManager = ((IContentManagerService)args[0]).contentManager;
            FEConfig config = ((IConfigService)args[1]).config;
            IContentPackService contentPackService = (IContentPackService)args[2];
            IModHelper helper = ((IModHelperService)args[3]).modHelper;
            ICustomEntitiesServices customEntitiesServices = (ICustomEntitiesServices)args[4];
            _exitsService = (IExitsService)args[5];
            _expansionManager = (IExpansionManager)args[6];
            _UtilitiesService = (IUtilitiesService)args[7];
            _landManager = (ILandManager)args[8];
            _gridManager = (IGridManager)args[9];
            IModDataService modDataService = (IModDataService)args[10];
            ICustomMovieService customMovieService= (ICustomMovieService)args[11];

                       ContentPackLoader contentPacks = contentPackService.contentPackLoader;
            chairTiles = new ChairTilesDataProviders(config.AddBridgeSeat, contentManager);
            NPCTastes = new NPCGiftTastesDataProvider(contentManager);


            dataProviders = new List<IGameDataProvider>
            {
                chairTiles,
                NPCTastes,
                new MailDataProvider(contentPackService.contentPackLoader),
                //new Backwoods(),
                new StringsFromMapsDataProviders( contentManager.stringFromMaps),
                new EventsDataProvider(modDataService),
                new BigCraftablesDataProvider(customEntitiesServices.customBigCraftableService),
                new BuildingsDataProvider(customEntitiesServices.customBuildingService.CustomBuildings, config.SkipBuildingConditions),
                new CraftingRecipesDataProvider(customEntitiesServices, _UtilitiesService),
                new LocationContextsDataProvider(customEntitiesServices.customLocationContextService),
                new LocationsDataProvider(customEntitiesServices.customBuildingService,modDataService),
                new MachinesDataProvider(customEntitiesServices.customMachineDataService,_UtilitiesService),
                new MineCartsDataProvider(modDataService,_UtilitiesService),
                new WorldMapDataProvider(contentPacks,_expansionManager,_landManager,_gridManager,_exitsService),
                new ObjectsDataProvider(customEntitiesServices.customObjectService),
                new AudioChangesDataProviders(),
                new CropsDataProvider(customEntitiesServices.customCropService,customEntitiesServices.customObjectService,_UtilitiesService),
                new MoviesDataProvider(customMovieService),
                new SDRDataProvider(contentPacks, helper, contentManager.ExternalReferences, contentManager.localizationStrings),
                new MapsDataProvider(contentManager,_UtilitiesService,_expansionManager,modDataService)
            };
        }
    }
}
