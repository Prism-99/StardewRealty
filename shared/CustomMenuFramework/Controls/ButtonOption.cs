using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomMenuFramework.Controls
{
    internal class ButtonOption : ICustomOption
    {
        private Action<int>? _onClick;
        public ButtonOption(string label, int x, int y, Action<int> onClick = null, int whichOption = -1, int controlId = -1) : base(label, x, y, 0, 0, whichOption, controlId)
        {
            _onClick = onClick;

            int width = (int)Game1.smallFont.MeasureString(label).X + 64;
            int height = 45;
            bounds = new Rectangle(x, y, width, height);
        }
        public override void receiveLeftClick(int x, int y)
        {
            if (_onClick != null)
                _onClick(whichOption);
        }
        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            float draw_layer = 0.8f - (float)(slotY + bounds.Y) * 1E-06f;
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), slotX + bounds.X, slotY + bounds.Y, bounds.Width, bounds.Height, Color.White * (greyedOut ? 0.33f : 1f), 4f, drawShadow: true, draw_layer);
            Vector2 string_center = Game1.smallFont.MeasureString(label) / 2f;
            string_center.X = (int)(string_center.X / 4f) * 4;
            string_center.Y = (int)(string_center.Y / 4f) * 4;
            Utility.drawTextWithShadow(b, label, Game1.smallFont, new Vector2(slotX + bounds.Center.X, slotY + bounds.Center.Y) - string_center, Game1.textColor * (greyedOut ? 0.33f : 1f), 1f, draw_layer + 1E-06f, -1, -1, 0f);
        }
    }
}
