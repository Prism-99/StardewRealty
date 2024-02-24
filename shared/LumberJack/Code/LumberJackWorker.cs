using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.Pathfinding;
using StardewValley.Tools;
using Netcode;


namespace LumberJack.Code
{
    internal class LumberJackWorker : NPC
    {
        private readonly NetGuid netHome = new NetGuid();
        private int whichJunimoFromThisHut;
        private new Rectangle nextPosition;
        private bool destroy;

        public LumberJackWorker(GameLocation location, Vector2 position, JunimoHut hut, int whichLJNumberFromThisCamp, Color? c)
     : base(new AnimatedSprite("Characters\\LumberJack", 0, 16, 16), position, 2, "LumberJack")
        {
            currentLocation = location;
            base.currentLocation = location;
            home = hut;
            whichJunimoFromThisHut = whichLJNumberFromThisCamp;
            //if (!c.HasValue)
            //{
            //    pickColor();
            //}
            //else
            //{
            //    color.Value = c.Value;
            //}

            nextPosition = GetBoundingBox();
            base.Breather = false;
            base.speed = 3;
            forceUpdateTimer = 9999;
            collidesWithOtherCharacters.Value = true;
            ignoreMovementAnimation = true;
            farmerPassesThrough = true;
            base.Scale = 0.75f;
            base.willDestroyObjectsUnderfoot = false;
            Vector2 vector = Vector2.Zero;
            switch (whichLJNumberFromThisCamp)
            {
                case 0:
                    vector = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((int)hut.tileX.Value + 1, (int)hut.tileY.Value + (int)hut.tilesHigh.Value + 1), 30);
                    break;
                case 1:
                    vector = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((int)hut.tileX.Value - 1, (int)hut.tileY.Value), 30);
                    break;
                case 2:
                    vector = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((int)hut.tileX.Value + (int)hut.tilesWide.Value, (int)hut.tileY.Value), 30);
                    break;
            }

            if (vector != Vector2.Zero)
            {
                //controller = new PathFindController(this, base.currentLocation, Utility.Vector2ToPoint(vector), -1, reachFirstDestinationFromHut, 100);
            }

            if ((controller == null || controller.pathToEndPoint == null) && Game1.IsMasterGame)
            {
                pathfindToRandomSpotAroundHut();
                if (controller?.pathToEndPoint == null)
                {
                    destroy = true;
                }
            }

            collidesWithOtherCharacters.Value = false;
        }
        public void pathfindToRandomSpotAroundHut()
        {
            JunimoHut junimoHut = home;
            if (junimoHut != null)
            {
                //controller = new PathFindController(endPoint: Utility.Vector2ToPoint(new Vector2((int)junimoHut.tileX + 1 + Game1.random.Next(-junimoHut.cropHarvestRadius, junimoHut.cropHarvestRadius + 1), (int)junimoHut.tileY + 1 + Game1.random.Next(-junimoHut.cropHarvestRadius, junimoHut.cropHarvestRadius + 1))), c: this, location: base.currentLocation, finalFacingDirection: -1, endBehaviorFunction: reachFirstDestinationFromHut, limit: 100);
            }
        }
        private JunimoHut home
        {
            get
            {
                if (!base.currentLocation.buildings.TryGetValue(netHome.Value, out var value))
                {
                    return null;
                }

                return value as JunimoHut;
            }
            set
            {
                netHome.Value = base.currentLocation.buildings.GuidOf(value);
            }
        }
    }
}
