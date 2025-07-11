using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using System;
using System.Linq;

namespace SDV_Realty_Core.Framework.ServiceProviders.GUI
{
    internal class WorldMapMenuService : IWordMapMenuService
    {
        private static IModDataService _modDataService;
        private static IUtilitiesService _utilitiesService;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IModDataService),
            typeof(IPatchingService)
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
            _utilitiesService = (IUtilitiesService)args[0];
            _modDataService = (IModDataService)args[1];
            IPatchingService patchingService = (IPatchingService)args[2];

            patchingService.patches.AddPatch(true, typeof(MapPage), "receiveLeftClick",
     new Type[] { typeof(int), typeof(int), typeof(bool) }, GetType(), "receiveLeftClick", "WorldMap left click for warping around Stardew Meadows", "WorldMap");

            patchingService.ApplyPatches("WorldMap");
            _utilitiesService.GameEventsService.AddSubscription("MenuChangedEventArgs", HandleMenuChange);
        }
        /// <summary>
        /// Handle warping when Stardew Meadows map is up
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="playSound"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        private static bool receiveLeftClick(int x, int y, bool playSound, MapPage __instance)
        {
            if (!_modDataService.Config.UseMapWarps || __instance.scrollText != "Stardew Meadows")
                return true;

            foreach (ClickableComponent c in __instance.points.Values)
            {
                if (!c.containsPoint(x, y))
                {
                    continue;
                }
                string cleanLabel = c.label.Split('\n')[0];
                if (_modDataService.MiniMapGrid.Any(p => p.Value.DisplayName == cleanLabel))
                {
                    var entry = _modDataService.MiniMapGrid.Where(p => p.Value.DisplayName == cleanLabel).Select(p => p.Value).First();
                    DelayedAction.warpAfterDelay(entry.Warp.ToMap, new Point(entry.Warp.ToX, entry.Warp.ToY), 100);
                    Game1.activeClickableMenu.exitThisMenu();
                    return false;
                }
                else
                {
                    var expansion = _modDataService.validContents.Where(p => p.Value.DisplayName == cleanLabel);
                    if (expansion.Any() && expansion.First().Value.CaveEntrance != null)
                    {
                        DelayedAction.warpAfterDelay(expansion.First().Key, new Point(expansion.First().Value.CaveEntrance.WarpIn.X, expansion.First().Value.CaveEntrance.WarpIn.Y), 100);
                        Game1.activeClickableMenu.exitThisMenu();
                        return false;
                    }
                    else
                        return true;
                }
            }
            return true;
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
