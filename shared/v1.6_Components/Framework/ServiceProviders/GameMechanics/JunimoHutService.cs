using SDV_Realty_Core.Framework.ServiceInterfaces.Configuration;
using SDV_Realty_Core.Framework.Patches.Buildings;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using StardewValley.Buildings;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;


namespace SDV_Realty_Core.Framework.ServiceProviders.GameMechanics
{
    internal class JunimoHutService : IJunimoHutService
    {
        private FEJuminoHut junimoHut;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IPatchingService),
            typeof(ISeasonUtilsService),typeof(IModDataService)
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
            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            //if (configService.config.EnablePremiumJunimos)
            //{
            IPatchingService patchingService = (IPatchingService)args[1];
            ISeasonUtilsService seasonUtilsService = (ISeasonUtilsService)args[2];
            IModDataService modDataService = (IModDataService)args[3];

            junimoHut = new FEJuminoHut(logger, modDataService, utilitiesService, seasonUtilsService);

            patchingService.patches.AddPatch(true, typeof(JunimoHut), "updateWhenFarmNotCurrentLocation",
    null, typeof(FEJuminoHut), nameof(FEJuminoHut.updateWhenFarmNotCurrentLocation),
    "Allows sending Junimos out in the rain and winter.",
    "JunimoHut");
            //}
        }
    }
}
