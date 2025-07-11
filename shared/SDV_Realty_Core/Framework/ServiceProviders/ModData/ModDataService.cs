using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using System;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using SDV_Realty_Core.Framework.Patches.Buildings;

namespace SDV_Realty_Core.Framework.ServiceProviders.ModData
{
    internal class ModDataService : IModDataService
    {
        private IModHelperService modHelperService;
        private ICustomEventsService customEventsService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IModHelperService),typeof(IGameEventsService),
            typeof(ICustomEventsService)
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
            modHelperService = (IModHelperService)args[0];
            IGameEventsService gameEventsService = (IGameEventsService)args[1];
            customEventsService = (ICustomEventsService)args[2];

            assetRequests = gameEventsService.assetRequests;
            gameEventsService.AddSubscription(new ReturnedToTitleEventArgs(), HandleReturnToTitle);
            gameEventsService.AddSubscription(new SaveLoadedEventArgs(), HandleSaveLoaded);
        }
        /// <summary>
        /// Save the ModState values to the game save
        /// </summary>
        public override void SaveModState()
        {
            modHelperService.modHelper.Data.WriteSaveData("prism99.sdr.modState", ModState);
        }
        /// <summary>
        /// Called once a save has been loaded to read mod state values from the save
        /// </summary>
        /// <param name="e"></param>
        private void HandleSaveLoaded(EventArgs e)
        {
            if (Game1.IsMasterGame)
            {
                ModState = modHelperService.modHelper.Data.ReadSaveData<ModStateValues>("prism99.sdr.modState");
                if (ModState == null)
                {
                    ModState = new ModStateValues();
                }
                else
                {
                    if (ModState.JunimoLetterSent)
                    {
                        CustomMail[FEJuminoHut.unionLetterId] = FEJuminoHut.GenerateJunimoFeeLetter();
                        customEventsService.TriggerCustomEvent("InvalidateCache", new object[] { "Data/Mail" });
                    }
                }
            }
        }
        /// <summary>
        /// Clears out required data when the player returns to the Game Title screen
        /// </summary>
        /// <param name="e"></param>
        private void HandleReturnToTitle(EventArgs e)
        {
            SubLocations.Clear();
            MapGrid.Clear();
        }
    }
}
