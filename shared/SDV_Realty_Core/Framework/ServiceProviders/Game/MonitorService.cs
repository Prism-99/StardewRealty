using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.Game
{
    internal class MonitorService : IMonitorService
    {
        public override Type ServiceType => typeof(IMonitorService);

        public override Type[] InitArgs => null;

        public MonitorService(IMonitor monitor)
        {
            this.monitor = monitor;
        }
        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
        }

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;

        }
    }
}
