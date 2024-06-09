using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System.Linq;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using xTile;
using xTile.Layers;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{

    internal class ContentPackServices : IContentPackService
    {
        private IUtilitiesService utilitiesService;
        private IModDataService modDataService;
        private IGameEnvironmentService gameEnvironmentService;
        public override Type ServiceType => typeof(IContentPackService);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService) ,typeof(ICustomEntitiesServices),
            typeof(IModDataService),typeof(IGameEventsService),
            typeof(IAutoMapperService),typeof(IGameEnvironmentService)
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

            contentPackLoader = new ContentPackLoader(logger, utilitiesService, customEntitiesServices, modDataService, autoMapperService, gameEnvironmentService);
            contentPacks = utilitiesService.ModHelperService.modHelper.ContentPacks.GetOwned().ToList();
            eventsService.AddSubscription(new GameLaunchedEventArgs(), LoadContent, 500);
        }
        private void LoadContent(EventArgs e)
        {
            LoadPacks();
            LoadMaps();
            utilitiesService.HaveExpansions = contentPackLoader.ValidContents.Any();
        }
        internal override void LoadPacks()
        {
            contentPackLoader.LoadPacks(contentPacks);
        }

        internal override void LoadMaps()
        {
            contentPackLoader.LoadMaps();
        }
    }
}
