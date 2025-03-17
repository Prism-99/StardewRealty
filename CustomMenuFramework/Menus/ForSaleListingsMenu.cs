using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using StardewValley.Menus;
using CustomMenuFramework.Controls;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace CustomMenuFramework.Menus
{
    internal class ForSaleListingsMenu : IScrollingMenu
    {
        //private bool centerMenu = false;
        //private ClickableTextureComponent upArrow;
        //private ClickableTextureComponent downArrow;
        //private ClickableTextureComponent leftArrow;
        //private ClickableTextureComponent rightArrow;
        //private ClickableTextureComponent scrollBar;
        //private Rectangle scrollBarRunner;
        //private bool scrolling;
        //private float scrollBarPosition = 0;
        //private bool verticalScrollBar;
        //private int numberOfSlots = 0;
        private bool useGlobalPrice = false;
        private int globalPrice;
        //private int displayIndex = 0;
        //private Action<string>? onClose = null;
        //private string expansionBought = "";
        public ForSaleListingsMenu(int x, int y, int width, int height, Action<string>? onClose = null, bool center = false, bool showUpperRightCloseButton = false, bool verticalScroll = true) : base(x, y, width, height,onClose,center, showUpperRightCloseButton,verticalScroll)
        {
            listingWidth = 350;
            //this.onClose = onClose;
            //centerMenu = center;
            //verticalScrollBar = verticalScroll;
            ////menuTexture = Game1.content.Load<Texture2D>("Maps\\MenuTiles");
            //LoadNavigationControls();
            //int top = yPositionOnScreen + 84;// + 80 + 4;

            //ResizeMenu();
        }
        public void SetGlobalPrice(int price)
        {
            useGlobalPrice = true;
            globalPrice = price;
        }
        public void SetListings(List<ExpansionPack> listings)
        {
            int slotId = 2000;
            foreach (ExpansionPack pack in listings.OrderBy(p => p.DisplayName))
            {
                RealEstateListing listing = new RealEstateListing(new Rectangle(0, 0, listingWidth, 500), "a", pack, LandBought);
                if (useGlobalPrice)
                    listing.SetCost(globalPrice);

                listing.myID = slotId++;
                if (listing.myID > 2000)
                {
                    listing.leftNeighborID = listing.myID - 1;
                    listing.downNeighborID = listing.myID - 1;
                }
                listing.rightNeighborID = listing.myID + 1;
                listing.upNeighborID = listing.myID + 1;

                AddControlToDisplaySlots(listing);
            }
        }
        //private void ExitMenu()
        //{
        //    if (this == Game1.activeClickableMenu)
        //    {
        //        Game1.exitActiveMenu();
        //        if (onClose != null)
        //            onClose(expansionBought);

        //    }
        //}
        private void LandBought(string expansionId)
        {
            itemSelected = expansionId;
            ExitMenu();
        }
        //private void LoadNavigationControls()
        //{
        //    if (verticalScrollBar)
        //    {
        //        upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
        //        downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
        //        scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
        //    }
        //    else
        //    {
        //        leftArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421 - 68, 472 + 22, 11, 12), 4f);
        //        rightArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421 - 66 + 11, 472 + 22, 11, 12), 4f);
        //        scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 40, 20), Game1.mouseCursors, new Rectangle(420, 441, 10, 10), 4f);
        //    }
        //    scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 128 - upArrow.bounds.Height - 8);
        //}
        //private void ResizeMenu()
        //{
        //    if (centerMenu)
        //    {
        //        xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
        //        yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).Y;// Game1.uiViewport.Height/2 - height/2 - 64;
        //    }
        //    if (upperRightCloseButton != null)
        //    {
        //        upperRightCloseButton.bounds = new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen - 8, 48, 48);
        //    }
        //    if (verticalScrollBar)
        //    {
        //        upArrow.setPosition(xPositionOnScreen + width - 90, yPositionOnScreen + 110);
        //        downArrow.setPosition(xPositionOnScreen + width - 90, yPositionOnScreen + height - 100);
        //        scrollBar.setPosition(xPositionOnScreen + width - 78, yPositionOnScreen + 160);
        //        scrollBarRunner.X = xPositionOnScreen + width - 78;
        //        scrollBarRunner.Y = yPositionOnScreen + 115 + upArrow.bounds.Height;
        //        scrollBarRunner.Height = height - 268;
        //        numberOfSlots = height / 80;
        //    }
        //    else
        //    {
        //        leftArrow.setPosition(xPositionOnScreen + 40, yPositionOnScreen + height - 96);
        //        rightArrow.setPosition(xPositionOnScreen + width - 100, yPositionOnScreen + height - 96);
        //        scrollBarRunner.X = xPositionOnScreen + 87;
        //        scrollBarRunner.Y = yPositionOnScreen + height - 84;
        //        scrollBarRunner.Height = 26;
        //        scrollBarRunner.Width = width - 190;
        //        scrollBar.setPosition(scrollBarRunner.X + 2, scrollBarRunner.Y);
        //        numberOfSlots = (int)Math.Floor((float)(width - 200) / listingWidth);
        //    }
        //}
        //public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        //{
        //    base.gameWindowSizeChanged(oldBounds, newBounds);
        //    width = Math.Min(1920, Game1.uiViewport.Width) - 20;
        //    height = Math.Min(1080, Game1.uiViewport.Height) - 20;
        //    ResizeMenu();
        //}
        //public override void receiveScrollWheelAction(int direction)
        //{
        //    if (!GameMenu.forcePreventClose)
        //    {
        //        base.receiveScrollWheelAction(direction);
        //        if (direction < 0 && scrollBarPosition > 0)
        //        {
        //            downArrowPressed("shiny4");
        //        }
        //        else if (direction > 0 && scrollBarPosition < 1f)
        //        {
        //            upArrowPressed("shiny4");
        //        }
        //        if (Game1.options.SnappyMenus)
        //        {
        //            snapCursorToCurrentSnappedComponent();
        //        }
        //    }
        //}
        //private void downArrowPressed(string sound = "shwip")
        //{
        //    Game1.playSound(sound);
        //    displayIndex--;
        //    displayIndex = Math.Max(0, displayIndex);
        //    scrollBarPosition = (float)displayIndex / (displaySlots.Count - numberOfSlots);
        //    SetScrollPosition();
        //}
        //private void upArrowPressed(string sound = "shwip")
        //{
        //    Game1.playSound(sound);
        //    displayIndex++;
        //    displayIndex = Math.Min(displaySlots.Count - numberOfSlots, displayIndex);
        //    scrollBarPosition = (float)displayIndex / (displaySlots.Count - numberOfSlots);
        //    scrollBarPosition = Math.Min(1, scrollBarPosition);
        //    SetScrollPosition();
        //}
        //private void SetScrollPosition()
        //{
        //    if (verticalScrollBar)
        //        scrollBar.bounds.Y = yPositionOnScreen + 115 + upArrow.bounds.Height + (int)((scrollBarRunner.Height - scrollBar.bounds.Height) * scrollBarPosition);
        //    else
        //        scrollBar.bounds.X = scrollBarRunner.X + 2 + (int)((scrollBarRunner.Width - scrollBar.bounds.Width) * scrollBarPosition);
        //}
        //public override void releaseLeftClick(int x, int y)
        //{
        //    scrolling = false;
        //    base.releaseLeftClick(x, y);
        //}
        //public override void leftClickHeld(int x, int y)
        //{
        //    base.leftClickHeld(x, y);
        //    if (scrolling)
        //    {
        //        if (verticalScrollBar)
        //        {
        //            int y2 = scrollBar.bounds.Y;
        //            scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
        //            float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
        //            displayIndex = Math.Min(displaySlots.Count - 3, Math.Max(0, (int)((float)displaySlots.Count * percentage)));
        //            //setScrollBarToCurrentIndex();
        //            if (y2 != scrollBar.bounds.Y)
        //            {
        //                Game1.playSound("shiny4");
        //            }
        //        }
        //        else
        //        {
        //            int x2 = scrollBar.bounds.X;
        //            scrollBar.bounds.X = Math.Min(xPositionOnScreen + width - 64 - 12 - 26 - scrollBar.bounds.Width, Math.Max(x, xPositionOnScreen + leftArrow.bounds.Width + 44));
        //            float percentage = (float)(x - scrollBarRunner.X) / (float)scrollBarRunner.Width;
        //            displayIndex = Math.Min(displaySlots.Count - 3, Math.Max(0, (int)((float)displaySlots.Count * percentage)));
        //            //setScrollBarToCurrentIndex();
        //            if (x2 != scrollBar.bounds.X)
        //            {
        //                Game1.playSound("shiny4");
        //            }

        //        }
        //    }
        //}
        //private void setScrollBarToCurrentIndex()
        //{
        //    if (displaySlots.Count > numberOfSlots)
        //    {
        //        if (verticalScrollBar)
        //        {
        //            scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, displaySlots.Count - 3) * displayIndex + upArrow.bounds.Bottom + 4;
        //            if (scrollBar.bounds.Y > downArrow.bounds.Y - scrollBar.bounds.Height - 4)
        //            {
        //                scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
        //            }
        //        }
        //        else
        //        {
        //            scrollBar.bounds.X = scrollBarRunner.Width / Math.Max(1, displaySlots.Count - numberOfSlots) * displayIndex + upArrow.bounds.Left + 4;
        //            if (scrollBar.bounds.X > downArrow.bounds.X - scrollBar.bounds.Width - 4)
        //            {
        //                scrollBar.bounds.X = downArrow.bounds.X - scrollBar.bounds.Width - 4;
        //            }
        //        }
        //    }
        //}
        //public override void receiveKeyPress(Keys key)
        //{
        //    bool handled = false;
        //    if (verticalScrollBar)
        //    {
        //        switch (key.ToSButton())
        //        {
        //            case SButton.Down:
        //                handled = true;
        //                downArrowPressed();
        //                break;
        //            case SButton.Up:
        //                handled = true;
        //                upArrowPressed();
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        switch (key.ToSButton())
        //        {
        //            case SButton.Left:
        //                handled = true;
        //                downArrowPressed();
        //                break;
        //            case SButton.Right:
        //                handled = true;
        //                upArrowPressed();
        //                break;
        //        }
        //    }
        //    if (!handled)
        //        base.receiveKeyPress(key);
        //}
        //public override void receiveLeftClick(int x, int y, bool playSound = true)
        //{
        //    bool handled = false;
        //    if (scrollBar.containsPoint(x, y))
        //    {
        //        scrolling = true;
        //    }
        //    else if (verticalScrollBar)
        //    {
        //        if (downArrow.containsPoint(x, y))
        //        {
        //            downArrowPressed();
        //        }
        //        else if (upArrow.containsPoint(x, y))
        //        {
        //            upArrowPressed();
        //        }
        //    }
        //    else
        //    {
        //        if (rightArrow.containsPoint(x, y))
        //        {
        //            upArrowPressed();
        //        }
        //        else if (leftArrow.containsPoint(x, y))
        //        {
        //            downArrowPressed();
        //        }
        //        else if (scrollBarRunner.Contains(x, y))
        //        {
        //            if (Game1.getMousePosition().X > scrollBar.bounds.X)
        //                upArrowPressed();
        //            else
        //                downArrowPressed();
        //        }
        //    }
        //    for (int panel = 0; panel < numberOfSlots; panel++)
        //    {
        //        int xPos = x - xPositionOnScreen - 100 - 380 * panel;
        //        int yPos = y - yPositionOnScreen - 100;
        //        if (displayIndex + panel < displaySlots.Count && displaySlots[displayIndex + panel].containsPoint(xPos, yPos))
        //        {
        //            displaySlots[displayIndex + panel].receiveLeftClick(xPos, yPos);
        //            break;
        //        }
        //    }
        //    if (!handled)
        //        base.receiveLeftClick(x, y, playSound);
        //}
        //public override void draw(SpriteBatch b)
        //{
        //    b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
        //    Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
        //    //Rectangle sourceRect = new Rectangle(64, 128, 64, 64);
        //    //int destSize = 40;
        //    //b.Draw(menuTexture, new Rectangle(xPositionOnScreen + destSize - 10, destSize - 10, width - 10 - destSize, destSize + 10), sourceRect, Color.White);
        //    //sourceRect.Y = 0;
        //    //sourceRect.X = 0;
        //    //b.Draw(menuTexture, new Rectangle(xPositionOnScreen + 10, 10, destSize, destSize), sourceRect, Color.White);
        //    //sourceRect.X = 192;
        //    //b.Draw(menuTexture, new Rectangle(xPositionOnScreen + width - destSize, 10, destSize, destSize), sourceRect, Color.White);
        //    //sourceRect.Y = 192;
        //    //b.Draw(menuTexture, new Rectangle(xPositionOnScreen + width - destSize, 10 + destSize + 10, destSize, destSize), sourceRect, Color.White);
        //    //sourceRect.X = 0;
        //    //b.Draw(menuTexture, new Rectangle(xPositionOnScreen + 10, 10 + destSize + 10, destSize, destSize), sourceRect, Color.White);
        //    //sourceRect.X = 128;
        //    //sourceRect.Y = 0;
        //    //b.Draw(menuTexture, new Rectangle(xPositionOnScreen + 10 + destSize, 10, width - destSize * 2 - 10, destSize), sourceRect, Color.White);
        //    //sourceRect.Y = 192;
        //    //b.Draw(menuTexture, new Rectangle(xPositionOnScreen + 10 + destSize, 20 + destSize, width - destSize * 2 - 10, destSize), sourceRect, Color.White);
        //    //sourceRect.Y = 128;
        //    //sourceRect.X = 0;
        //    //b.Draw(menuTexture, new Rectangle(xPositionOnScreen + 10, destSize, destSize, destSize), sourceRect, Color.White);
        //    //sourceRect.X = 192;
        //    //b.Draw(menuTexture, new Rectangle(xPositionOnScreen + width - destSize, destSize, destSize, destSize), sourceRect, Color.White);

        //    //b.End();
        //    //b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        //    for (int index = 0; index < numberOfSlots; index++)
        //    {
        //        if (displayIndex + index < displaySlots.Count)
        //            displaySlots[displayIndex + index].draw(b, xPositionOnScreen + (width - numberOfSlots * (listingWidth + 25)) / 2 + 380 * index, yPositionOnScreen + 100);
        //        else
        //            break;
        //    }

        //    if (!GameMenu.forcePreventClose)
        //    {
        //        if (verticalScrollBar)
        //        {
        //            upArrow.draw(b);
        //            downArrow.draw(b);
        //        }
        //        else
        //        {
        //            leftArrow.draw(b);
        //            rightArrow.draw(b);
        //        }
        //        if (displaySlots.Count > numberOfSlots)
        //        {
        //            drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
        //            scrollBar.draw(b);
        //        }
        //    }
        //    base.draw(b);
        //    drawMouse(b, ignore_transparency: true);
        //}
    }
}
