using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceProviders;
using SDV_Realty_Core.Framework.ServiceProviders.CustomEntities;
using SDV_Realty_Core.Framework.ServiceProviders.DataProviders;
using SDV_Realty_Core.Framework.ServiceProviders.Integrations;
using SDV_Realty_Core.Framework.ServiceProviders.Patches;
using SDV_Realty_Core.Framework.ServiceProviders.Events;
using SDV_Realty_Core.Framework.ServiceProviders.GUI;
using SDV_Realty_Core.Framework.ServiceProviders.GameMechanics;
using SDV_Realty_Core.Framework.ServiceProviders.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.ServiceProviders.Configuration;
using SDV_Realty_Core.Framework.ServiceProviders.Game;
using SDV_Realty_Core.Framework.ServiceProviders.FileManagement;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceProviders.ModMechanics;
using SDV_Realty_Core.Framework.ServiceProviders.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceProviders.Services;
using System;
using System.IO;
using System.Collections.Generic;
using StardewModdingAPI.Events;
using static SDV_MapRenderer.SDVMapRenderer;
using SDV_MapRenderer;

namespace SDV_Realty_Core.Framework.Objects
{
    /// <summary>
    /// Mod entry class
    /// </summary>
    internal class ModEntryHandler : Mod
    {
        internal IServicesManager servicesManager;
        public override void Entry(IModHelper helper)
        {
            InitializeApplication(helper);
        }

