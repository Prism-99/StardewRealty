using Prism99_Core.PatchingFramework;
using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;

namespace SDV_Realty_Core.Framework.ServiceProviders.Patches
{
    internal class PatchingService : IPatchingService
    {
        public override Type[] InitArgs => new Type[] 
        {
            typeof(IManifestService),typeof(IGameEventsService) 
        };
        public override Type[] Dependants => new Type[]
        {
            typeof(IGameLocationPatchService)
#if DEBUG
            //,typeof(IDebugPatchService)
#endif
        };
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(IPatchingService))
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger=logger;
            IManifestService manifest = (IManifestService)args[0];
            IGameEventsService events = (IGameEventsService)args[1];

            patches=new GamePatches();
            patches.Initialize(manifest.manifest.UniqueID, (SDVLogger)logger.CustomLogger);

            events.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, ApplyPatches);
        }
        internal override void ApplyPatches()
        {
            patches.ApplyPatches(null);

        }
    }
}
