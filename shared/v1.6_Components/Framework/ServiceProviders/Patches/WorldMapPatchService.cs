using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewValley.Menus;
using System;
using xTile.Dimensions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;
using StardewModdingAPI.Events;
using System.IO;
using SDV_Realty_Core.Framework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System.Linq;

namespace SDV_Realty_Core.Framework.ServiceProviders.Patches
{
    internal class WorldMapPatchService : IWorldMapPatch
    {
        private static Point boxLocation = new Point(260, 320);
        private readonly static Size boxSize = new Size(210, 210);
        private readonly static Point standardBoxOffset = new Point(140, 85);
        private static Point activeBoxOffset;
        private static Rectangle imageBox = new Rectangle(220, 320, 200, 230);
        private static Rectangle topBoundingBox;
        private static Rectangle bottomBoundingBox;
        private static Texture2D worldMapTexture;
        private static Texture2D areaTexture;
        private static Texture2D mapMenuTexture;
        private static IModDataService _modDataService;
        private static ILandManager _landManager;
        private static IUtilitiesService _utilitiesService;
        private static IPatchingService _patchingService;
        public override Type[] InitArgs => new Type[]
    {
            typeof(IPatchingService),typeof(IUtilitiesService),
            typeof(ILandManager),typeof(IModDataService)
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
            _patchingService = (IPatchingService)args[0];
            _utilitiesService = (IUtilitiesService)args[1];
            _landManager = (ILandManager)args[2];
            _modDataService = (IModDataService)args[3];

            worldMapTexture = _utilitiesService.ModHelperService.modHelper.ModContent.Load<Texture2D>(Path.Combine("data", "assets", "images", "world map image.png"));
            mapMenuTexture = _utilitiesService.ModHelperService.modHelper.ModContent.Load<Texture2D>(Path.Combine("data", "assets", "images", "meadow_menu_background.png"));
            activeBoxOffset = standardBoxOffset;

            _utilitiesService.GameEventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, HandleContentLoaded);

        }
        private void HandleContentLoaded()
        {
            //
            //  only patch world map if any expansion loaded
            //
            if (_utilitiesService.HaveExpansions)
            {
                //
                //  add patches required to draw map box on worldmap
                //
                _patchingService.patches.AddPatch(false, typeof(MapPage), "draw",
                new Type[] { typeof(SpriteBatch) }, typeof(WorldMapPatchService),
                "MapPageDraw_Postfix", "Adds SDR to world map", "WorldMap");

                _patchingService.patches.AddPatch(true, typeof(MapPage), "receiveLeftClick",
                null, typeof(WorldMapPatchService),
               "MapPageLeftClick_Prefix", "Adds SDR to world map", "WorldMap");

                _patchingService.patches.ApplyPatches("WorldMap");
                //
                //  add game event hooks
                //
                _utilitiesService.GameEventsService.AddSubscription("WindowResizedEventArgs", HandleWindowResize);
                 SetBoxLocation();
                //
                //  create highlight box texture
                //
                var colors = new Color[] { Color.PaleVioletRed };
                areaTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                areaTexture.SetData<Color>(colors);
            }
        }
        private void SetBoxLocation()
        {
            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(1200, 720);
            boxLocation = new Point((int)topLeft.X + activeBoxOffset.X, (int)topLeft.Y + activeBoxOffset.Y);
            topBoundingBox = new Rectangle((int)topLeft.X + 135, (int)topLeft.Y + 164, boxSize.Width, 80);
            bottomBoundingBox = new Rectangle((int)topLeft.X + 135, (int)topLeft.Y + 164 + 80, boxSize.Width - 80, boxSize.Height - 80);
            imageBox = new Rectangle((int)topLeft.X + 135, (int)topLeft.Y + 164, boxSize.Width, boxSize.Height);
        }
        internal void HandleWindowResize(EventArgs eventArgs)
        {
            SetBoxLocation();
        }
        private static bool IsMainMap(MapPage mapPage)
        {
            return mapPage.mapAreas.Where(p => p.Id == "Farm").Any();
            //return mapPage.scrollText.Contains(Game1.player.farmName.Value);
        }
        internal static bool MapPageLeftClick_Prefix(int x, int y, bool playSound, ref MapPage __instance)
        {
            if (IsMainMap(__instance) && (topBoundingBox.Contains(x, y) || bottomBoundingBox.Contains(x, y)))
            {               
                Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(mapMenuTexture.Width, mapMenuTexture.Height);
                SDRWorldMapMenu menu = new SDRWorldMapMenu((int)topLeft.X, (int)topLeft.Y, mapMenuTexture, _landManager,_modDataService, _utilitiesService);
                menu.Open(Game1.activeClickableMenu);
                return false;
            }
            return true;
        }
        internal static void MapPageDraw_Postfix(SpriteBatch b, ref MapPage __instance)
        {
            if (IsMainMap(__instance))
            {
                b.Draw(worldMapTexture, imageBox, new Rectangle(0, 0, worldMapTexture.Width, worldMapTexture.Height), Color.Wheat);

                //b.Draw(areaTexture, topBoundingBox, Color.Lavender);
                //b.Draw(areaTexture, bottomBoundingBox, Color.Lavender);
                Point mouseCoords = Game1.getMousePosition(ui_scale: true);
                if (topBoundingBox.Contains(mouseCoords.X, mouseCoords.Y) || bottomBoundingBox.Contains(mouseCoords.X, mouseCoords.Y))
                {
                    IClickableMenu.drawHoverText(b, "Stardew Meadows", Game1.smallFont);
                }
            }
        }
    }
}
