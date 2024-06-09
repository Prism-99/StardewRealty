using SDV_Realty_Core.Framework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.ServiceProviders.GUI
{
    internal class WorldMapMenuService : IWordMapMenuService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService)
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

            utilitiesService.GameEventsService.AddSubscription("MenuChangedEventArgs", HandleMenuChange);
        }

        private void HandleMenuChange(EventArgs e)
        {
            MenuChangedEventArgs menuChangedEventArgs = (MenuChangedEventArgs)e;
            GameMenu gamemenu = menuChangedEventArgs.NewMenu as GameMenu;
            if (gamemenu != null)
            {
                if (gamemenu.currentTab == GameMenu.mapTab)
                {
                    //gamemenu.SetChildMenu(new SDRWorldMapMenu(1, 1));
                }
            }
        }
    }
}
