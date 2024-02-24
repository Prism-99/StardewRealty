using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModHelpers;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.Utilities
{
    internal class ThreadSafeLoaderService : IThreadSafeLoaderService
    {
        public override Type[] InitArgs => new Type[]
        {
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
            IModHelperService modHelperService= (IModHelperService)args[0];

            StardewThreadSafeLoader.Initialize(modHelperService.modHelper);

        }
    }
}
