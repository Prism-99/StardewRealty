using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.MessageBox
{
    class SDVMessageBox
    {
        private IUtilitiesService utilitiesService;
        private Action<SpriteBatch, int, int, int, int> renderer;

        public Rectangle BoundingBox { get; set; }
        public SDVMessageBox(IUtilitiesService utilitiesService, Action<SpriteBatch, int, int, int, int> renderer, int x, int y, int width, int height)
        {
            BoundingBox = new Rectangle(x, y, width, height);
            this.renderer = renderer;
            this.utilitiesService = utilitiesService;

            utilitiesService.ModHelperService.modHelper.Events.Display.RenderedHud += RenderMessageBox;
            //utilitiesService.GameEventsService.AddSubscription(new RenderedHudEventArgs(), RenderMessageBox);
        }
        ~SDVMessageBox()
        {
            utilitiesService.ModHelperService.modHelper.Events.Display.RenderedHud -= RenderMessageBox;
        }
        public void CloseBox()
        {
            utilitiesService.ModHelperService.modHelper.Events.Display.RenderedHud -= RenderMessageBox;
        }
        private void RenderMessageBox(object? obj, RenderedHudEventArgs args)
        {
            args.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(BoundingBox.X, BoundingBox.Y, BoundingBox.Width, BoundingBox.Height), new Rectangle(306, 320, 16, 16), Color.White);
            args.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(BoundingBox.X, BoundingBox.Y - 20, BoundingBox.Width, 24), new Rectangle(275, 313, 1, 6), Color.White);
            args.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(BoundingBox.X + 12, BoundingBox.Y + BoundingBox.Height, BoundingBox.Width - 20, 32), new Rectangle(275, 328, 1, 8), Color.White);
            args.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(BoundingBox.X - 32, BoundingBox.Y + 24, 32, BoundingBox.Height - 28), new Rectangle(264, 325, 8, 1), Color.White);
            args.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(BoundingBox.X + BoundingBox.Width, BoundingBox.Y, 28, BoundingBox.Height), new Rectangle(293, 324, 7, 1), Color.White);
            args.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(BoundingBox.X - 44, BoundingBox.Y - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            args.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(BoundingBox.X + BoundingBox.Width - 8, BoundingBox.Y - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            args.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(BoundingBox.X + BoundingBox.Width - 8, BoundingBox.Y + BoundingBox.Height - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            args.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(BoundingBox.X - 44, BoundingBox.Y + BoundingBox.Height - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);

            if (renderer != null)
                renderer(args.SpriteBatch, BoundingBox.X, BoundingBox.Y, BoundingBox.Width, BoundingBox.Height);
        }
    }
}