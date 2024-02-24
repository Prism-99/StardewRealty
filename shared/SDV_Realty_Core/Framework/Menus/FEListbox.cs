using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;

namespace SDV_Realty_Core.Framework.Menus
{
    internal class FEListbox
    {
        private Rectangle Area;
        public int currentItemIndex;
        private bool scrolling;
        public ClickableTextureComponent upArrow;
        private Rectangle scrollBarRunner;

        public ClickableTextureComponent downArrow;

        public ClickableTextureComponent scrollBar;
        private FEListButton[] displayItems;
        //ClickableComponent
        public bool ItemSelected = false;
        public string KeySelected = "";
        private int numButtonsVisible;

        public FEListbox(Rectangle rArea, Dictionary<string, string> options)
        {
            Area = rArea;
            displayItems = new FEListButton[options.Count];
            int iIndex = 0;
            foreach (string key in options.Keys)
            {
                displayItems[iIndex++] = new FEListButton(new Rectangle(0, 0, 0, 0), options[key], key, true);
            }
            int buttonHeight = 40;

            numButtonsVisible = (int)Math.Floor((float)Area.Height / buttonHeight);

            for (int i = 0; i < displayItems.Length ; i++)
            {
                displayItems[i].bounds = new Rectangle(rArea.X + 40, rArea.Y + i * buttonHeight + 40, 250, buttonHeight);
            }
             upArrow = new ClickableTextureComponent(new Rectangle(Area.X + Area.Width - 44, Area.Y + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
            {
                myID = 97865,
                downNeighborID = 106,
                leftNeighborID = 3546
            };
            downArrow = new ClickableTextureComponent(new Rectangle(Area.X + Area.Width - 44, Area.Y + Area.Height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
            {
                myID = 106,
                upNeighborID = 97865,
                leftNeighborID = 3546
            };
            scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, Area.Height - 64 - upArrow.bounds.Height - 28);
        }
        public void EnableItem(string itemKey)
        {
            var olist=displayItems.Where(p=>p.name == itemKey).ToList();
            if(olist.Count() > 0)
            {
                olist[0].Disabled = false;
            }
        }
        public void DisableItem(string itemKey)
        {
            var olist = displayItems.Where(p => p.name == itemKey).ToList();
            if (olist.Count() > 0)
            {
                olist[0].Disabled = true;
            }
        }
        public void EnableAllItems()
        {
            foreach(FEListButton button in displayItems)
            {
                button.Disabled= false;
            }
        }
        public void draw(SpriteBatch b)
        {
             if (displayItems.Length > numButtonsVisible)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
                scrollBar.draw(b);
                upArrow.draw(b);
                downArrow.draw(b);
            }
            for (int i = 0; i < numButtonsVisible && i<displayItems.Length; i++)
            {
                displayItems[i].draw(b);
            }
 
        }
        public bool receiveLeftClick(int x, int y, bool playSound = true)
        {
            bool releaseClick = false;
            ItemSelected = false;
            KeySelected = null;
            if (Game1.activeClickableMenu == null)
            {
                return false;
            }
            //Vector2 snappedPosition = inventory.snapToClickableComponent(x, y);
            if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, displayItems.Length - 4))
            {
                downArrowPressed();
                Game1.playSound("shwip");
            }
            else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
            {
                upArrowPressed();
                Game1.playSound("shwip");
            }
            else if (scrollBar.containsPoint(x, y))
            {
                scrolling = true;
            }
            else if (!downArrow.containsPoint(x, y) && x > Area.X + Area.Width && x < Area.X + Area.Width + 128 && y > Area.Y && y < Area.Y + Area.Height)
            {
                scrolling = false;
                //leftClickHeld(x, y);
                releaseClick = true;
                //releaseLeftClick(x, y);
            }
            else
            {
                var oList = displayItems.Where(p => p.containsPoint(x, y)).ToList();
                if (oList.Count == 1 && !oList.First().Disabled)
                {
                    ItemSelected = true;
                    KeySelected = oList.First().name;
                }
            }
            //for (int k = 0; k < tabButtons.Count; k++)
            //{
            //    if (tabButtons[k].containsPoint(x, y))
            //    {
            //        switchTab(k);
            //    }
            //}
            currentItemIndex = Math.Max(0, Math.Min(displayItems.Length - 4, currentItemIndex));

            return releaseClick;
        }
        private void downArrowPressed()
        {
            downArrow.scale = downArrow.baseScale;
            currentItemIndex++;
            setScrollBarToCurrentIndex();
            //updateSaleButtonNeighbors();
        }

        private void upArrowPressed()
        {
            upArrow.scale = upArrow.baseScale;
            currentItemIndex--;
            setScrollBarToCurrentIndex();
            // updateSaleButtonNeighbors();
        }


        private void setScrollBarToCurrentIndex()
        {
            if (displayItems.Length > 0)
            {
                scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, displayItems.Length - 4 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
                if (currentItemIndex == displayItems.Length - 4)
                {
                    scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                }
            }
        }
        public void HoverIn(Point p, Point o)
        {
            var oList = displayItems.Where(r => !r.containsPoint(p.X, p.Y) && r.Hovered).ToList();
            if (oList.Count == 1)
            {
                oList.First().HoverOut(p, o);
            }
            oList = displayItems.Where(r => r.containsPoint(p.X, p.Y)).ToList();
            if (oList.Count == 1)
            {
                oList.First().HoverIn(p, o);
            }
        }
    }
}