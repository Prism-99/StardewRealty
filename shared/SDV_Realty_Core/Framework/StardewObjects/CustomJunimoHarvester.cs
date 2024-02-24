using System;
using StardewValley.Characters;
using StardewValley.Buildings;
using Netcode;
using HarmonyLib;
using Prism99_Core.Utilities;
using StardewValley.Pathfinding;

namespace Framework.StardewObjects
{
    class CustomJunimoHarvester : JunimoHarvester
    {
        //private JunimoHut home;
        private readonly NetColor localColor = new NetColor();
        private new Rectangle nextPosition;
        private bool destroy;
        private Farm homelocation;
        private SDVLogger logger;
        public CustomJunimoHarvester(SDVLogger mon, Vector2 position, JunimoHut myHome, Farm glFarm, int whichJunimoNumberFromThisHut, Color? c)
        {
            logger = mon;
            initNetFields();
            homelocation = glFarm;
            //base(new AnimatedSprite("Characters\\Junimo", 0, 16, 16), position, 2, "Junimo")

            //base(sprite, position, 2, name)
            sprite.Value = new AnimatedSprite("Characters\\Junimo", 0, 16, 16);

            Position = position;
            speed = 2;
            Name = "Junimo";
            datable.Value = false;
            if (sprite.Value != null)
            {
                Traverse.Create(this).Field("originalSourceRect").SetValue(sprite.Value.SourceRect);
            }

            faceDirection(2);
            DefaultPosition = position;
            defaultFacingDirection = 2;
            lastCrossroad = new Rectangle((int)position.X, (int)position.Y + 64, 64, 64);


            home = myHome;
            whichJunimoFromThisHut = whichJunimoNumberFromThisHut;
            if (!c.HasValue)
            {
                pickColor();
            }
            else
            {
                localColor.Value = c.Value;
            }
            Traverse.Create(this).Field("color").SetValue(localColor);

            nextPosition = GetBoundingBox();
            base.Breather = false;
            speed = 1;
            forceUpdateTimer = 9999;
            collidesWithOtherCharacters.Value = true;
            ignoreMovementAnimation = true;
            farmerPassesThrough = true;
            base.Scale = 0.75f;
            base.willDestroyObjectsUnderfoot = false;
            base.currentLocation = glFarm;
            Vector2 vector = Vector2.Zero;
            switch (whichJunimoNumberFromThisHut)
            {
                case 0:
                    vector = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2(home.tileX.Value + 1, home.tileY.Value + home.tilesHigh.Value + 1), 30);
                    break;
                case 1:
                    vector = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2(home.tileX.Value - 1, home.tileY.Value), 30);
                    break;
                case 2:
                    vector = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2(home.tileX.Value + home.tilesWide.Value, home.tileY.Value), 30);
                    break;
            }

            if (vector != Vector2.Zero)
            {
                controller = new PathFindController(this, base.currentLocation, Utility.Vector2ToPoint(vector), -1, reachFirstDestinationFromHut, 100);
            }

            if ((controller == null || controller.pathToEndPoint == null) && Game1.IsMasterGame)
            {
                pathfindToRandomSpotAroundHut();
                if (controller == null || controller.pathToEndPoint == null)
                {
                    destroy = true;
                }
            }

            collidesWithOtherCharacters.Value = false;

        }
        private JunimoHut home
        {
            get
            {
                NetGuid buildingGuid = (NetGuid)Traverse.Create(this).Field("netHome").GetValue();
                return homelocation.buildings[buildingGuid.Value] as JunimoHut;
            }
            set
            {
                Traverse.Create(this).Field("netHome").SetValue(new NetGuid(homelocation.buildings.GuidOf(value)));
            }
        }
        private void pickColor()
        {
            Random random = new Random(home.tileX.Value + home.tileY.Value * 777 + whichJunimoFromThisHut);
            if (random.NextDouble() < 0.25)
            {
                switch (random.Next(8))
                {
                    case 0:
                        localColor.Value = Color.Red;
                        break;
                    case 1:
                        localColor.Value = Color.Goldenrod;
                        break;
                    case 2:
                        localColor.Value = Color.Yellow;
                        break;
                    case 3:
                        localColor.Value = Color.Lime;
                        break;
                    case 4:
                        localColor.Value = new Color(0, 255, 180);
                        break;
                    case 5:
                        localColor.Value = new Color(0, 100, 255);
                        break;
                    case 6:
                        localColor.Value = Color.MediumPurple;
                        break;
                    case 7:
                        localColor.Value = Color.Salmon;
                        break;
                }

                if (random.NextDouble() < 0.01)
                {
                    localColor.Value = Color.White;
                }
            }
            else
            {
                switch (random.Next(8))
                {
                    case 0:
                        localColor.Value = Color.LimeGreen;
                        break;
                    case 1:
                        localColor.Value = Color.Orange;
                        break;
                    case 2:
                        localColor.Value = Color.LightGreen;
                        break;
                    case 3:
                        localColor.Value = Color.Tan;
                        break;
                    case 4:
                        localColor.Value = Color.GreenYellow;
                        break;
                    case 5:
                        localColor.Value = Color.LawnGreen;
                        break;
                    case 6:
                        localColor.Value = Color.PaleGreen;
                        break;
                    case 7:
                        localColor.Value = Color.Turquoise;
                        break;
                }
            }
        }

    }
}
