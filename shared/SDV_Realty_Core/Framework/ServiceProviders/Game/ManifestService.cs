using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.Game
{
    internal class ManifestService : IManifestService
    {
        public override Type ServiceType => typeof(IManifestService);

        public override Type[] InitArgs => null;

        public ManifestService(IManifest manifest)
        {
            this.manifest = manifest;
        }
        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
        }

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(IManifestService))
                return this;

            return null;
        }
    }
}
