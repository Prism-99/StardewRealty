

//namespace SDV_Realty_Core.Framework.Menus.Controls
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using System;



namespace SDV_Realty_Core.Framework.Menus.Controls
{
    internal class DropDownOption : OptionsElement
    {
        [InstancedStatic]
        public static DropDownOption? selected;
        public Rectangle dropDownBounds;
        public List<string> dropDownOptions = new List<string>();
        public List<string> dropDownDisplayOptions = new List<string>();
        public static Rectangle dropDownBGSource = new Rectangle(433, 451, 3, 3);
        public int selectedOption;
        private Action<int, string> optionPicked;
        public int recentSlotY;

        public int startingSelected;

        private bool clicked;
        public static Rectangle dropDownButtonSource = new Rectangle(437, 450, 10, 11);

        public DropDownOption(string label, Dictionary<string, string> options, string? selected, int whichOption, Action<int, string> optionPicked, int x = -1, int y = -1) : base(label, x, y, (int)Game1.smallFont.MeasureString("Windowed Borderless Mode   ").X + 48, 44, whichOption)
        {
            this.optionPicked = optionPicked;
            int index = 0;
            foreach (var option in options)
            {
                dropDownOptions.Add(option.Key);
                dropDownDisplayOptions.Add(option.Value);
                if (!string.IsNullOrEmpty(selected) && option.Key == selected)
                    selectedOption = index;
                else
                    index++;
            }
            RecalculateBounds();
        }
        public virtual void RecalculateBounds()
        {
            foreach (string displayed_option in dropDownDisplayOptions)
            {
                float text_width = Game1.smallFont.MeasureString(displayed_option).X;
                if (text_width >= (float)(bounds.Width - 48))
                {
                    bounds.Width = (int)(text_width + 64f);
                }
            }
            dropDownBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width - 48, bounds.Height * dropDownOptions.Count);
        }
        
        public override void leftClickHeld(int x, int y)
        {
            if (!greyedOut )
            {
                base.leftClickHeld(x, y);
                //clicked = true;
                dropDownBounds.Y = Math.Min(dropDownBounds.Y, Game1.uiViewport.Height - dropDownBounds.Height - recentSlotY);
                if (!Game1.options.SnappyMenus)
                {
                    selectedOption = (int)Math.Max(Math.Min((float)(y - dropDownBounds.Y) / (float)bounds.Height, dropDownOptions.Count - 1), 0f);
                }
            }
        }
        public override void leftClickReleased(int x, int y)
        {
            if (!greyedOut &&  dropDownOptions.Count > 0)
            {
                base.leftClickReleased(x, y);
                if (clicked)
                {
                    Game1.playSound("drumkit6");
                }
                clicked = false;
                selected = this;
                if (dropDownBounds.Contains(x, y) || (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse))
                {
                    optionPicked(whichOption, dropDownOptions[selectedOption]);
                }
                else
                {
                    selectedOption = startingSelected;
                }
                selected = null;
            }
        }
        public override void receiveLeftClick(int x, int y)
        {
            if (!greyedOut)
            {
                base.receiveLeftClick(x, y);
                startingSelected = selectedOption;
                if (!clicked)
                {
                    Game1.playSound("shwip");
                }
                leftClickHeld(x, y);
                selected = this;
                clicked = true;
            }
        }
        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            recentSlotY = slotY;
            base.draw(b, slotX, slotY, context);
            float alpha = (greyedOut ? 0.33f : 1f);
            if (clicked)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, dropDownBGSource, slotX + dropDownBounds.X, slotY + dropDownBounds.Y, dropDownBounds.Width, dropDownBounds.Height, Color.White * alpha, 4f, drawShadow: false, 0.97f);
                for (int i = 0; i < dropDownDisplayOptions.Count; i++)
                {
                    if (i == selectedOption)
                    {
                        b.Draw(Game1.staminaRect, new Rectangle(slotX + dropDownBounds.X, slotY + dropDownBounds.Y + i * bounds.Height, dropDownBounds.Width, bounds.Height), new Rectangle(0, 0, 1, 1), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
                    }
                    b.DrawString(Game1.smallFont, dropDownDisplayOptions[i], new Vector2(slotX + dropDownBounds.X + 4, slotY + dropDownBounds.Y + 8 + bounds.Height * i), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.98f);
                }
                b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X + bounds.Width - 48, slotY + bounds.Y), dropDownButtonSource, Color.Wheat * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.981f);
            }
            else
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, dropDownBGSource, slotX + bounds.X, slotY + bounds.Y, bounds.Width - 48, bounds.Height, Color.White * alpha, 4f, drawShadow: false);
                b.DrawString(Game1.smallFont, (selectedOption < dropDownDisplayOptions.Count && selectedOption >= 0) ? dropDownDisplayOptions[selectedOption] : "", new Vector2(slotX + bounds.X + 4, slotY + bounds.Y + 8), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.88f);
                b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X + bounds.Width - 48, slotY + bounds.Y), dropDownButtonSource, Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            }
        }
    }
}

