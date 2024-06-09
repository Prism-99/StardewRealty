using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;

namespace SDV_Realty_Core.Framework.ServiceProviders.CustomEntities
{
    internal class CustomEntitiesService : ICustomEntitiesServices
    {

        public override Type ServiceType => typeof(ICustomEntitiesServices);
 
        public override Type[] InitArgs => new Type[] {
            typeof(IContentManagerService), typeof(ICustomBigCraftableService),
            typeof(ICustomMachineService), typeof(IUtilitiesService),
            typeof(ICustomBuildingService),typeof(ICustomObjectService),
            typeof(ICustomCropService),typeof(ICustomMachineDataService),
            typeof(ICustomLocationContextService),typeof(ICustomMovieService)
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
            customBigCraftableService = (ICustomBigCraftableService)args[1];
            customMachineService= (ICustomMachineService)args[2];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[3];
            customBuildingService = (ICustomBuildingService)args[4];
            customObjectService = (ICustomObjectService)args[5];
            customCropService= (ICustomCropService)args[6];
            customMachineDataService = (ICustomMachineDataService)args[7];
            customLocationContextService = (ICustomLocationContextService)args[8];
            customMovieService = (ICustomMovieService)args[9];
  
            IModHelper helper = utilitiesService.ModHelperService.modHelper;
            contentManager = contentManagerService.contentManager;
            IMonitor monitor = utilitiesService.MonitorService.monitor;
            //GamePatches patches=utilitiesService.PatchingService.patches;
            //
            //  initialize custom entity managers
            //
            //CustomBuildingManager.Initialize(logger, helper, monitor, patches, contentManager);
            // moved to ICustomObjectService
            //CustomObjectManager.Initialize((SDVLogger)logger.CustomLogger, helper, contentManager);
            // moved to ICustomCropService
            //CustomCropManager.Initialize((SDVLogger)logger.CustomLogger, helper, contentManager);
            //CustomMovieManager.Initialize((SDVLogger)logger.CustomLogger, helper, contentManager);
            // moved to ICustomMachineDataService
            //CustomMachineDataManager.Initialize((SDVLogger)logger.CustomLogger, helper, contentManager);

            // moved to ICustomBigCraftableService
            //customBigCraftableService.customBigCraftableManager.Initialize((SDVLogger)logger.CustomLogger, helper);
            // moved to ICustomMachineService
            //customMachineService.customMachineManager.Initialize((SDVLogger)logger.CustomLogger, helper, customBigCraftableService);
 
            utilitiesService.GameEventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, ContentLoaded);
            utilitiesService.GameEventsService.AddSubscription(new GameLaunchedEventArgs(), GameLaunched,100);
        }
        /// <summary>
        /// Loaded custom content packs
        /// </summary>
        /// <param name="eventArgs"></param>
        internal void GameLaunched(EventArgs eventArgs)
        {
            customObjectService.LoadDefinitions();
            customCropService.LoadDefinitions();
            customBigCraftableService.LoadDefinitions();
            customMachineService.LoadDefinitions();
            customMachineDataService.LoadDefinitions();
            customLocationContextService.LoadDefinitions();
            customBuildingService.LoadDefinitions();
            customMovieService.LoadDefinitions();
        }
        internal void ContentLoaded()
        {
            //
            //  add custom entity external references
            //
            //  BigCraftables
            //            
            foreach (KeyValuePair<string, object> extRef in customBigCraftableService.ExternalReferences)
            {
                contentManager.ExternalReferences.Add(extRef.Key, extRef.Value);
            }
            //
            //  buildings
            //
            foreach (KeyValuePair<string, object> buildingRef in customBuildingService.ExternalReferences)
            {
                contentManager.ExternalReferences.Add(buildingRef.Key, buildingRef.Value);
            }
            //
            //  crops
            //
            foreach (KeyValuePair<string, object> cropRef in customCropService.ExternalReferences)
            {
                contentManager.ExternalReferences.Add(cropRef.Key, cropRef.Value);
            }
            //
            //  movies
            //
            foreach(KeyValuePair<string, object> movieRef in customMovieService.ExternalReferences)
            {
                contentManager.ExternalReferences.Add(movieRef.Key, movieRef.Value);
            }
            //
            //  objects
            //
            foreach(KeyValuePair<string, object> objExtRef in customObjectService.ExternalReferences)
            {
                contentManager.ExternalReferences.Add(objExtRef.Key, objExtRef.Value);
            }
        }
    }
}
