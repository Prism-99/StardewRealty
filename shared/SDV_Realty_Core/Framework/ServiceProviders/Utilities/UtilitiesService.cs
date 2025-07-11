using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceProviders.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;
using System.Linq;

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
        private IPlayerComms _playerComms;
        private IModDataService _modDataService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService),typeof(IManifestService),
            typeof(IMonitorService),typeof(IConfigService),
            typeof(IPatchingService),typeof(IGameEventsService),
            typeof(IMapLoaderService),typeof(IMapUtilities),
            typeof(IGameEnvironmentService),typeof(ICustomEventsService),
            typeof(IPlayerComms),typeof(IModDataService)
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
        public override IModDataService ModDataService => _modDataService;
        public override IPlayerComms PlayerComms => _playerComms;

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
            _manifestService = (ManifestService)args[1];
            _monitorService = (IMonitorService)args[2];
            _configService = (IConfigService)args[3];
            _patchingService = (IPatchingService)args[4];
            _eventsService = (IGameEventsService)args[5];
            _mapLoaderService = (IMapLoaderService)args[6];
            _mapUtilities = (IMapUtilities)args[7];
            _gameEnvironmentService = (IGameEnvironmentService)args[8];
            _customEventsService = (ICustomEventsService)args[9];
            _playerComms = (IPlayerComms)args[10];
            _modDataService = (IModDataService)args[11];

            _customEventsService.AddCustomSubscription("InvalidateCache", InvalidateCache);
            _eventsService.AddSubscription(new SaveLoadedEventArgs(), LogDebugInfo, 1000);
        }
        private void InvalidateCache(object[] args)
        {
            if (args.Length == 1)
                InvalidateCache(args[0].ToString());
        }
        /// <summary>
        /// Log various SDR configuration values to help in
        /// error debugging
        /// </summary>
        /// <param name="args"></param>
        private void LogDebugInfo(EventArgs args)
        {
            LogLevel loggingLevel = LogLevel.Trace;
            logger.Log("=====================", loggingLevel,false);
            logger.Log("=  SDR Parameters   =", loggingLevel, false);
            logger.Log("=====================", loggingLevel, false);
            logger.Log($"Use Global Condition? {(_modDataService.Config.useGlobalCondition ? "Y" : "N")}", loggingLevel, false);
            if (_modDataService.Config.useGlobalCondition)
                logger.Log($"  Global Condition: '{_modDataService.Config.globalCondition}'", loggingLevel, false);
            logger.Log($"Use Global Pice? {(_modDataService.Config.useGlobalPrice ? "Y" : "N")}", loggingLevel, false);
            if (_modDataService.Config.useGlobalPrice)
                logger.Log($"    Global Pice: {_modDataService.Config.globalPrice}", loggingLevel, false);
            logger.Log($"Number of Expansion packs: {_modDataService.validContents.Count}", loggingLevel, false);
            logger.Log($"Total # of active Expansions: {_modDataService.farmExpansions.Where(p => p.Value.Active).Count()}", loggingLevel, false);
            logger.Log($"Number of Expansions in grid: {_modDataService.MapGrid.Count}", loggingLevel, false);
            logger.Log($"Number of Expansions for sale: {_modDataService.LandForSale.Count}", loggingLevel, false);
            int maxName = _modDataService.validContents.Select(p => p.Key.Length).Max();
            logger.Log($"__________________{new string('_', maxName + 1)}", loggingLevel, false);
            logger.Log($"|Active |For Sale|Expansion Key{new string(' ', maxName - 13)}|", loggingLevel, false);
            logger.Log($"=================={new string('=', maxName + 1)}", loggingLevel, false);
            foreach (var expansion in _modDataService.validContents.OrderBy(p => p.Key))
            {
                logger.Log($"|  {(expansion.Value.Added ? "Y" : "N")}    |   {(_modDataService.LandForSale.Contains(expansion.Key) ? "Y" : "N")}    |{expansion.Key}{new string(' ', maxName - expansion.Key.Length)}|", loggingLevel, false);
            }
            logger.Log(new string('-', maxName + 19), loggingLevel, false);
        }
    }
}
