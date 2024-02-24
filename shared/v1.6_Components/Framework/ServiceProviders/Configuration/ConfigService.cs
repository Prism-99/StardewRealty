using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.Configuration
{
    internal class ConfigService : IConfigService
    {
        private IModHelperService _modHelperService;
        public override Type ServiceType => typeof(IConfigService);

        public override Type[] InitArgs => new Type[] 
        {
            typeof(IModHelperService) ,typeof(ICustomEventsService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(IConfigService))
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger=logger;
            _modHelperService = (IModHelperService)args[0];

            LoadConfig();
        }
        public override void SaveConfig()
        {
            _modHelperService.WriteConfig(config);
        }
        public override void LoadConfig()
        {
            config = _modHelperService.ReadConfig<FEConfig>();
        }
    }
}
