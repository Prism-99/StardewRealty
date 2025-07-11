using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Patches;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewValley.Menus;
using System;
using xTile.Dimensions;
using System.IO;
using SDV_Realty_Core.Framework.Menus;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System.Linq;
using StardewValley.GameData.WorldMaps;
using StardewValley.WorldMaps;

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
        private static Texture2D worldMapBGTexture;
        private static Texture2D areaTexture;
        private static Texture2D mapMenuTexture;
        private static IModDataService _modDataService;
        private static ILandManager _landManager;
        private static IUtilitiesService _utilitiesService;
        private static IPatchingService _patchingService;
        private static Texture2D borderTexture;
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

            worldMapBGTexture = _utilitiesService.ModHelperService.modHelper.ModContent.Load<Texture2D>(Path.Combine("data", "assets", "images", "world map image.png"));
            mapMenuTexture = _utilitiesService.ModHelperService.modHelper.ModContent.Load<Texture2D>(Path.Combine("data", "assets", "images", "meadow_menu_background.png"));
            activeBoxOffset = standardBoxOffset;

            borderTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            borderTexture.SetData(new Color[] { Color.White });

            _utilitiesService.GameEventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, HandleContentLoaded);


        }
        private static void DrawRectangle(SpriteBatch b, Rectangle rectangle, Color color)
        {
            b.Draw(borderTexture, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 2), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            b.Draw(borderTexture, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, 2), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            b.Draw(borderTexture, new Rectangle(rectangle.Left, rectangle.Top, 2, rectangle.Height), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            b.Draw(borderTexture, new Rectangle(rectangle.Right, rectangle.Top, 2, rectangle.Height + 1), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
        }
        private void HandleContentLoaded()
        {
            //
            //  only patch world map if any expansion loaded
            //
            //if (true ||_utilitiesService.HaveExpansions)
            //{
            //
            //  add patches required to draw map box on worldmap
            //
            //_patchingService.patches.AddPatch(false, typeof(MapPage), "draw",
            //new Type[] { typeof(SpriteBatch) }, typeof(WorldMapPatchService),
            //"MapPageDraw_Postfix", "Adds SDR to world map", "WorldMap");

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
            //}
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
        /// <summary>
        /// Patch of the game's map page left click event.
        /// Will pop SDRMenu is the cursor is contained in the
        /// SDR region of the map.
        /// </summary>
        /// <param name="x">Current Cursor X</param>
        /// <param name="y">Current Cursor Y</param>
        /// <param name="playSound"></param>
        /// <param name="__instance"></param>
        /// <returns>True if SDR menu is popped</returns>
        internal static bool MapPageLeftClick_Prefix(int x, int y, bool playSound, ref MapPage __instance)
        {
            if (IsMainMap(__instance) && InMeadowsBox(x, y) )
            {
                __instance.hoverText = "";
                Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(mapMenuTexture.Width, mapMenuTexture.Height);
                SDRWorldMapMenu menu = new SDRWorldMapMenu((int)topLeft.X, (int)topLeft.Y, mapMenuTexture, _landManager, _modDataService, _utilitiesService);
                menu.Open(Game1.activeClickableMenu);
                return false;
            }
            return true;
        }
        internal static bool InMeadowsBox(int x, int y)
        {
            //
            //  TODO: Add code to check for position in splitscreen game
            //
            //Game1.game1.localMultiplayerWindow
            //
            if (Context.IsSplitScreen)
                return false;
            else
                return topBoundingBox.Contains(x, y) || bottomBoundingBox.Contains(x, y);
        }
        internal static void MapPageDraw_Postfix(SpriteBatch b, ref MapPage __instance)
        {
            bool enabled = false;
            if (enabled && IsMainMap(__instance))
            {
                b.Draw(worldMapBGTexture, imageBox, new Rectangle(0, 0, worldMapBGTexture.Width, worldMapBGTexture.Height), Color.Wheat);
                Point mouseCoords = Game1.getMousePosition(ui_scale: true);
                if (topBoundingBox.Contains(mouseCoords.X, mouseCoords.Y) || bottomBoundingBox.Contains(mouseCoords.X, mouseCoords.Y))
                {
                    IClickableMenu.drawHoverText(b, "Stardew Meadows", Game1.smallFont);
                }
            }
            if (false)
            {
                Point playerTile = Game1.player.TilePoint;
                MapAreaPositionWithContext? mapPosition = WorldMapManager.GetPositionData(Game1.player.currentLocation, playerTile) ?? WorldMapManager.GetPositionData(Game1.getFarm(), Point.Zero);

                if (mapPosition.HasValue)
                {
                    foreach (var area in mapPosition.Value.Data.Area.Region.Data.MapAreas)
                    {
                        Rectangle zoneArea = new Rectangle(area.PixelArea.X * 4, area.PixelArea.Y * 4, area.PixelArea.Width * 4, area.PixelArea.Height * 4);// area.Tooltips[0].PixelArea;
                        if (zoneArea != Rectangle.Empty)
                        {
                            zoneArea.X += __instance.mapBounds.X;
                            zoneArea.Y += __instance.mapBounds.Y;
                            DrawRectangle(b, zoneArea, Color.White);
                        }
                        if (area.Tooltips.Count > 0)
                        {
                            foreach (var tooltip in area.Tooltips)
                            {
                                Rectangle tipArea = new Rectangle(tooltip.PixelArea.X * 4, tooltip.PixelArea.Y * 4, tooltip.PixelArea.Width * 4, tooltip.PixelArea.Height * 4);// area.Tooltips[0].PixelArea;
                                if (tipArea != Rectangle.Empty)
                                {
                                    tipArea.X += __instance.mapBounds.X;
                                    tipArea.Y += __instance.mapBounds.Y;
                                    DrawRectangle(b, tipArea, Color.Red);
                                }
                            }
                        }
                        foreach (var world in area.WorldPositions)
                        {
                            Rectangle worldArea = new Rectangle(world.MapPixelArea.X * 4, world.MapPixelArea.Y * 4, world.MapPixelArea.Width * 4, world.MapPixelArea.Height * 4);// area.Tooltips[0].PixelArea;
                            if (worldArea != Rectangle.Empty)
                            {
                                worldArea.X += __instance.mapBounds.X;
                                worldArea.Y += __instance.mapBounds.Y;
                                DrawRectangle(b, worldArea, Color.Blue);
                            }
                        }
                    }
                }
            }
            //var region = WorldMapManager.GetMapRegions().Where(p => p.Id == "Valley");
            //if (region.Any())
            //{
            //    foreach (var zone in region.First().GetAreas())
            //    {
            //        if (zone.GetTooltips().Length > 0)
            //        {
            //            Rectangle zoneArea = zone.GetTooltips()[0].GetPixelArea();
            //            zoneArea.X += (int)__instance.mapBounds.X;
            //            zoneArea.Y += __instance.mapBounds.Y;
            //            DrawRectangle(b, zoneArea, Color.White);
            //        }
            //    }
            //}
        }
    }
}
