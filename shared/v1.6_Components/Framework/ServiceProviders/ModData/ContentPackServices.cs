using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System.Linq;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{

    internal class ContentPackServices : IContentPackService
    {
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
            IUtilitiesService utilities = ((IUtilitiesService)args[0]);
            ICustomEntitiesServices customEntitiesServices = (ICustomEntitiesServices)args[1];
            IModDataService modDataService = (IModDataService)args[2];
            IGameEventsService eventsService = (IGameEventsService)args[3];
            IAutoMapperService autoMapperService = (IAutoMapperService)args[4];
            IGameEnvironmentService gameEnvironmentService= (IGameEnvironmentService)args[5];

            contentPackLoader = new ContentPackLoader(logger, utilities, customEntitiesServices, modDataService, autoMapperService, gameEnvironmentService);
            contentPacks = utilities.ModHelperService.modHelper.ContentPacks.GetOwned().ToList();
            eventsService.AddSubscription(new GameLaunchedEventArgs(), LoadContent,500);
        }
        private void LoadContent(EventArgs e)
        {
            LoadPacks();
            LoadMaps();
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
