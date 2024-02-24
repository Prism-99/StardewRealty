using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System;
using SDV_Realty_Core.Framework.Menus;
using StardewModdingAPI.Events;

namespace SDV_Realty_Core.Framework.ServiceProviders.GUI
{
    internal class MineCartMenuService : IMineCartMenuService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IGameEventsService)
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
            IGameEventsService eventsService = (IGameEventsService)args[0];

            MineCartMenu = new FEMineCartMenu();

            eventsService.AddSubscription(new ReturnedToTitleEventArgs(), ResetForNewGame);
        }
        private void ResetForNewGame(EventArgs e)
        {
            MineCartMenu.ClearCustomDestinations();
        }
    }
}
