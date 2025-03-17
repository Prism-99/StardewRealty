using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SDV_Realty_Core.Framework.Menus.Controls
{
    internal class LabelOption : ICustomOption
    {
        public enum LabelSize
        {
            small,
            dialogue,
            auto
        }
        private readonly LabelSize _labelSize;
        public string Text { get { return label; } set { label = value; } }
        public LabelOption(string label, Rectangle bounds, LabelSize _labelSize = LabelSize.auto, int whichOption = -1, int controlId = -1) : base(label, bounds, whichOption, controlId)
        {
            this._labelSize = _labelSize;
            //style = Style.OptionLabel;
        }
        public LabelOption(string label, int x, int y, LabelSize _labelSize = LabelSize.auto, int whichOption = -1, int controlId = -1) : this(label, new Rectangle(x, y, 0, 0), _labelSize, whichOption, controlId)
        {
        }
        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            switch (_labelSize)
            {
                case LabelSize.small:
                    Utility.drawTextWithShadow(b, label, Game1.smallFont, new Vector2(slotX + bounds.X + (int)labelOffset.X, slotY + bounds.Y + (int)labelOffset.Y + 12), greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
                    return;
                case LabelSize.dialogue:
                    Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(slotX + bounds.X + (int)labelOffset.X, slotY + bounds.Y + (int)labelOffset.Y + 12), greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
                    return;
                //case LabelSize.auto:
                //    SpriteText.drawString(b, label, slotX + bounds.X + (int)labelOffset.X, slotY + bounds.Y + (int)labelOffset.Y + 56 - SpriteText.getHeightOfString(label), 999, -1, 999, 1f, 0.1f);
                //    break;
            }
            //if (style == Style.OptionLabel)
            //{
            //    Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(slotX + bounds.X + (int)labelOffset.X, slotY + bounds.Y + (int)labelOffset.Y + 12), greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
            //    return;
            //}
            //if (false && whichOption == -1)
            //{
            //    SpriteText.drawString(b, label, slotX + bounds.X + (int)labelOffset.X, slotY + bounds.Y + (int)labelOffset.Y + 56 - SpriteText.getHeightOfString(label), 999, -1, 999, 1f, 0.1f);
            //    return;
            //}
            int label_start_x = slotX + bounds.X + bounds.Width + 8 + (int)labelOffset.X;
            int label_start_y = slotY + bounds.Y + (int)labelOffset.Y;
            string displayed_text = label;
            SpriteFont font = Game1.dialogueFont;
            if (context != null)
            {
                int max_width = context.width - 64;
                int menu_start_x = context.xPositionOnScreen;
                if (font.MeasureString(label).X + (float)label_start_x > (float)(max_width + menu_start_x))
                {
                    int allowed_space = max_width + menu_start_x - label_start_x;
                    font = Game1.smallFont;
                    displayed_text = Game1.parseText(label, font, allowed_space);
                    label_start_y -= (int)((font.MeasureString(displayed_text).Y - font.MeasureString("T").Y) / 2f);
                }
            }
            Utility.drawTextWithShadow(b, displayed_text, font, new Vector2(label_start_x, label_start_y), greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
        }

    }
}
