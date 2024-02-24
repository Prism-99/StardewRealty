using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using System.Collections.Generic;



namespace SDV_Realty_Core.Framework.Objects
{
    internal class WarpSign : SDObject
    {
        private DateTime dtLastChange = DateTime.Now;
        private int CurrentLocationIndex = -1;
        private Dictionary<string, Tuple<string, EntranceDetails>> CaveEntrances;
        public WarpSign() { }
        public WarpSign(Vector2 newLocation, Dictionary<string, Tuple<string, EntranceDetails>> CaveEntrances)
        {
            this.CaveEntrances = CaveEntrances;
            tileLocation.Value = newLocation;
            boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
            ParentSheetIndex = 11;
            Name = "sdr.warpsign";
            ItemId = "sdr.warpsign";
        }
        public string Text { get; set; }
        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            //base.draw(spriteBatch, xNonTile, yNonTile, layerDepth, alpha);
        }
        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            Vector2 vector = getScale();
            float num = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            vector *= 4f;
            Vector2 textSize = Game1.smallFont.MeasureString(Text ?? "");
            // text area width 250
            int iTextOffset = 155 - (int)(textSize.X / 2);

            Vector2 locationNamePosition = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + iTextOffset + 40, y * 64 + 24));
            Rectangle destinationRectangle = new Rectangle((int)(locationNamePosition.X - vector.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(locationNamePosition.Y - vector.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + vector.X), (int)(128f + vector.Y / 2f));
            //spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, getSourceRectForBigCraftable(11), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, num);
            //string caveKey;
            bool bUpdateText = false;
            if (CurrentLocationIndex == -1)
            {
                CurrentLocationIndex = 0;

                bUpdateText = true;
            }
            else
            {
                if ((DateTime.Now - dtLastChange).TotalSeconds > 2)
                {
                    CurrentLocationIndex++;
                    if (CurrentLocationIndex > CaveEntrances.Count - 1) CurrentLocationIndex = 0;
                    bUpdateText = true;
                }
            }
            if (bUpdateText)
            {
                string caveEntrance = CaveEntrances.Keys.ToArray()[CurrentLocationIndex];

                if (caveEntrance == "FarmHouse" || caveEntrance.Contains("Cabin"))
                {
                    Text = "Home";
                }
                else
                {
                    Text = CaveEntrances[caveEntrance].Item1;
                }
                //
                //  update warps points in room
                //

                Vector2[] arWarps = new Vector2[] { new Vector2(7, 5), new Vector2(8, 5), new Vector2(9, 5) };

                GameLocation glWarp = Game1.getLocationFromName(WarproomManager.WarpRoomLoacationName);

                foreach (Vector2 vWarp in arWarps)
                {
                    Warp wTmp = GetWarp(vWarp, glWarp);
                    if (wTmp != null) glWarp.warps.Remove(wTmp);
                    glWarp.warps.Add(new Warp((int)vWarp.X, (int)vWarp.Y, caveEntrance, CaveEntrances[caveEntrance].Item2.WarpIn.X, CaveEntrances[caveEntrance].Item2.WarpIn.Y, true));
                }

                dtLastChange = DateTime.Now;
            }
            spriteBatch.DrawString(Game1.smallFont, Text ?? "", locationNamePosition, Color.White);
         }
        private Warp GetWarp(Vector2 vVector, GameLocation gl)
        {
            var olist = gl.warps.Where(p => p.X == vVector.X && p.Y == vVector.Y).ToList();

            if (olist.Any())
            {
                return olist.First();
            }
            else
            {
                return null;
            }
        }
    }
}
