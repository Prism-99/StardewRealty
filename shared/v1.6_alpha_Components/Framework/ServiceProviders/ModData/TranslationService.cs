using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class TranslationService : ITranslationService
    {
        public override Type ServiceType => typeof(ITranslationService);

        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService)
        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(ITranslationService))
                return this;
            else
                return null;
        }

        internal override void Initialize(  ILoggerService logger, object[] args)
        {
            this.logger=logger;
            IModHelperService modHelperService = (IModHelperService)args[0];

            I18n.Init(modHelperService.modHelper.Translation);
        }
    }
}
