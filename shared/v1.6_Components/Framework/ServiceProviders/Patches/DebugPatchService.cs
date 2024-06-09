using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.Patches;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;


namespace SDV_Realty_Core.Framework.ServiceProviders.Patches
{
    internal class DebugPatchService : IDebugPatchService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService),typeof(IPatchingService),
            typeof(IModHelperService)
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
            IGameEventsService eventsService = (IGameEventsService)args[0];
            IPatchingService patchingService= (IPatchingService)args[1];
            IModHelperService modHelperService= (IModHelperService)args[2];

            DebugPatches.Initialize(logger, eventsService, patchingService, modHelperService);
        }
    }
}
