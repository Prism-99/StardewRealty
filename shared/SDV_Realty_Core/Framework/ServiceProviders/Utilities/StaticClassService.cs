using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;

namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class StaticClassService : IStaticClassService
    {
        public override Type ServiceType => typeof(IStaticClassService);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService),typeof(IConfigService),
            typeof(IPatchingService),typeof(IModDataService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(  ILoggerService logger, object[] args)
        {
            this.logger = logger;
            IModHelper modHelper = ((IModHelperService)args[0]).modHelper;
            IConfigService configService = (IConfigService)args[1];
            IPatchingService patches = (IPatchingService)args[2];
            IModDataService modDataService = (IModDataService)args[3];
            //
            //  static inits
            //
            // not used
            //FEUtility.Initialize((SDVLogger)logger.CustomLogger, modHelper, modDataService);
            // does not appear to be used any more
            //IOHelpers.Initialize(modHelper);
            // moved to IModDataKeysService
            //FEModDataKeys.Initialize(modHelper.ModRegistry.ModID);
            // moved to IMapLoaderService
            //MapLoader.Initialize(modHelper, logger);
            // moved to IAutoMapperService
            //AutoMapper.Initialize((SDVLogger)logger.CustomLogger);
            //CGCM_Integration.Initialize(modHelper, manifest, configService.config, (SDVLogger)logger.CustomLogger);
            // moved to IMultiplayerService
            //SDRMultiplayer.Initialize(modHelper, logger);
            //seems unused
            //SDVEnvironment.Initialize(modHelper, (SDVLogger)logger.CustomLogger);
            // moved to IFileManagerService
            //FileManager.Initialize(logger, patches.patches);
            //SaveManager.Initialize(logger, modHelper);
            // moved to IExpansionManagerService
            //ExpansionManager.Initialize(logger, modHelper);
            // moved to IForSaleService
            //FEForSale.Initialize(logger, modHelper);
            //FEInputHandler.Initialize(logger, configService.config, patches.patches);
            // moved to ICropGracePeriodService
            //CropGracePeriod.Initialize(patches.patches, (SDVLogger)logger.CustomLogger, configService.config);
            // moved to IThreadSafeLoaderService
            //StardewThreadSafeLoader.Initialize(modHelper);
            // moved  to IChatboxCommandService
            //SDRChatboxCommaands.Initialize(modHelper, logger);
            // moved to ISeasonUtilsService
            //SeasonalUtils.Initialize(configService.config, logger);
            // not used
            //FEBFAV.Initialize((SDVLogger)logger.CustomLogger);
            // moved to IValleyStatsService
            //ValleyStats.Initialize(configService.config);
            // moved to IJunimoHarvester service
            //FEJuminoHarvester.Initialize(logger, configService.config);
            // moved to IJunimoHutService
            //FEJuminoHut.Initialize((SDVLogger)logger.CustomLogger, configService.config);
            // not used
            //FEBuilding.Initialize((SDVLogger)logger.CustomLogger);
        }
    }
}
