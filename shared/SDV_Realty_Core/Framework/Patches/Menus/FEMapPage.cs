using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley;
using SDV_Realty_Core.Framework.Objects;



namespace SDV_Realty_Core.Framework.Patches.Menus
{
  internal  class FEMapPage
    {
         public static void drawMiniPortraits(SpriteBatch b,MapPage __instance)
        {

            Dictionary<Vector2, int> dictionary = new Dictionary<Vector2, int>();
            foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
            {
                Vector2 vector;
                if (FEFramework.farmExpansions.ContainsKey(onlineFarmer.currentLocation.Name)) {
                    Rectangle rMapLoc = FEFramework.GetExpansionWorldMapLocation(onlineFarmer.currentLocation.Name);
                    if (rMapLoc.IsEmpty)
                    {
                        vector = Vector2.Zero;
                    }
                    else
                    {
                        vector = new Vector2(rMapLoc.X * 4 + 60, rMapLoc.Y * 4);
                    }
                }
                else
                {
#if v16
                    vector = new Vector2(32f, 32f);
#else
                    vector = __instance.getPlayerMapPosition(onlineFarmer) - new Vector2(32f, 32f);
#endif
                }
                int value = 0;
                if (vector != Vector2.Zero)
                {
                    dictionary.TryGetValue(vector, out value);
                    dictionary[vector] = value + 1;
                    vector += new Vector2(48 * (value % 2), 48 * (value / 2));
                    onlineFarmer.FarmerRenderer.drawMiniPortrat(b, vector, 0.00011f, 4f, 2, onlineFarmer);
                }
            }
        }
    }
}
