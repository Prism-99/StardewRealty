using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;


namespace SDV_Realty_Core.Framework.Menus
{
    public class FEMineCartMenu : IClickableMenu
    {
        private Rectangle Area;
        private static readonly Rectangle BottomCenter = new Rectangle(128, 192, 64, 64);
        private static readonly Rectangle Background = new Rectangle(64, 128, 64, 64);
        private static readonly Rectangle BottomLeft = new Rectangle(0, 192, 64, 64);
        private static readonly Rectangle BottomRight = new Rectangle(192, 192, 64, 64);
        private static readonly Rectangle MiddleLeft = new Rectangle(0, 128, 64, 64);
        private static readonly Rectangle MiddleRight = new Rectangle(192, 128, 64, 64);
        private static readonly Rectangle TopCenter = new Rectangle(128, 0, 64, 64);
        private static readonly Rectangle TopLeft = new Rectangle(0, 0, 64, 64);
        private static readonly Rectangle TopRight = new Rectangle(192, 0, 64, 64);
        //private static readonly Rectangle Background = new Rectangle(64, 128, 64, 64);
        private static FEListbox listBox = null;
        internal static readonly Dictionary<string, Tuple<string, Vector2>> customWarps = new Dictionary<string, Tuple<string, Vector2>> { };
        public FEMineCartMenu() : this(new Vector2(400, 600))
        {
        }
        public FEMineCartMenu(Vector2 size)
        {
            Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen((int)size.X, (int)size.Y);
            Area = new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
            initialize(Area.X, Area.Y, Area.Width, Area.Height, true);
            SetupListbox();
            
        }
        public void ClearCustomDestinations()
        {
            customWarps.Clear();
            SetupListbox();
        }
        public void AddDestination(string displayName, string locationName, int X, int Y)
        {
            //ModEntry.DestinationData.Add(new KeyValuePair<string, string>(displayName, locationName));
            customWarps.Add(displayName,  Tuple.Create(locationName, new Vector2(X, Y)));
            SetupListbox();
        }
        private void SetupListbox()
        {
            Dictionary<string, string> dcChoices = new Dictionary<string, string>
            {
                {"Farm", "farm" },
                {"Town", "town" },
                {"Mine", "mine" },
                {"Bus Stop", "bus" },
                {"Quarry", "quarry" },
                {"Desert", "desert" },
                {"Woods", "woods" },
                {"Beach", "beach" },
                {"Forest", "wizard" }
            };

            foreach (string key in customWarps.Keys)
            {
                dcChoices.Add(key, customWarps[key].Item1);
            }

            listBox = new FEListbox(new Rectangle(Area.X + 20, Area.Y + 20, Area.Width - 40, Area.Height - 40), dcChoices);

        }
        public override void draw(SpriteBatch b)
        {
            if (listBox == null) SetupListbox();

            b.Draw(Game1.menuTexture, new Rectangle(Area.X+16,Area.Y+15,Area.Width-32,Area.Height-32), Background, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(Area.X, Area.Y, 64, 64), TopLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(Area.X + Area.Width - 64, Area.Y, 64, 64), TopRight, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(Area.X, Area.Y + Area.Height - 64, 64, 64), BottomLeft, Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(Area.X + Area.Width - 64, Area.Y + Area.Height - 64, 64, 64), BottomRight, Color.White);

            listBox.draw(b);

            base.draw(b);
            drawMouse(b);
        }

        public override void performHoverAction(int x, int y)
        {
            listBox.HoverIn(new Point(x, y), Point.Zero);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y);
            if (listBox.receiveLeftClick(x, y, playSound))
            {
                base.releaseLeftClick(x, y);
            }
            if (listBox.ItemSelected)
            {
                switch (listBox.KeySelected)
                {
                    case "quarry":
                        Game1.warpFarmer("Mountain", 124, 12, 2);
                        break;
                    case "bus":
                        if (Game1.currentLocation.Name.Equals("Desert"))
                        {
                            Game1.warpFarmer("Woods", 46, 5, 1);
                        }
                        Game1.warpFarmer("BusStop", 4, 4, 2);
                        break;
                    case "mine":
                        Game1.warpFarmer("Mine", 13, 9, 1);
                        break;
                    case "town":
                        Game1.warpFarmer("Town", 105, 80, 1);
                        break;
                    case "Farm":
                        {
                            //string[] parts = Game1.getFarm().modData["Entoarox.ExtendedMinecarts.CartPoint"].Split(',');
                            //float x = float.Parse(parts[0]);
                            //float y = float.Parse(parts[1]);
                            //Game1.warpFarmer("Farm", (int)x + 1, (int)y + 3, 1);
                            break;
                        }
                    case "desert":
                        if (Game1.currentLocation.Name.Equals("BusStop"))
                        {
                            Game1.warpFarmer("Woods", 46, 5, 1);
                        }
                        Game1.warpFarmer("Desert", 29, 5, 1);
                        break;
                    case "woods":
                        Game1.warpFarmer("Woods", 46, 5, 1);
                        break;
                    case "wizard":
                        Game1.warpFarmer("Forest", 14, 40, 1);
                        break;
                    case "beach":
                        Game1.warpFarmer("Beach", 12, 4, 1);
                        break;
                    default:
                        var olist = customWarps.Where(p => p.Value.Item1 == listBox.KeySelected).ToList();
                        if (olist.Count == 1)
                        {
                            var custmWarp = olist[0];
                            Game1.warpFarmer(custmWarp.Value.Item1, (int)custmWarp.Value.Item2.X, (int)custmWarp.Value.Item2.Y, 1);
                        }
                        break;
                }
                exitThisMenu(true);
            }
        }
        public void DisableItem(string itemName)
        {
            listBox.DisableItem(itemName);
        }
        public void EnableItem(string itemName)
        {
            listBox.EnableItem(itemName);
        }
        public void EnableAllItems()
        {
            listBox.EnableAllItems();
        }
    }
}