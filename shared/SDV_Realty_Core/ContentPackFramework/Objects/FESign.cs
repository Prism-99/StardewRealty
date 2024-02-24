using System;
using StardewValley.Objects;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using StardewModHelpers;

namespace SDV_Realty_Core.ContentPackFramework.Objects
{
    public class FESign : Sign
    {
        private StardewBitmap imSign;
        private Texture2D txSign=null;
        private string sSignText="";
        public FESign() { }


        public FESign(Vector2 signpos, string sText) : base(signpos, "39")
        {

            sSignText = sText;
            LoadSign();
            //Rectangle rBaseBox = base.boundingBox.Value;
            //base.boundingBox.Value = new Rectangle(rBaseBox.X, rBaseBox.Y, rBaseBox.Width * 2, rBaseBox.Height);
        }
        private void LoadSign()
        {
            StardewBitmap iTileSheet = new StardewBitmap(Game1.bigCraftableSpriteSheet);
            Rectangle sourceRectangle = getSourceRectForBigCraftable(39);
            imSign = new StardewBitmap(sourceRectangle.Width * 8, sourceRectangle.Height * 4);
            imSign.DrawImage(iTileSheet, new Rectangle(0, 0, imSign.Width, imSign.Height), sourceRectangle);
            imSign.DrawString(sSignText, 10, 30);

            txSign = imSign.Texture();
        }
        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            // base.draw(spriteBatch, x, y, alpha);
            Vector2 vPos = new Vector2((float)(x * 64), (float)((y - 1) * 64));
            //Texture2D objectSpriteSheet = Game1.bigCraftableSpriteSheet;
            if (txSign == null) LoadSign();
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, vPos);
            Rectangle sourceRectangle = new Rectangle(0, 0, txSign.Width, txSign.Height);
            Color color = Color.White * alpha;
            Vector2 origin = new Vector2(8f, 8f);
            Vector2 scale2 = scale;
            float num = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            spriteBatch.Draw(txSign, position, sourceRectangle, color, 0f, origin, (scale.Y > 1f) ? getScale().Y : 1f, ((bool)flipped.Value) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, num);
            // Utility.drawTextWithShadow(spriteBatch, "Hello", Game1.smallFont, new Vector2((float)(x * 64 + 25), (float)((y - 1) * 64 + 32)), Color.White, 1f, num + .1f, -1, -1, 0.0f, 3);

        }
    }
}