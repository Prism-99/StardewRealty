using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.Objects;



namespace SDV_Realty_Core.Framework.Menus
{
    internal class FETreeTransplantMenu : IClickableMenu
    {
        private IClickableMenu TreeTransplantMenu;
        private ClickableTextureComponent swapFarmButton;
        private Rectangle previousClientBounds;

        public FETreeTransplantMenu(IClickableMenu menu)
        {
            TreeTransplantMenu = menu;

            assignClientBounds();

            //swapFarmButton = new ClickableTextureComponent(new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), framework.TreeTransplantFarmIcon, new Rectangle(0, 0, 64, 64), 1.0f);

            resetBounds();
        }

        private void resetBounds()
        {
            swapFarmButton.bounds.X = (Game1.viewport.Width - Game1.tileSize * 2) - (int)(Game1.tileSize / 0.75) - (int)(Game1.tileSize / 0.75);
            swapFarmButton.bounds.Y = (Game1.viewport.Height - Game1.tileSize * 2);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
            swapFarmButton.tryHover(x, y);
        }

        public override void draw(SpriteBatch b)
        {
            if (Game1.globalFade)
                return;

            if (Game1.game1.Window.ClientBounds.X != previousClientBounds.X || Game1.game1.Window.ClientBounds.Y != previousClientBounds.Y || Game1.game1.Window.ClientBounds.Width != previousClientBounds.Width || Game1.game1.Window.ClientBounds.Height != previousClientBounds.Height)
            {
                assignClientBounds();
                resetBounds();
            }

            swapFarmButton.draw(b);
        }

        private void assignClientBounds()
        {
            Rectangle newClientBounds = Game1.game1.Window.ClientBounds;
            previousClientBounds = new Rectangle(newClientBounds.X, newClientBounds.Y, newClientBounds.Width, newClientBounds.Height);
        }
    }
}
