using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using System;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using StardewModdingAPI.Utilities;

namespace SDV_Realty_Core.Framework.ServiceProviders
{
    internal class InputService : IInputService
    {
        private IUtilitiesService utilitiesService;
        private List<KeybindList> pressedKeys = new();
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IForSaleMenuService),
            typeof(IMineCartMenuService),typeof(IModDataService),
            typeof(ILandManager)
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

            utilitiesService = (IUtilitiesService)args[0];
            IForSaleMenuService forSaleMenuService = (IForSaleMenuService)args[1];
            IMineCartMenuService mineCartMenuService = (IMineCartMenuService)args[2];
            IModDataService modDataService = (IModDataService)args[3];
            ILandManager landManager = (ILandManager)args[4];
            utilitiesService.GameEventsService.AddSubscription(new UpdateTickingEventArgs(), GameLoop_UpdateTicking);

            inputHandler = new FEInputHandler(logger, utilitiesService,landManager, utilitiesService, forSaleMenuService, mineCartMenuService, modDataService);
        }
        public override void AddKeyBind(KeybindList key, Action<KeybindList> bind)
        {
            actionHandlers.Add(key, bind);
        }
        private void GameLoop_UpdateTicking(EventArgs e)
        {
            if (Game1.hasLoadedGame)
            {
                foreach (var keyPair in actionHandlers)
                {
                    //
                    //  add key debouncing
                    //
                    if (!pressedKeys.Contains(keyPair.Key))
                    {
                        if (keyPair.Key.JustPressed())
                        {
                            pressedKeys.Add(keyPair.Key);
                            keyPair.Value.Invoke(keyPair.Key);
                        }
                    }
                    else if (!keyPair.Key.IsDown()) 
                    { 
                        pressedKeys.Remove(keyPair.Key);
                    }

                }
                //utilitiesService.CustomEventsService.TriggerModEvent(ServiceInterfaces.Events.ICustomEventsService.ModEvents.ConfigChanged, new object[] { });
            }
        }
    }
}
