using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CustomMenuFramework.Controls
{
    internal class TextEntryOption : ICustomOption
    {
        public const int pixelsHigh = 11;
        private Action<int, string>? pickedAction = null;
        public TextBox textBox;
        private bool selected = false;
        public TextEntryOption(string label, Rectangle bounds, int whichOption, Action<int, string>? picked = null, int controlId=-1) : base(label, bounds, whichOption,controlId)
        {
            pickedAction = picked;
            textBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Color.Black);
            textBox.Width = bounds.Width;
            this.bounds.Height = textBox.Height;
            textBox.OnEnterPressed += textBoxEnter;
        }
        public void textBoxEnter(TextBox sender)
        {
            if (pickedAction != null)
                pickedAction(whichOption, sender.Text);

            textBox.Selected = false;
            selected = false;
        }
        public void LostFocus()
        {
            if (selected)
            {
                textBoxEnter(textBox);
             }
        }
        public override void receiveKeyPress(Keys key)
        {
            int xx = 1;
            //base.receiveKeyPress(key);
        }
        public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
        {
            textBox.X = slotX + bounds.Left - 8;
            textBox.Y = slotY + bounds.Top;
            textBox.Draw(b);
            base.draw(b, slotX, slotY, context);
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y)
        {
            textBox.SelectMe();
            //textBox.Update();
            selected = textBox.Selected;

        }
    }
}
