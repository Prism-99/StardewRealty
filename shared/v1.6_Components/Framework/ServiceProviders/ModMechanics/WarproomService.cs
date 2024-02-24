using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class WarproomService : IWarproomService
    {

        public override Type[] InitArgs => new Type[]
        {
            typeof(IContentManagerService),typeof(IUtilitiesService),
            typeof(IMultiplayerService),typeof(IExpansionManager),
            typeof(IContentPackService)

        };
        public override List<string> CustomServiceEventSubscripitions => new List<string>
        {
            "AddCaveEntrance"
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
            IContentManagerService contentManager = (IContentManagerService)args[0];
            IUtilitiesService utilitiesService = (IUtilitiesService)args[1];
            IMultiplayerService multiplayerService = (IMultiplayerService)args[2];
            IExpansionManager expansionManager = (IExpansionManager)args[3];
            IContentPackService contentPackService = (IContentPackService)args[4];

            warproomManager = new WarproomManager(logger, contentManager.contentManager, utilitiesService, multiplayerService, expansionManager, contentPackService);
            RegisterEventHandler("AddCaveEntrance", AddCaveEntrance);
        }
        private void AddCaveEntrance(object[] args)
        {
            warproomManager.AddCaveEntrance(args[0].ToString(), (Tuple<string, EntranceDetails>)args[1]);
        }
    }
}
