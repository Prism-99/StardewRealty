using System;
using System.Collections.Generic;
using System.Text;
using StardewValley.Tools;

#if !v16
using SDObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using StardewValley;
#endif

namespace SDV_Realty_Core.Framework.Patches.Tools
{
    internal class FEPickaxe
	{
		public static bool DoFunction(GameLocation location, int x, int y, int power, Farmer who,Pickaxe __instance)
		{
            //base.DoFunction(location, x, y, power, who);
            //__instance.power = who.toolPower;
            if (!__instance.isEfficient.Value)
            {
                who.Stamina -= (float)(2 * (power + 1)) - (float)who.MiningLevel * 0.1f;
            }
            Utility.clampToTile(new Vector2(x, y));
            int tileX = x / 64;
            int tileY = y / 64;
            Vector2 tile = new Vector2(tileX, tileY);
            //if (location.performToolAction(__instance, tileX, tileY))
            //{
            //    return true;
            //}
            SDObject o = null;
            location.Objects.TryGetValue(tile, out o);
            if (o == null)
            {
                if (who.FacingDirection == 0 || who.FacingDirection == 2)
                {
                    tileX = (x - 8) / 64;
                    location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
                    if (o == null)
                    {
                        tileX = (x + 8) / 64;
                        location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
                    }
                }
                else
                {
                    tileY = (y + 8) / 64;
                    location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
                    if (o == null)
                    {
                        tileY = (y - 8) / 64;
                        location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
                    }
                }
                x = tileX * 64;
                y = tileY * 64;
#if v16
                if (location.terrainFeatures.ContainsKey(tile) && location.terrainFeatures[tile].performToolAction(__instance, 0, tile))
#else
                if (location.terrainFeatures.ContainsKey(tile) && location.terrainFeatures[tile].performToolAction(__instance, 0, tile, location))
#endif
                {
                    location.terrainFeatures.Remove(tile);
                }
            }
            tile = new Vector2(tileX, tileY);
            //if (o != null)
            //{
            //	if (o.Name.Equals("Stone"))
            //	{
            //		location.playSound("hammer");
            //		if ((int)o.minutesUntilReady > 0)
            //		{
            //			int damage = Math.Max(1, (int)upgradeLevel + 1) + additionalPower.Value;
            //			o.minutesUntilReady.Value -= damage;
            //			o.shakeTimer = 200;
            //			if ((int)o.minutesUntilReady > 0)
            //			{
            //				Game1.createRadialDebris(Game1.currentLocation, 14, tileX, tileY, Game1.random.Next(2, 5), resource: false);
            //				return;
            //			}
            //		}
            //		if (o.ParentSheetIndex < 200 && !Game1.objectInformation.ContainsKey(o.ParentSheetIndex + 1) && (int)o.parentSheetIndex != 25)
            //		{
            //			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(o.ParentSheetIndex + 1, 300f, 1, 2, new Vector2(x - x % 64, y - y % 64), flicker: true, o.flipped)
            //			{
            //				alphaFade = 0.01f
            //			});
            //		}
            //		else
            //		{
            //			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(47, new Vector2(tileX * 64, tileY * 64), Color.Gray, 10, flipped: false, 80f));
            //		}
            //		Game1.createRadialDebris(location, 14, tileX, tileY, Game1.random.Next(2, 5), resource: false);
            //		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(46, new Vector2(tileX * 64, tileY * 64), Color.White, 10, flipped: false, 80f)
            //		{
            //			motion = new Vector2(0f, -0.6f),
            //			acceleration = new Vector2(0f, 0.002f),
            //			alphaFade = 0.015f
            //		});
            //		location.OnStoneDestroyed(o.parentSheetIndex, tileX, tileY, getLastFarmerToUse());
            //		if ((int)o.minutesUntilReady <= 0)
            //		{
            //			o.performRemoveAction(new Vector2(tileX, tileY), location);
            //			location.Objects.Remove(new Vector2(tileX, tileY));
            //			location.playSound("stoneCrack");
            //			Game1.stats.RocksCrushed++;
            //		}
            //	}
            //	else if (o.Name.Contains("Boulder"))
            //	{
            //		location.playSound("hammer");
            //		if (base.UpgradeLevel < 2)
            //		{
            //			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Pickaxe.cs.14194")));
            //			return;
            //		}
            //		if (tileX == boulderTileX && tileY == boulderTileY)
            //		{
            //			hitsToBoulder += power + 1;
            //			o.shakeTimer = 190;
            //		}
            //		else
            //		{
            //			hitsToBoulder = 0;
            //			boulderTileX = tileX;
            //			boulderTileY = tileY;
            //		}
            //		if (hitsToBoulder >= 4)
            //		{
            //			location.removeObject(tile, showDestroyedObject: false);
            //			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X - 32f, 64f * (tile.Y - 1f)), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
            //			{
            //				delayBeforeAnimationStart = 0
            //			});
            //			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X + 32f, 64f * (tile.Y - 1f)), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
            //			{
            //				delayBeforeAnimationStart = 200
            //			});
            //			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X, 64f * (tile.Y - 1f) - 32f), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
            //			{
            //				delayBeforeAnimationStart = 400
            //			});
            //			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile.X, 64f * tile.Y - 32f), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
            //			{
            //				delayBeforeAnimationStart = 600
            //			});
            //			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * tile.X, 64f * tile.Y), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, 128));
            //			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * tile.X + 32f, 64f * tile.Y), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, 128)
            //			{
            //				delayBeforeAnimationStart = 250
            //			});
            //			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * tile.X - 32f, 64f * tile.Y), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, 128)
            //			{
            //				delayBeforeAnimationStart = 500
            //			});
            //			location.playSound("boulderBreak");
            //			Game1.stats.BouldersCracked++;
            //		}
            //	}
            //	else if (o.performToolAction(this, location))
            //	{
            //		o.performRemoveAction(tile, location);
            //		if (o.type.Equals("Crafting") && (int)o.fragility != 2)
            //		{
            //			Game1.currentLocation.debris.Add(new Debris(o.bigCraftable ? (-o.ParentSheetIndex) : o.ParentSheetIndex, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y)));
            //		}
            //		Game1.currentLocation.Objects.Remove(tile);
            //	}
            //}
            //else
            //{
            //	location.playSound("woodyHit");
            //	if (location.doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null)
            //	{
            //		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileX * 64, tileY * 64), Color.White, 8, flipped: false, 80f)
            //		{
            //			alphaFade = 0.015f
            //		});
            //	}
            //}

            return false;

        }
    }
}
