using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.Game
{
    internal class ModHelperService : IModHelperService
    {
        private ICustomEventsService customEventsService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(ICustomEventsService)
        };
       
        public override string DirectoryPath => modHelper.DirectoryPath;

        public override ITranslationHelper Translation => modHelper.Translation;
        public override IModRegistry ModRegistry => modHelper.ModRegistry;
        public override IMultiplayerHelper Multiplayer => modHelper.Multiplayer;
        public override void WriteConfig<TConfig>(TConfig config)
        {
            modHelper.WriteConfig(config);
            customEventsService.TriggerCustomEvent("ConfigChanged",null);
        }
        public override TConfig ReadConfig<TConfig>()
        {
            return modHelper.ReadConfig<TConfig>();
        }
        public ModHelperService(IModHelper modHelper)
        {
            this.modHelper = modHelper;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            customEventsService = (ICustomEventsService)args[0];
        }

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(IModHelperService))
                return this;

            return null;

        }
    }
}
