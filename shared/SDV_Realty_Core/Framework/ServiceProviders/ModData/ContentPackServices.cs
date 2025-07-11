using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System.Linq;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using static SDV_Realty_Core.Framework.ServiceInterfaces.Events.IGameEventsService;
using SDV_Realty_Core.Framework.DataProviders;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{

    internal class ContentPackServices : IContentPackService
    {
        private IUtilitiesService utilitiesService;
        private IModDataService modDataService;
        private IGameEnvironmentService gameEnvironmentService;
        private bool additionalFarmsLoaded = false;
        public override Type ServiceType => typeof(IContentPackService);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService) ,typeof(ICustomEntitiesServices),
            typeof(IModDataService),typeof(IGameEventsService),
            typeof(IAutoMapperService),typeof(IGameEnvironmentService),
            typeof(IContentManagerService)
        };
        public override Type[] Dependants => new Type[]
        {

        };

        public List<IContentPack> contentPacks;
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            utilitiesService = ((IUtilitiesService)args[0]);
            ICustomEntitiesServices customEntitiesServices = (ICustomEntitiesServices)args[1];
            modDataService = (IModDataService)args[2];
            IGameEventsService eventsService = (IGameEventsService)args[3];
            IAutoMapperService autoMapperService = (IAutoMapperService)args[4];
            gameEnvironmentService = (IGameEnvironmentService)args[5];
            IContentManagerService contentManagerService = (IContentManagerService)args[6];

            contentPackLoader = new ContentPackLoader(logger, contentManagerService.ExternalReferences, utilitiesService, customEntitiesServices, modDataService, autoMapperService, gameEnvironmentService);
            contentPacks = utilitiesService.ModHelperService.modHelper.ContentPacks.GetOwned().ToList();
            eventsService.AddSubscription(new GameLaunchedEventArgs(), LoadContent, 500);
            eventsService.AddSubscription(EventTypes.AllModsLoaded, AllModsLoaded);
        }
        /// <summary>
        /// Called after all mods have loaded to add all
        /// AdditionalFarm definitions as Expansions
        /// </summary>
        private void AllModsLoaded()
        {
            if (modDataService.Config.UseAdditionalFarms)
            {
                contentPackLoader.LoadAdditionalFarms();
            }
        }
        internal override void LoadExpansionMap(string expansionName)
        {
            contentPackLoader.LoadExpansionMap(expansionName);
        }
        /// <summary>
        /// Called at GameLaunch to load SDR Content Packs
        /// </summary>
        /// <param name="e"></param>
        private void LoadContent(EventArgs e)
        {
            // load all content packs
            LoadPacks();
            // load maps for ExpansionPacks
            LoadMaps();
            utilitiesService.HaveExpansions = modDataService.validContents.Any();
        }
        internal override void LoadPacks()
        {
            contentPackLoader.LoadPacks(contentPacks);
        }
        /// <summary>
        /// Load maps for loaded validContent records
        /// </summary>
        internal override void LoadMaps()
        {
            contentPackLoader.LoadMaps();
        }
    }
}
