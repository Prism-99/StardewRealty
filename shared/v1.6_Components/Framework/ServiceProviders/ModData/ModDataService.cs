using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class ModDataService : IModDataService
    {
        public override Type[] InitArgs => new Type[]
        {

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
         }
    }
}
