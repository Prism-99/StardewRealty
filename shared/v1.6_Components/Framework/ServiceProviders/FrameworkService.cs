using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.AssetUtils;
using Prism99_Core.PatchingFramework;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class FrameworkService : IFrameworkService
    {
        public override Type[] InitArgs => new Type[]
       {
            typeof(IUtilitiesService),typeof(IContentManagerService),
            typeof(IContentPackService)
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
            IUtilitiesService utilities = (IUtilitiesService)args[0];

            IMonitor monitor = utilities.MonitorService.monitor;
            IModHelper helper = utilities.ModHelperService.modHelper;
            FEConfig config = utilities.ConfigService.config;
            GamePatches patches = utilities.PatchingService.patches;

            SDRContentManager conMan = ((IContentManagerService)args[1]).contentManager;
            ContentPackLoader loader = ((IContentPackService)args[2]).contentPackLoader;
        }
    }
}
