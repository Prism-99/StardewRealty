using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;


namespace SDV_Realty_Core.Framework.Menus
{
    internal class FEListButton : ClickableComponent
    {
        protected static readonly Rectangle ButtonNormal = new Rectangle(256, 256, 10, 10);
        protected static readonly Rectangle ButtonHover = new Rectangle(267, 256, 10, 10);
        private bool drawLabelWithShadow;
        public bool Hovered;
        protected bool Pressed;
        public bool Disabled;

        public FEListButton(Rectangle bounds, string name) : base(bounds, name)
        {
        }
        public FEListButton(Rectangle bounds, string name, string label, bool drawShadow = false) : base(bounds, name, label)
        {
            drawLabelWithShadow = drawShadow;
        }
        public void HoverIn(Point p, Point o)
        {
            if (this.Disabled)
                return;
            Game1.playSound("Cowboy_Footstep");
            this.Hovered = true;
        }

        public void HoverOut(Point p, Point o)
        {
            if (this.Disabled)
                return;
            this.Hovered = false;
        }
        public void draw(SpriteBatch b)
        {
            if (visible)
            {
                draw(b, Color.White, 0.86f + (float)bounds.Y / 20000f);
            }
        }
        public void draw(SpriteBatch b, Color c, float layerDepth, int frameOffset = 0)
        {
            if (!visible)
            {
                return;
            }

            if (!string.IsNullOrEmpty(label))
            {
                Rectangle r = this.Hovered && !this.Pressed ? ButtonHover : ButtonNormal;
                int endWidth = 8;
                b.Draw(Game1.mouseCursors, new Rectangle(this.bounds.X, this.bounds.Y, endWidth, this.bounds.Height), new Rectangle(r.X, r.Y, 2, r.Height), Color.White * (this.Disabled ? 0.33f : 1), 0, Vector2.Zero, SpriteEffects.None, 1f);
                // End
                b.Draw(Game1.mouseCursors, new Rectangle(this.bounds.X + this.bounds.Width, this.bounds.Y, endWidth, this.bounds.Height), new Rectangle(r.X + r.Width -2, r.Y, 2, r.Height), Color.White * (this.Disabled ? 0.33f : 1), 0, Vector2.Zero, SpriteEffects.None, 1f);
                // Center
                b.Draw(Game1.mouseCursors, new Rectangle(this.bounds.X+ endWidth, this.bounds.Y, this.bounds.Width- endWidth, this.bounds.Height), new Rectangle(r.X + 2, r.Y, r.Width-4 , r.Height), Color.White * (this.Disabled ? 0.33f : 1), 0, Vector2.Zero, SpriteEffects.None, 1f);

                Vector2 textSize = Game1.smallFont.MeasureString(label);
                Vector2 vTextPos = new Vector2(bounds.X + (bounds.Width / 2) - (textSize.X / 2f), (float)bounds.Y + ((float)(bounds.Height / 2) - textSize.Y / 2f));
                if (drawLabelWithShadow)
                {
                    Utility.drawTextWithShadow(b, label, Game1.smallFont,vTextPos, Game1.textColor);
                }
                else
                {
                    b.DrawString(Game1.smallFont, label, vTextPos, Game1.textColor);
                }
             }
        }
    }
}
