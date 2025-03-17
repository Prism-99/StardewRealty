using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using StardewValley.Menus;
using System;
using System.Linq;

namespace CustomMenuFramework.Controls
{
    internal class RealEstateListing : IScrollingControl
    {
        private ExpansionPack expansionPack;
        private Texture2D menuTexture;
        private OptionsButton purchaseButton;
        private Point purchaseButtonPos;
        private int cost;
        private Action<string> bought;
        private Texture2D bgTexture;
        private Texture2D overlayTexture;
        public Color BackgroundColour { set { bgTexture.SetData<Color>(new Color[] { value }); } }

        public RealEstateListing(Rectangle bounds, string name, ExpansionPack pack, Action<string> bought) : base(bounds, name)
        {
            this.bought = bought;
            expansionPack = pack;
            cost = pack.Cost;
            menuTexture = Game1.content.Load<Texture2D>("Maps\\MenuTiles");
            purchaseButton = new OptionsButton(I18n.Purchase(), Purchase);
            purchaseButton.bounds.Height = 45;
            purchaseButtonPos = new Point(bounds.X + 20, bounds.Y + bounds.Height - 64);
            bgTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            BackgroundColour = Color.White;
            overlayTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color overLayColour = Color.Black;
            //overLayColour.A = 255;
            overlayTexture.SetData<Color>(new Color[] { overLayColour });
        }
        public void SetCost(int cost)
        {
            this.cost = cost;
        }
        private void Purchase()
        {
            Game1.playSound("shwip");
        }
        public override void snapToMenu(int x, int y)
        {
            Point position = new Point(x+purchaseButtonPos.X, y + purchaseButtonPos.Y);
            Game1.setMousePosition(position.X  , position.Y , ui_scale: true);
        }
        public override void receiveLeftClick(int x, int y)
        {
            if (!purchaseButton.greyedOut && purchaseButton.bounds.Contains(x - purchaseButtonPos.X, y - purchaseButtonPos.Y))
            {
                bought(expansionPack.LocationName);
            }
        }
        public override void draw(SpriteBatch b, int x, int y)
        {
            SpriteFont font = Game1.smallFont;// ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont);
            float shadow_intensity = 0.5f;
            Color font_color = Color.Black;
            Rectangle sourceRect = new Rectangle(64, 128, 64, 64);
            int destSize = 40;
            //b.Draw(menuTexture, new Rectangle(x , y, bounds.Width , bounds.Height), sourceRect, Color.White);
            b.Draw(bgTexture, new Rectangle(x + 5, y + 10, bounds.Width - 10, bounds.Height - 10), sourceRect, Color.White);
            sourceRect.Y = 0;
            sourceRect.X = 0;
            b.Draw(menuTexture, new Rectangle(x - 10, y, destSize, destSize), sourceRect, Color.White);
            sourceRect.X = 192;
            b.Draw(menuTexture, new Rectangle(x + bounds.Width - destSize + 10, y, destSize, destSize), sourceRect, Color.White);
            sourceRect.Y = 192;
            b.Draw(menuTexture, new Rectangle(x + bounds.Width - destSize + 10, y + bounds.Height - destSize + 10, destSize, destSize), sourceRect, Color.White);
            sourceRect.X = 0;
            b.Draw(menuTexture, new Rectangle(x - 10, y + bounds.Height - destSize + 10, destSize, destSize), sourceRect, Color.White);
            sourceRect.X = 128;
            sourceRect.Y = 0;
            b.Draw(menuTexture, new Rectangle(x - 10 + destSize, y, bounds.Width - destSize - 10, destSize), sourceRect, Color.White);
            sourceRect.Y = 192;
            b.Draw(menuTexture, new Rectangle(x - 10 + destSize, y + bounds.Height - destSize + 10, bounds.Width - destSize - 10, destSize), sourceRect, Color.White);
            sourceRect.Y = 128;
            sourceRect.X = 0;
            b.Draw(menuTexture, new Rectangle(x - 10, y + destSize, destSize, bounds.Height - destSize * 2 + 10), sourceRect, Color.White);
            sourceRect.X = 192;
            b.Draw(menuTexture, new Rectangle(x + bounds.Width - destSize + 10, y + destSize, destSize, bounds.Height - destSize * 2 + 10), sourceRect, Color.White);

            string toolTip = "";
            Rectangle imageBox = new Rectangle(x + 25, y + 70, bounds.Width - 50, 180);
            if (expansionPack.ForSaleImage != null)
            {
                b.Draw(expansionPack.ForSaleImage, imageBox, Color.White);
                //if (imageBox.Contains(Game1.getMousePosition()))
                //{
                //    toolTip = expansionPack.Description;
                //}
                if (expansionPack.MapSize != Vector2.Zero)
                {
                    b.Draw(overlayTexture, new Rectangle(imageBox.X, imageBox.Y + imageBox.Height - 35, imageBox.Width, 35), Color.White * 0.4f);
                    b.DrawString(Game1.smallFont, $"{I18n.Size()} {expansionPack.MapSize.X}x{expansionPack.MapSize.Y}", new Vector2(imageBox.X + 5, imageBox.Y + imageBox.Height - 32), Color.White);
                }
            }
            //b.DrawString(Game1.smallFont, expansionPack.DisplayName, new Vector2(x+20,y+25),Color.Black);
            Utility.drawTextWithShadow(b, expansionPack.DisplayName, Game1.smallFont, new Vector2(x + 20, y + 25), font_color, 1, -1f, -1, -1, shadow_intensity);
            int textPadding = 30;
            string description = "";
            if (true || string.IsNullOrEmpty(expansionPack.ForSaleDescription))
            {
                string raw_description = expansionPack.Description.Replace("\r\n", " ");
                description = Game1.parseText(raw_description, font, bounds.Width - textPadding);
                //float height = font.MeasureString(description).Y;
                //float scale = 1f;
                //float max_height = 150f;
                //while (height > max_height && scale > 0.7f)
                //{
                //    scale -= 0.05f;
                //    description = Game1.parseText(raw_description, font, (int)((bounds.Width- textPadding) / scale));
                //    height = font.MeasureString(description).Y;
                //}
                string[] lines = description.Split("\r\n");
                description = string.Join("\r\n", lines.Take(5));
                //bool shortend = false;
                //int chop = 0;
                //while (height > max_height && chop < description.Length)
                //{
                //    shortend = true;
                //    chop++;
                //    description = Game1.parseText(raw_description.Substring(0, raw_description.Length - chop), font, (int)((bounds.Width - textPadding) / scale));
                //    //description = ;
                //    height = font.MeasureString(description).Y;
                //}
                if (lines.Length > 5)
                {
                    description = description.Substring(0, description.LastIndexOf(" ") - 1);
                    description = description.Substring(0, description.Length - 3) + "...";
                }
                expansionPack.ForSaleDescription = description;
            }
            else
            {
                description = expansionPack.ForSaleDescription;
            }
            if (cost > 0)
                b.DrawString(Game1.smallFont, I18n.Cost(cost), new Vector2(x + 20, y + bounds.Height - 100), Color.Black);

            Utility.drawTextWithShadow(b, description, font, new Vector2(x + 20, y + 260), font_color, scale, -1f, 0, 0, shadow_intensity);
            purchaseButton.greyedOut = cost > Game1.player.Money;
            if (!purchaseButton.greyedOut)
                purchaseButton.draw(b, x + purchaseButtonPos.X, y + purchaseButtonPos.Y);
            //if (!string.IsNullOrEmpty(toolTip))
            //    //IClickableMenu.drawHoverText(b, toolTip, Game1.smallFont);
            //    IClickableMenu.drawToolTip(b, toolTip, "", null);
        }
    }
}
