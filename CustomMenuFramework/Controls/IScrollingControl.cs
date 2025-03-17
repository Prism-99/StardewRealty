using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomMenuFramework.Controls
{
    internal abstract class IScrollingControl : ClickableComponent
    {
        protected IScrollingControl(Rectangle bounds, string name) : base(bounds, name)
        {
        }
        public abstract void receiveLeftClick(int x, int y);
        public abstract void draw(SpriteBatch b, int x, int y);
        public abstract void snapToMenu(int x, int y);
    }
}
