using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;



namespace SDV_Realty_Core.Framework.ServiceProviders.Configuration
{
    internal class ConfigService : IConfigService
    {
        private IModHelperService _modHelperService;
        private IModDataService _modDataService;
        public override Type ServiceType => typeof(IConfigService);

        public override Type[] InitArgs => new Type[] 
        {
            typeof(IModHelperService),typeof(IModDataService) 
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
            _modDataService = (IModDataService)args[1];

            //config=_modDataService.Config;

            LoadConfig();
        }
        public override void SaveConfig()
        {
            _modHelperService.WriteConfig(_modDataService.Config);
        }
        public override void LoadConfig()
        {
            _modDataService.Config = _modHelperService.ReadConfig<FEConfig>();            
        }
    }
}
