using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;


namespace SDV_Realty_Core.Framework.Objects
{
    internal class FEInputHandler
    {
        //
        //  version 1.6
        //
        private static ILoggerService _logger;
        private static IMineCartMenuService _mineCartMenuService;
        private static IModDataService _modDataService;
        private static IUtilitiesService _UtilitiesService;
        private ILandManager landManager;

        public FEInputHandler(ILoggerService logger, IUtilitiesService UtilitiesService, ILandManager landManager, IUtilitiesService utilitiesService,  IMineCartMenuService mineCartMenuService, IModDataService modDataService)
        {
            _logger = logger;
            _mineCartMenuService = mineCartMenuService;
            _modDataService = modDataService;
            _UtilitiesService = UtilitiesService;
            this.landManager = landManager;
            //utilitiesService.PatchingService.patches.AddPatch(false, typeof(GameLocation), "performAction",
            // new Type[] { typeof(string), typeof(Farmer), typeof(Location) },
            // typeof(FEInputHandler), nameof(performAction),
            // "To intercept player action to check for the for sale sign opening.",
            // "GameLocation");

            GameLocation.RegisterTileAction(ModKeyStrings.Action_WaterMe, HandleWaterMe);
            GameLocation.RegisterTileAction(ModKeyStrings.Action_ForSale, HandleForSale);
            GameLocation.RegisterTileAction(ModKeyStrings.Action_MineCartOld, HandleMineCart);
            GameLocation.RegisterTileAction(ModKeyStrings.Action_MineCart, HandleMineCart);
        }
        private bool HandleMineCart(GameLocation location, string[] args, Farmer who, Point pos)
        {
            _mineCartMenuService.MineCartMenu.EnableAllItems();
            _mineCartMenuService.MineCartMenu.DisableItem(Game1.currentLocation.Name);
            Game1.activeClickableMenu = _mineCartMenuService.MineCartMenu;

            return true;
        }
        private bool HandleForSale(GameLocation location, string[] args, Farmer who, Point pos)
        {
            landManager.PopBuyingMenu();
           
            return true;
        }
        private bool HandleWaterMe(GameLocation location, string[] args, Farmer who, Point pos)
        {
            location.WaterMe();
            _UtilitiesService.PopMessage(I18n.LocationWatered());

            return true;
        }

        //public static void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        //{
        //    //foreach(var binding in Config.WarpRoomKey.Keybinds)
        //    //{
        //    //    foreach (var key in binding.Buttons)
        //    //    {
        //    //        if (e.Button.HasFlag(key))
        //    //        {

        //    //        }
        //    //    }
        //    //}
        //        // handled in updating ticking now
        //    //if (e.Button.HasFlag(SButton.Z)) zKeyDown = false;
        //    //if (e.Button.HasFlag(SButton.LeftControl)) controlDown = false;
        //}

        //public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        //{

        //    if (Game1.hasLoadedGame && !Game1.eventUp )
        //    {
        //        foreach (var binding in Config.WarpRoomKey.Keybinds)
        //        {
        //            foreach (var key in binding.Buttons)
        //            {
        //                if (!e.Button.HasFlag(key))
        //                {
        //                    return;
        //                }
        //            }
        //        }
        //        WarproomManager.lastPlayerPoint = Game1.player.Tile;
        //        Game1.warpFarmer(WarproomManager.WarpRoomLoacationName, (int)WarproomManager.WarpRoomEntrancePoint.X, (int)WarproomManager.WarpRoomEntrancePoint.Y, 1);
        //    }


        //    // handled in updating ticking now
        //    //if (e.Button.HasFlag(SButton.Z)) zKeyDown = true;
        //    //if (e.Button.HasFlag(SButton.LeftControl)) controlDown = true;

        //    //if (Game1.hasLoadedGame && !Game1.eventUp && zKeyDown && controlDown)
        //    //{
        //    //    WarproomManager.lastPlayerPoint = Game1.player.Tile;
        //    //    Game1.warpFarmer(WarproomManager.WarpRoomLoacationName, (int)WarproomManager.WarpRoomEntrancePoint.X, (int)WarproomManager.WarpRoomEntrancePoint.Y, 1);
        //    //}
        //}

//        public static void performAction(string fullActionString, Farmer who, Location tileLocation, ref bool __result)
//        {
//            //
//            //  handle custom actions
//            //
//            if (!__result)
//            {
//#if DEBUG
//                _logger.Log("Checking action: " + fullActionString, LogLevel.Debug);
//#endif

//                switch (fullActionString)
//                {
//                    //case "prism99.sdr.WaterMe":
//                    //    Game1.player.currentLocation.WaterMe();
//                    //    Game1.addHUDMessage(new HUDMessage("Location watered") { noIcon = true, timeLeft = 1500 });
//                    //    __result = true;
//                    //    break;
//                    //case "prism99.sdr.ForSale":
//                    //    //
//                    //    //  activate ForSaleMenu
//                    //    //
//                    //    if (_landManager.LandForSale.Count == 0)
//                    //    {
//                    //        Game1.addHUDMessage(new HUDMessage("There currently is no land for sale.") { noIcon = true, timeLeft = 1500 });
//                    //    }
//                    //    else
//                    //    {
//                    //        Game1.activeClickableMenu = forSaleMenuService.GetMenu();//  new FEForSaleMenu(logger);
//                    //    }
//                    //    __result = true;
//                    //    break;
//                    //case "SDR_MineCart":
//                    //case "prism99.sdr.MineCart":
//                    //    _mineCartMenuService.MineCartMenu.EnableAllItems();
//                    //    _mineCartMenuService.MineCartMenu.DisableItem(Game1.currentLocation.Name);
//                    //    Game1.activeClickableMenu = _mineCartMenuService.MineCartMenu;
//                    //    __result = true;
//                    //    break;
//                        //
//                        //  keeping in case want to add
//                        //  popup message about moving to
//                        //  carpenter
//#if !v16
//                case "GoBuilder":
//                    //
//                    //  activate the building contruction menu
//                    //
//                    logger.Log("Go builder", LogLevel.Info);
//                    if (FEFramework.IsThereAnyConstruction())
//                    {
//                        Game1.addHUDMessage(new HUDMessage(I18n.RobinBusy()) { noIcon = true });
//                    }
//                    else
//                    {
//                        Game1.activeClickableMenu = new FECarpenterMenu(CustomBuildingManager.FarmBlueprints.ToArray(), CustomBuildingManager.ExpansionBlueprints.ToArray()); // copy blueprint lists to avoid saving temporary blueprints
//                    }
//                    __result = true;
//                    break;
//                case "GoAnimals":
//                    //
//                    //  activate the animal purchase menu
//                    //
//                    logger.Log("Go animals", LogLevel.Info);
//                    Game1.activeClickableMenu = new FEPurchaseAnimalsMenu();
//                    __result = true;
//                    break;
//#endif
//                }
//            }
//        }
    }
}
