using xTile.Dimensions;
using System;
using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;

namespace SDV_Realty_Core.Framework.Objects
{
    internal class FEInputHandler
    {
        //
        //  version 1.6
        //
        private static ILoggerService logger;
        private static FEConfig Config;
        private static bool controlDown = false;
        private static bool zKeyDown = false;
        private static IForSaleMenuService forSaleMenuService;
        private static IMineCartMenuService _mineCartMenuService;
        private static ILandManager _landManager;
        //public static Vector2 lastPlayerPoint = Vector2.Zero;
        public FEInputHandler(ILoggerService olog, IUtilitiesService utilitiesService, IForSaleMenuService forSaleMenuServ, IMineCartMenuService mineCartMenuService, ILandManager landManager)
        {
            logger = olog;
            Config = utilitiesService.ConfigService.config;
            forSaleMenuService = forSaleMenuServ;
            _mineCartMenuService = mineCartMenuService;
            _landManager = landManager;

            utilitiesService.PatchingService.patches.AddPatch(false, typeof(GameLocation), "performAction",
             new Type[] { typeof(string), typeof(Farmer), typeof(Location) },
             typeof(FEInputHandler), nameof(performAction),
             "To intercept player action to check for the for sale sign opening.",
             "GameLocation");
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

        public static void performAction(string fullActionString, Farmer who, Location tileLocation, ref bool __result)
        {
            //
            //  handle custom actions
            //
            if (!__result)
            {
                logger.Log("Checking action: " + fullActionString, LogLevel.Debug);

                switch (fullActionString)
                {
                    case "prism99.sdr.WaterMe":
                        Game1.player.currentLocation.WaterMe();
                        Game1.addHUDMessage(new HUDMessage("Location watered") { noIcon = true, timeLeft = 1500 });
                        __result = true;
                        break;
                    case "prism99.sdr.ForSale":
                        //
                        //  activate ForSaleMenu
                        //
                        if (_landManager.LandForSale.Count == 0)
                        {
                            Game1.addHUDMessage(new HUDMessage("There currently is no land for sale.") { noIcon = true, timeLeft = 1500 });
                        }
                        else
                        {
                            Game1.activeClickableMenu = forSaleMenuService.GetMenu();//  new FEForSaleMenu(logger);
                        }
                        __result = true;
                        break;
                    case "SDR_MineCart":
                    case "prism99.sdr.MineCart":
                        _mineCartMenuService.MineCartMenu.EnableAllItems();
                        _mineCartMenuService.MineCartMenu.DisableItem(Game1.currentLocation.Name);
                        Game1.activeClickableMenu = _mineCartMenuService.MineCartMenu;
                        __result = true;
                        break;
                    //
                    //  keeping in case want to add
                    //  popup message about moving to
                    //  carpenter

                    case "GoBuilder":
                        //
                        //  activate the building contruction menu
                        //
                        logger.Log("Go builder", LogLevel.Info);
                        Game1.addHUDMessage(new HUDMessage(I18n.GotoCarpenter()) { noIcon = true });
                        //if (FEFramework.IsThereAnyConstruction())
                        //{
                        //    Game1.addHUDMessage(new HUDMessage(I18n.RobinBusy()) { noIcon = true });
                        //}
                        //else
                        //{
                        //    Game1.activeClickableMenu = new FECarpenterMenu(CustomBuildingManager.FarmBlueprints.ToArray(), CustomBuildingManager.ExpansionBlueprints.ToArray()); // copy blueprint lists to avoid saving temporary blueprints
                        //}
                        __result = true;
                        break;
                    case "GoAnimals":
                        //
                        //  activate the animal purchase menu
                        //
                        logger.Log("Go animals", LogLevel.Info);
                        Game1.addHUDMessage(new HUDMessage(I18n.GotoRanch()) { noIcon = true });
                        //Game1.activeClickableMenu = new FEPurchaseAnimalsMenu();
                        __result = true;
                        break;

                }
            }
        }
    }
}
