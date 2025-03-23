using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceProviders.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using System.Diagnostics;

namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class UtilitiesService : IUtilitiesService
    {
        private IModHelperService _modHelperService;
        private IManifestService _manifestService;
        private IMonitorService _monitorService;
        private IConfigService _configService;
        private IPatchingService _patchingService;
        private IGameEventsService _eventsService;
        private IMapLoaderService _mapLoaderService;
        private IMapUtilities _mapUtilities;
        private IGameEnvironmentService _gameEnvironmentService;
        private ICustomEventsService _customEventsService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService),typeof(IManifestService),
            typeof(IMonitorService),typeof(IConfigService),
            typeof(IPatchingService),typeof(IGameEventsService),
            typeof(IMapLoaderService),typeof(IMapUtilities),
            typeof(IGameEnvironmentService),typeof(ICustomEventsService)
            };

        public override IModHelperService ModHelperService => _modHelperService;

        public override IManifestService ManifestService => _manifestService;

        public override IMonitorService MonitorService => _monitorService;

        public override IConfigService ConfigService => _configService;

        public override IPatchingService PatchingService => _patchingService;

        public override IGameEventsService GameEventsService => _eventsService;
        public override IMapLoaderService MapLoaderService => _mapLoaderService;
        public override IMapUtilities MapUtilities => _mapUtilities;
        public override IGameEnvironmentService GameEnvironment => _gameEnvironmentService;
        public override ICustomEventsService CustomEventsService => _customEventsService;
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            _modHelperService = (IModHelperService)args[0];
            _manifestService= (ManifestService)args[1];
            _monitorService = (IMonitorService)args[2];
            _configService = (IConfigService)args[3];
            _patchingService = (IPatchingService)args[4];
            _eventsService = (IGameEventsService)args[5];
            _mapLoaderService = (IMapLoaderService)args[6];
            _mapUtilities= (IMapUtilities)args[7];
            _gameEnvironmentService= (IGameEnvironmentService)args[8];
            _customEventsService = (ICustomEventsService)args[9];
        }
      
    }
}
