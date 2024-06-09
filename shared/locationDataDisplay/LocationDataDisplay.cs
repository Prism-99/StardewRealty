using System;
using Microsoft.Xna.Framework.Graphics;
using Prism99_Core.PatchingFramework;
using StardewModdingAPI.Events;
using StardewValley.GameData.Locations;
using Prism99_Core.Utilities;
using StardewModHelpers;
using StardewModdingAPI.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System.Collections.Generic;


namespace locationDataDisplay
{
    internal class LocationDataDisplay
    {
        private static GamePatches patches;
        public static bool Visible;
        private static int iRow = 0;
        private static int iWidest = 0;
        private static List<string> displayLines;
        private static SpriteFont font = null;
        private static Vector2 lineheight = Vector2.Zero;
        private static KeybindList toggleKey;
        public LocationDataDisplay(IModHelperService helper, ILoggerService olog, IGameEventsService gameEventsService)
        {
            toggleKey = new KeybindList(new Keybind(new SButton[] { SButton.O, SButton.LeftControl }));
            Visible = false;

            //patches = new GamePatches();
            //patches.Initialize(helper.modHelper.ModContent.ModID, (SDVLogger)olog.CustomLogger);
            //patches.AddPatch(false, typeof(GameLocation), "draw",
            //    new Type[] { typeof(SpriteBatch) }, typeof(LocationDataDisplay), nameof(draw),
            //    ".",
            //    "Location");
            //patches.ApplyPatches("");

            gameEventsService.AddSubscription(new RenderedHudEventArgs(), RenderDisplay);
            gameEventsService.AddSubscription(typeof( ButtonPressedEventArgs).Name, Input_ButtonPressed);
        }

        private void RenderDisplay(EventArgs e)
        {
            RenderData(Game1.spriteBatch);
         }
        private void Input_ButtonPressed(EventArgs e)
        {
            if (toggleKey.JustPressed())
                Visible = !Visible;
        }
        private static void RenderData(SpriteBatch b)
        {
            if (Game1.player.currentLocation != null && Visible)
            {
                float iBGLevel = 5f;
                int iTextlevel = 4;
                displayLines = new List<string>();

                if (font == null) font = Game1.smallFont;// ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont);
                iRow = 0;
                iWidest = 0;
                string fishArea = "None";
                if (Game1.player.currentLocation.TryGetFishAreaForTile(Game1.player.Tile, out string id, out FishAreaData data))
                {
                    fishArea = Game1.player.currentLocation.GetFishingAreaDisplayName(id) ?? id;
                }
                Writeline(b, $"Location: {Game1.player.currentLocation.DisplayName}", iTextlevel);
                if (!string.IsNullOrEmpty(Game1.player.currentLocation.locationContextId))
                {
                    Writeline(b, $"ContextId: {Game1.player.currentLocation.locationContextId}", iTextlevel);
                }
                Writeline(b, $"Location: {Game1.player.currentLocation.DisplayName}", iTextlevel);
                Writeline(b, $"Farmer: {Game1.player.Tile.X},{Game1.player.Tile.Y}", iTextlevel);
                if ((Game1.player.currentLocation.GetData()?.FishAreas.Values.Count ?? 0) > 0)
                {
                    Writeline(b, $"# of Fish Areas: {Game1.player.currentLocation.GetData().FishAreas.Values.Count}", iTextlevel);
                    Writeline(b, $"Fish Area: {fishArea}", iTextlevel);
                }
                Writeline(b, $"Season: {Game1.player.currentLocation.GetSeason()}", iTextlevel);
                bool filterProps = false;

                foreach (string prop in Game1.player.currentLocation.Map.Properties.Keys)
                {
                    switch (prop)
                    {
                        case "SeasonOverride":
                        case "DefaultWarpLocation":
                        case "AllowGaintCrops":
                        case "CanBuildHere":
                        case "LooserBuildRestions":
                        case "Outdoors":
                            Writeline(b, $"{prop}: '{Game1.player.currentLocation.Map.Properties[prop]}'", iTextlevel);
                            break;
                        default:
                            if (!filterProps)
                            {
                                Writeline(b, $"{prop}:{Game1.player.currentLocation.Map.Properties[prop]}", iTextlevel);
                            }
                            break;
                    }
                }
                int bgWidth = Math.Min(iWidest + 40, 450);
                int bgHeight = 40 + (int)(iRow * lineheight.Y);

                StardewBitmap bitmap = new StardewBitmap(bgWidth, bgHeight);
                bitmap.FillRectangle(Color.Black, 0, 0, bgWidth, bgHeight);
                b.Draw(bitmap.Texture(), new Rectangle(10, 10, bgWidth, bgHeight), null, Color.AliceBlue, 0, Vector2.Zero, SpriteEffects.None, iBGLevel);
                int printLine = 0;
                foreach(string line in displayLines)
                {
                    b.DrawString(font, line, new Vector2(15, 15 + printLine++ * lineheight.Y), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, iTextlevel);
                }
            }

        }
        public static void draw(SpriteBatch b)
        {
            RenderData(b);
        }
        private static void Writeline(SpriteBatch b, string text, int iTextlevel)
        {
            if (lineheight == Vector2.Zero)
            {
                lineheight = font.MeasureString("A");
            }
            Vector2 textmeasure = font.MeasureString(text);

            if (textmeasure.X > iWidest)
                iWidest = (int)textmeasure.X;

            displayLines.Add(text);
            //b.DrawString(font, text, new Vector2(15, 15 + iRow * lineheight.Y), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, iTextlevel);
            iRow++;
        }
    }
}
