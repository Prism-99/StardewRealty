using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using System;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.Objects;
using StardewModdingAPI.Events;

namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class MulitplayerService : IMultiplayerService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IExitsService),typeof(ILandManager),
            typeof(IUtilitiesService),typeof(IExpansionManager)
            
        };
        public override List<string> CustomServiceEventSubscripitions => new List<string>
        {
            "SendFarmHandPurchase"
        };
        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger=logger;
            IExitsService exitsService = (IExitsService)args[0];
            ILandManager landManager = (ILandManager)args[1];
            IUtilitiesService utilitiesService= (IUtilitiesService)args[2];
            IExpansionManager expansionManager = (IExpansionManager)args[3];
 
            Multiplayer = new SDRMultiplayer();
            Multiplayer.Initialize((SDVLogger)logger.CustomLogger, utilitiesService.ModHelperService.modHelper,   expansionManager, landManager, exitsService);

            utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), Multiplayer.AddHooks);
            RegisterEventHandler("SendFarmHandPurchase", SendFarmHandPurchase);
        }
        private void SendFarmHandPurchase(object[] args)
        {

        }
    }
}
