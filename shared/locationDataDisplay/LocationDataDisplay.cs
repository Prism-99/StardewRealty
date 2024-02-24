using System;
using Microsoft.Xna.Framework.Graphics;
using Prism99_Core.PatchingFramework;
using StardewModdingAPI.Events;
using StardewValley.GameData.Locations;
using Prism99_Core.Utilities;
using StardewModHelpers;
using StardewModdingAPI.Utilities;


namespace locationDataDisplay
{
    internal static class LocationDataDisplay
    {
        private static GamePatches patches;
        public static bool Visible;
        private static int iRow = 0;
        private static int iWidest = 0;
        private static SpriteFont font = null;
        private static Vector2 lineheight = Vector2.Zero;
        private static KeybindList toggleKey;
        public static void Initialize(IModHelper helper, SDVLogger olog)
        {
            toggleKey = new KeybindList(new Keybind(new SButton[] { SButton.O, SButton.LeftControl }));

            patches = new GamePatches();
            patches.Initialize(helper.ModContent.ModID, olog);
            patches.AddPatch(false, typeof(GameLocation), "draw",
            new Type[] { typeof(SpriteBatch) }, typeof(LocationDataDisplay), nameof(draw),
            ".",
            "Location");
            Visible = false;
            patches.ApplyPatches("");
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (toggleKey.JustPressed())
                Visible = !Visible;
        }

        public static void draw(SpriteBatch b)
        {
            if (Game1.player.currentLocation != null && Visible)
            {
                int iBGLevel = 9;
                int iTextlevel = 10;

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
            }
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

            b.DrawString(font, text, new Vector2(15, 15 + iRow * lineheight.Y), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, iTextlevel);
            iRow++;
        }
    }
}
