using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;
using SDV_Realty_Core.Framework.ServiceProviders.Game;
using StardewModdingAPI.Events;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class TranslationService : ITranslationService
    {
        private IUtilitiesService utilitiesService;
        public override Type ServiceType => typeof(ITranslationService);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(ITranslationService))
                return this;
            else
                return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            utilitiesService = (IUtilitiesService)args[0];

            utilitiesService.GameEventsService.AddSubscription(new GameLaunchedEventArgs(), HandleGameLaunched);
        }

        private void HandleGameLaunched(EventArgs e)
        {
            I18n.Init(utilitiesService.ModHelperService.modHelper.Translation);

        }
    }
}
