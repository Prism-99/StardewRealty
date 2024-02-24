using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using System;
using SDV_Realty_Core.Framework.Patches;


namespace SDV_Realty_Core.Framework.ServiceProviders.Patches
{
    internal class GameLocationPatchService : IGameLocationPatchService
    {
        public override Type[] InitArgs => new Type[]
        {

        };
        private GameLocationPatches gameLocationPatches;
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            gameLocationPatches = new GameLocationPatches(logger);
        }
    }
}
