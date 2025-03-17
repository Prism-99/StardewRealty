using System;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley.GameData.Locations;
using StardewModHelpers;
using StardewModdingAPI.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using System.Collections.Generic;
using xTile.Layers;
using System.Linq;
using StardewValley.Menus;
using StardewValley;
using StardewValley.TokenizableStrings;


namespace locationDataDisplay
{
    internal class LocationDataDisplay
    {
        public static int mode = 0;
        private static int currentRow = 0;
        private static int widestLine = 0;
        private static List<string> displayLines;
        private static SpriteFont font = null;
        private static Vector2 lineheight = Vector2.Zero;
        private static KeybindList toggleKey;
        private Texture2D borderTexture;

        public bool filterProps { get; set; } = false;

        public LocationDataDisplay(IGameEventsService gameEventsService)
        {
#if DEBUG
            mode=3;
#endif
            toggleKey = new KeybindList(new Keybind(new SButton[] { SButton.O, SButton.LeftControl }));
            borderTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            borderTexture.SetData(new Color[] { Color.White });

            gameEventsService.AddSubscription(new RenderedHudEventArgs(), RenderDisplay);
            gameEventsService.AddSubscription(typeof(ButtonPressedEventArgs).Name, Input_ButtonPressed);
        }

        private void RenderDisplay(EventArgs e)
        {
            RenderedHudEventArgs ev = (RenderedHudEventArgs)e;
            if (Game1.player.currentLocation != null)
            {
                if (mode == 1 || mode == 2)
                    RenderData(ev.SpriteBatch);
                if (mode == 2 || mode == 3)
                    RenderMapDetails(ev.SpriteBatch);
            }
        }
        /// <summary>
        /// Highlights tiles that are:
        /// -warps
        /// -TouchAction
        /// -Action
        /// </summary>
        /// <param name="b"></param>
        private void RenderMapDetails(SpriteBatch b)
        {
            int tileX = (int)Math.Floor(Game1.viewport.X / 64f);
            int tileY = (int)Math.Floor(Game1.viewport.Y / 64f);
            int width = (int)Math.Ceiling(Game1.viewport.Width / 64f);
            int height = (int)Math.Ceiling(Game1.viewport.Height / 64f);
            Color warpOutColour = Color.Red;
            Color warpInColour = Color.Pink;
            Color touchActionColour = Color.Blue;
            Color actionColour = Color.Yellow;
            // create list for tracking stacked Tooltips
            List<Tuple<string, string>> toolTips = new List<Tuple<string, string>>();

            //
            //  get warp ins
            //
            var entranceLocations = Game1.locations.Where(p => p.warps.Where(p => p.TargetName == Game1.currentLocation.Name && new Rectangle(tileX, tileY, width, height).Contains(new Point(p.TargetX, p.TargetY))).Any()).ToList();
            if (entranceLocations.Any())
            {
                foreach (var warpLoc in entranceLocations)
                {
                    var entranceWarps = warpLoc.warps.Where(p => p.TargetName == Game1.currentLocation.Name && new Rectangle(tileX, tileY, width, height).Contains(new Point(p.TargetX, p.TargetY)));
                    foreach (var warp in entranceWarps)
                    {
                        Rectangle warpBox = new Rectangle((int)(warp.TargetX * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.X * Game1.options.zoomLevel)), (int)(warp.TargetY * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.Y * Game1.options.zoomLevel)), (int)(64 * Game1.options.zoomLevel), (int)(64 * Game1.options.zoomLevel));
                        DrawRectangle(b, warpBox, warpInColour);
                        if (warpBox.Contains(Game1.getMousePosition()))
                        {
                            toolTips.Add(Tuple.Create("Warp In From", $"{warpLoc.DisplayName ?? warpLoc.Name} ({warp.X},{warp.Y})"));
                        }
                    }
                }
            }
            //
            //  add building warp ins
            //
            if (Game1.currentLocation.IsBuildableLocation())
            {
                var buildingWarps = Game1.currentLocation.buildings.Where(p => p.GetIndoors()?.warps.Any(p => p.TargetName == Game1.currentLocation.Name) ?? false);
                foreach (var buildinglocation in buildingWarps)
                {
                    foreach (var buildingwarp in buildinglocation.GetIndoors().warps.Where(p => p.TargetName == Game1.currentLocation.Name))
                    {
                        Rectangle warpBox = new Rectangle((int)(buildingwarp.TargetX * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.X * Game1.options.zoomLevel)), (int)(buildingwarp.TargetY * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.Y * Game1.options.zoomLevel)), (int)(64 * Game1.options.zoomLevel), (int)(64 * Game1.options.zoomLevel));
                        DrawRectangle(b, warpBox, warpInColour);
                        if (warpBox.Contains(Game1.getMousePosition()))
                        {
                            toolTips.Add(Tuple.Create("Warp In From", $"{TokenParser.ParseText(buildinglocation.GetIndoors().GetData()?.DisplayName ?? null) ?? buildinglocation.GetIndoors().Name} ({buildingwarp.X},{buildingwarp.Y})"));
                        }
                    }
                }
            }
            //
            //  add warp tiles
            //
            var infocusWarps = Game1.currentLocation.warps.Where(p => new Rectangle(tileX, tileY, width, height).Contains(new Point(p.X, p.Y)));

            if (infocusWarps.Any())
            {
                foreach (var warp in infocusWarps)
                {
                    Rectangle warpBox = new Rectangle((int)(warp.X * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.X * Game1.options.zoomLevel)), (int)(warp.Y * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.Y * Game1.options.zoomLevel)), (int)(64 * Game1.options.zoomLevel), (int)(64 * Game1.options.zoomLevel));
                    DrawRectangle(b, warpBox, warpOutColour);
                    if (warpBox.Contains(Game1.getMousePosition()))
                    {
                        toolTips.Add(Tuple.Create("Warp Out To", $"{warp.TargetName} ({warp.TargetX},{warp.TargetY})"));
                    }
                }
            }
            //
            //  add warps off screen top
            if (tileY == 0)
            {
                infocusWarps = Game1.currentLocation.warps.Where(p => p.Y == -1 && p.X >= tileX && p.X < tileX + width);
                foreach (var warp in infocusWarps)
                {
                    Rectangle warpBox = new Rectangle((int)(warp.X * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.X * Game1.options.zoomLevel)), (int)((warp.Y + 1) * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.Y * Game1.options.zoomLevel)), (int)(64 * Game1.options.zoomLevel), (int)(64 * Game1.options.zoomLevel));
                    DrawRectangle(b, warpBox, warpOutColour);
                    float lineSpace = 64 * Game1.options.zoomLevel / 7;
                    for (int lines = 1; lines < 8; lines++)
                    {
                        DrawLine(b, new Vector2(warpBox.X + lines * lineSpace, warpBox.Y), new Vector2(warpBox.X + lines * lineSpace, warpBox.Y + (int)(64 * Game1.options.zoomLevel)), Color.Red);
                    }
                    if (warpBox.Contains(Game1.getMousePosition()))
                    {
                        toolTips.Add(Tuple.Create("Warp Out To", $"{warp.TargetName} ({warp.TargetX},{warp.TargetY})"));
                    }
                }
            }
            //
            //  add warps off screen left
            if (tileX == 0)
            {
                infocusWarps = Game1.currentLocation.warps.Where(p => p.X == -1 && p.Y >= tileY && p.Y < tileY + height);
                foreach (var warp in infocusWarps)
                {
                    Rectangle warpBox = new Rectangle((int)((warp.X + 1) * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.X * Game1.options.zoomLevel)), (int)(warp.Y * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.Y * Game1.options.zoomLevel)), (int)(64 * Game1.options.zoomLevel), (int)(64 * Game1.options.zoomLevel));
                    DrawRectangle(b, warpBox, warpOutColour);
                    float lineSpace = 64 * Game1.options.zoomLevel / 7;
                    for (int lines = 1; lines < 8; lines++)
                    {
                        DrawLine(b, new Vector2(warpBox.X + lines * lineSpace, warpBox.Y), new Vector2(warpBox.X + lines * lineSpace, warpBox.Y + (int)(64 * Game1.options.zoomLevel)), Color.Red);
                    }
                    if (warpBox.Contains(Game1.getMousePosition()))
                    {
                        toolTips.Add(Tuple.Create("Warp Out To", $"{warp.TargetName} ({warp.TargetX},{warp.TargetY})"));
                    }
                }
            }
            //
            //  add warps off screen right
            infocusWarps = Game1.currentLocation.warps.Where(p => p.X == tileX + width && p.Y >= tileY && p.Y < tileY + height);
            foreach (var warp in infocusWarps)
            {
                Rectangle warpBox = new Rectangle((int)((warp.X - 1) * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.X * Game1.options.zoomLevel)), (int)(warp.Y * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.Y * Game1.options.zoomLevel)), (int)(64 * Game1.options.zoomLevel), (int)(64 * Game1.options.zoomLevel));
                DrawRectangle(b, warpBox, warpOutColour);
                float lineSpace = 64 * Game1.options.zoomLevel / 7;
                for (int lines = 1; lines < 8; lines++)
                {
                    DrawLine(b, new Vector2(warpBox.X + lines * lineSpace, warpBox.Y), new Vector2(warpBox.X + lines * lineSpace, warpBox.Y + (int)(64 * Game1.options.zoomLevel)), Color.Red);
                }
                if (warpBox.Contains(Game1.getMousePosition()))
                {
                    toolTips.Add(Tuple.Create("Warp Out To", $"{warp.TargetName} ({warp.TargetX},{warp.TargetY})"));
                }
            }
            //
            //  add warps off screen bottom
            infocusWarps = Game1.currentLocation.warps.Where(p => p.X >= tileX && p.X < tileX + width && p.Y == tileY + height);
            foreach (var warp in infocusWarps)
            {
                Rectangle warpBox = new Rectangle((int)(warp.X * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.X * Game1.options.zoomLevel)), (int)((warp.Y - 1) * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.Y * Game1.options.zoomLevel)), (int)(64 * Game1.options.zoomLevel), (int)(64 * Game1.options.zoomLevel));
                DrawRectangle(b, warpBox, warpOutColour);
                float lineSpace = 64 * Game1.options.zoomLevel / 7;
                for (int lines = 1; lines < 8; lines++)
                {
                    DrawLine(b, new Vector2(warpBox.X + lines * lineSpace, warpBox.Y), new Vector2(warpBox.X + lines * lineSpace, warpBox.Y + (int)(64 * Game1.options.zoomLevel)), Color.Red);
                }
                if (warpBox.Contains(Game1.getMousePosition()))
                {
                    toolTips.Add(Tuple.Create("Warp Out To", $"{warp.TargetName} ({warp.TargetX},{warp.TargetY})"));
                }
            }


            //
            //  highlight selected tile properties
            //
            Layer back = Game1.currentLocation.Map.GetLayer("Back");
            Layer buildings = Game1.currentLocation.Map.GetLayer("Buildings");
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Rectangle activeTile = new Rectangle((int)((tileX + x) * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.X * Game1.options.zoomLevel)), (int)((tileY + y) * 64 * Game1.options.zoomLevel - (int)(Game1.viewport.Y * Game1.options.zoomLevel)), (int)(64 * Game1.options.zoomLevel), (int)(64 * Game1.options.zoomLevel));
                    //
                    //  add TouchAction tiles
                    //
                    if (back != null)
                    {
                        if (back.Tiles[tileX + x, tileY + y] != null && back.Tiles[tileX + x, tileY + y].Properties != null)
                        {
                            if (back.Tiles[tileX + x, tileY + y].Properties.TryGetValue("TouchAction", out var prop))
                            {
                                if (activeTile.Contains(Game1.getMousePosition()))
                                {
                                    toolTips.Add(Tuple.Create("TouchAction", prop.ToString()));
                                }
                                DrawRectangle(b, activeTile, touchActionColour);
                            }
                        }
                    }
                    //
                    //  add Action tiles
                    //
                    if (buildings != null)
                    {
                        if (buildings.Tiles[tileX + x, tileY + y] != null && buildings.Tiles[tileX + x, tileY + y].Properties != null)
                        {
                            if (buildings.Tiles[tileX + x, tileY + y].Properties.TryGetValue("Action", out var prop))
                            {
                                if (activeTile.Contains(Game1.getMousePosition()))
                                {
                                    toolTips.Add(Tuple.Create("Action", prop.ToString()));
                                }
                                DrawRectangle(b, activeTile, actionColour);
                            }
                        }
                    }
                    //
                    //  add any Tooltips
                    //
                    if (toolTips.Count > 0)
                    {
                        string title = string.Join("/", toolTips.Select(p => p.Item1));
                        string body = string.Join("\n", toolTips.Select(p => p.Item2));
                        IClickableMenu.drawToolTip(b, body, title, null);
                    }
                }
            }
        }
        public void DrawLine(SpriteBatch b, Vector2 start, Vector2 end, Color color)
        {
            float length = (end - start).Length();
            float rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
            b.Draw(borderTexture, start, null, color, rotation, Vector2.Zero, new Vector2(length, 2), SpriteEffects.None, 0);
        }
        public void DrawRectangle(SpriteBatch b, Rectangle rectangle, Color color)
        {
            b.Draw(borderTexture, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 2), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            b.Draw(borderTexture, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, 2), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            b.Draw(borderTexture, new Rectangle(rectangle.Left, rectangle.Top, 2, rectangle.Height), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
            b.Draw(borderTexture, new Rectangle(rectangle.Right, rectangle.Top, 2, rectangle.Height + 1), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
        }
        private void Input_ButtonPressed(EventArgs e)
        {
            if (toggleKey.JustPressed())
            {
                mode++;
                if (mode > 3) mode = 0;
            }

        }
        private void RenderData(SpriteBatch b)
        {
            float iBGLevel = 5f;
            int iTextlevel = 4;
            displayLines = new List<string>();

            if (font == null) font = Game1.smallFont;
            currentRow = 0;
            widestLine = 0;
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
            int bgWidth = Math.Min(widestLine + 40, 450);
            int bgHeight = 40 + (int)(currentRow * lineheight.Y);

            //
            //  add tile properties
            //
            Layer backLayer = Game1.player.currentLocation.Map.GetLayer("Back");
            if (backLayer != null)
            {
                Point dataPoint = new Point((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y);
                if (backLayer.IsValidTileLocation(dataPoint.X, dataPoint.Y))
                {
                    try
                    {
                        if (backLayer.Tiles[dataPoint.X, dataPoint.Y]?.Properties != null)
                        {
                            foreach (var prop in backLayer.Tiles[dataPoint.X, dataPoint.Y].Properties)
                            {
                                Writeline(b, $"{prop.Key}: {prop.Value}", iTextlevel);
                            }
                        }
                    }
                    catch { }
                }
            }

            StardewBitmap bitmap = new StardewBitmap(bgWidth, bgHeight);
            bitmap.FillRectangle(Color.Black, 0, 0, bgWidth, bgHeight);
            b.Draw(bitmap.Texture(), new Rectangle(10, 10, bgWidth, bgHeight), null, Color.AliceBlue, 0, Vector2.Zero, SpriteEffects.None, iBGLevel);
            int printLine = 0;
            foreach (string line in displayLines)
            {
                b.DrawString(font, line, new Vector2(15, 15 + printLine++ * lineheight.Y), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, iTextlevel);
            }
        }

        private static void Writeline(SpriteBatch b, string text, int iTextlevel)
        {
            if (lineheight == Vector2.Zero)
            {
                lineheight = font.MeasureString("A");
            }
            Vector2 textmeasure = font.MeasureString(text);

            if (textmeasure.X > widestLine)
                widestLine = (int)textmeasure.X;

            displayLines.Add(text);
            currentRow++;
        }
    }
}