        public override object GetApi()
        {
            IModAPIService modAPIService = servicesManager.GetService<IModAPIService>(typeof(IModAPIService));
            return modAPIService.GetAPI();
        }
        /// <summary>
        /// Load the required services for the mod application and then
        /// load the mod application.
        /// </summary>
        /// <param name="helper"></param>
        private void InitializeApplication(IModHelper helper)
        {
            //
            //  initialize Service Manager
            //
            servicesManager = new ServicesManager(new SDVServiceLogger(Monitor, helper));
            //
            //  add base mod services
            servicesManager.AddService(new ModHelperService(Helper));
            servicesManager.AddService(new ManifestService(ModManifest));
            servicesManager.AddService(new MonitorService(Monitor));
            servicesManager.AddService(new TranslationService());
            servicesManager.AddService(new ConfigService());
            //
            //  add utility services
            //
            servicesManager.AddService(new ModDataKeysService());
            servicesManager.AddService(new AutoMapService());
            servicesManager.AddService(new CGCMIntergrationService());
            servicesManager.AddService(new ModAPIService());
            //servicesManager.AddService(new FrameworkService());
            servicesManager.AddService(new GridManager());
            servicesManager.AddService(new MapUtilities());
            servicesManager.AddService(new PatchingService());
            servicesManager.AddService(new MapLoaderService());
            servicesManager.AddService(new ConsoleCommandsService());
            servicesManager.AddService(new UtilitiesService());
            servicesManager.AddService(new MulitplayerService());
            servicesManager.AddService(new StringService());
            servicesManager.AddService(new SeasonUtilsService());
            servicesManager.AddService(new InputService());
            servicesManager.AddService(new ForSaleMenuService());
            servicesManager.AddService(new WeatherManagerService());
            servicesManager.AddService(new GameEnviromentService());
            servicesManager.AddService(new ThreadSafeLoaderService());
            servicesManager.AddService(new ValleyStatsService());
            servicesManager.AddService(new LocationDataProviderService());
            //
            //  add event services
            //
            servicesManager.AddService(new CustomEventsService());
            servicesManager.AddService(new GameEventsService());
            //
            //  add gamemechanics services
            //
            servicesManager.AddService(new AutoGrabberService());
            servicesManager.AddService(new CropGracePeriodService());
            servicesManager.AddService(new JunimoHarvesterService());
            servicesManager.AddService(new JunimoHutService());
            servicesManager.AddService(new GameFixesService());
            servicesManager.AddService(new ChatBoxCommandsService());
            //
            //  add gui services
            //
            servicesManager.AddService(new SaveManagerService());
            servicesManager.AddService(new MineCartMenuService());
            servicesManager.AddService(new MapRendererService());
            servicesManager.AddService(new LocationDisplayService());
            servicesManager.AddService(new WorldMapMenuService());
            servicesManager.AddService(new WorldMapPatchService());
            //
            //  add patch services
            //
            servicesManager.AddService(new GameLocationPatchService());
#if DEBUG
            servicesManager.AddService(new DebugPatchService());
#endif
            //
            //  add data services
            //
            servicesManager.AddService(new CustomBigCraftableService());
            servicesManager.AddService(new CustomMachineService());
            servicesManager.AddService(new CustomCropService());
            servicesManager.AddService(new CustomObjectService());
            servicesManager.AddService(new CustomBuildingService());
            servicesManager.AddService(new CustomMachineDataService());
            servicesManager.AddService(new CustomLocationContextService());
            servicesManager.AddService(new ContentManagerService());
            servicesManager.AddService(new ContentPackServices());
            servicesManager.AddService(new DataProviderService());
            servicesManager.AddService(new CustomEntitiesService());
            servicesManager.AddService(new ExpansionCustomizationService());
            servicesManager.AddService(new ExpansionManagerService());
            servicesManager.AddService(new ModDataService());
            servicesManager.AddService(new LandManagerService());
            servicesManager.AddService(new ExpansionCustomizationService());
            servicesManager.AddService(new CustomMovieService());
            //
            //  add logic services
            //
            servicesManager.AddService(new WarproomService());
            servicesManager.AddService(new GameDataProviderService());
            servicesManager.AddService(new SaveManagerService());
            servicesManager.AddService(new CustomLightingService());
            servicesManager.AddService(new ExitsService());
            servicesManager.AddService(new FishStockService());
            servicesManager.AddService(new ForSaleSignService());
            servicesManager.AddService(new JunimoHarvesterService());
            servicesManager.AddService(new FileManagerService());
            servicesManager.AddService(new PLayerMComms());
            servicesManager.AddService(new TreasureManagerService());
            //
            //  add the main application sservice
            //
            servicesManager.AddService(new StardewRealtyService());

#if DEBUG
            //servicesManager.DumpConfiguration();
            servicesManager.VerifyServices();
#endif
            //
            // load mod application
            //
            var mod = servicesManager.GetService<IStardewRealty>(typeof(IStardewRealty));

#if DEBUG
            //var gameEventService = servicesManager.GetService<IGameEventsService>(typeof(IGameEventsService));
            //var customEventService = servicesManager.GetService<ICustomEventsService>(typeof(ICustomEventsService));
            //gameEventService.DumpSubscriptions();
            //customEventService.DumpSubscriptions();

            //  dump map renderings
            //gameEventService.AddSubscription(new SaveLoadedEventArgs(), SaveLoaded);
            //
            //  unit testing
            //
            //var saveManager=servicesManager.GetService<ISaveManagerService>(typeof(ISaveManagerService));
            //saveManager.UnitTest();
#endif
        }
        private void SaveLoaded(EventArgs e)
        {
            //
            //  test of map renderer
            //
            //  renderers both expansions and buildings
            //
            var dataService = servicesManager.GetService<IModDataService>(typeof(IModDataService));
            var mapRenderer = servicesManager.GetService<IMapRendererService>(typeof(IMapRendererService));
            var locationData = servicesManager.GetService<ILocationDataProvider>(typeof(ILocationDataProvider));

            var logger = servicesManager.GetService<ILoggerService>(typeof(ILoggerService));

            //foreach (KeyValuePair<string, xTile.Map> bMap in dataService.BuildingMaps)
            //{
            //    try
            //    {
            //        mapRenderer.RenderMap(bMap.Value,null,false).Save($"RenderedMaps/{Path.GetFileNameWithoutExtension(bMap.Key)}.png");
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.LogError($"Render Building Map {Path.GetFileNameWithoutExtension(bMap.Key)}", ex);
            //    }
            //}
            //
            //  dump expansion maps only
            //
            foreach (var eMap in dataService.ExpansionMaps)
            {
                try
                {
                    //
                    //  get season, if active
                    //
                    GameLocation gl = Game1.getLocationFromName(eMap.Key);
                    Season locationSeason = Season.Spring;
                    if (gl != null)
                    {
                        locationSeason = gl.GetSeason();
                    }

                    SDVMapRenderer.MapOptions mapOptions = new SDVMapRenderer.MapOptions
                    {
                        ShowFishAreas = true,
                        ShowFishAreaName = true,
                        IsWinter = locationSeason == Season.Winter,
                        DrawBuildings = true,
                        DrawPropertyIndicator = true,
                        SkipPathsLayer = true
                    };

                    //mapRenderer.RenderMap(eMap.Value.assetPath, mapOptions);
                    mapRenderer.RenderMap(eMap.Value, locationData.GetData(eMap.Key), mapOptions).Save($"expansionmaps/{eMap.Key}.png");
                }
                catch (Exception ex)
                {
                    servicesManager.logger.LogError($"Render Location Map {eMap.Key}", ex);
                }

                //}
                //
                //  render all location maps in the game
                //
                //foreach (var gameLocation in Game1.locations)
                //{
                //    try
                //    {
                //        SDVMapRenderer.MapOptions mapOptions = new SDVMapRenderer.MapOptions
                //        {
                //            ShowFishAreas = true,
                //            ShowFishAreaName = true,
                //            IsWinter = gameLocation.GetSeason() == Season.Winter,
                //            DrawBuildings = true
                //        };

                //        mapRenderer.RenderMap(gameLocation, mapOptions).Save($"RenderedMaps/{gameLocation.Name}.png");
                //    }
                //    catch (Exception ex)
                //    {
                //        servicesManager.logger.LogError($"Render Location Map {gameLocation.Name}", ex);
                //    }

            }
        }
        private void GameLaunched(EventArgs e)
        {

        }
    }

}
