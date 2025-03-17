using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.Objects
{
    public class StardewMeadowsTrain:Train
    {
        public float yPos {  get; set; }
        private TemporaryAnimatedSprite whistleSteam;
        public StardewMeadowsTrain():base()
        {
            yPos = 2792f;
        }
        public StardewMeadowsTrain(float yCrossing):base()
        {
            yPos = yCrossing;
        }

        public new Rectangle getBoundingBox()
        {
            return new Rectangle(-cars.Count * 128 * 4 + (int)position.X, (int)yPos+96, cars.Count * 128 * 4, 128);
        }
        public new void draw(SpriteBatch b, GameLocation location)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                cars[i].draw(b, new Vector2(position.X - (float)((i + 1) * 512), yPos), wheelRotation, location);
            }
            TemporaryAnimatedSprite whistle =(TemporaryAnimatedSprite) Traverse.Create(this).Field("whistleSteam").GetValue();
            whistle?.draw(b);
        }
        public new bool Update(GameTime time, GameLocation location)
        {
            if (Game1.IsMasterGame)
            {
                position.X += (float)time.ElapsedGameTime.Milliseconds * speed;
            }
            wheelRotation += (float)time.ElapsedGameTime.Milliseconds * ((float)Math.PI / 256f);
            wheelRotation %= (float)Math.PI * 2f;
            if (!Game1.eventUp && location.Equals(Game1.currentLocation))
            {
                Farmer player = Game1.player;
                Rectangle playerBounds = player.GetBoundingBox();
                Rectangle trainBounds = getBoundingBox();
                if (playerBounds.Intersects(trainBounds))
                {
                    player.xVelocity = 8f;
                    player.yVelocity = (float)(trainBounds.Center.Y - playerBounds.Center.Y) / 4f;
                    player.takeDamage(20, overrideParry: true, null);
                    if (player.UsingTool)
                    {
                        Game1.playSound("clank");
                    }
                }
            }
            if (Game1.random.NextDouble() < 0.001 && location.Equals(Game1.currentLocation))
            {
                Game1.playSound("trainWhistle");
                whistleSteam = new TemporaryAnimatedSprite(27, new Vector2(position.X - 250f, yPos+32), Color.White, 8, flipped: false, 100f, 0, 64, 1f, 64);
            }
            if (whistleSteam != null)
            {
                whistleSteam.Position = new Vector2(position.X - 258f, yPos);
                if (whistleSteam.update(time))
                {
                    whistleSteam = null;
                }
            }
            smokeTimer -= time.ElapsedGameTime.Milliseconds;
            if (smokeTimer <= 0f)
            {
                location.temporarySprites.Add(new TemporaryAnimatedSprite(25, new Vector2(position.X - 170f, yPos-96), Color.White, 8, flipped: false, 100f, 0, 64, 1f, 128));
                smokeTimer = speed * 2000f;
            }
            if (position.X >cars.Count * 128 * 4 + location.Map.DisplayWidth)
            {
                return true;
            }
            return false;
        }

    }
}
