using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using CustomMenuFramework.Controls;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;


namespace CustomMenuFramework.Menus
{
    internal class IScrollingMenu : IClickableMenu
    {
        protected bool centerMenu = false;
        private ClickableTextureComponent upArrow;
        private ClickableTextureComponent downArrow;
        private ClickableTextureComponent leftArrow;
        private ClickableTextureComponent rightArrow;
        private ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private bool scrolling;
        private float scrollBarPosition = 0;
        private bool verticalScrollBar;
        private int numberOfSlots = 0;
        private int displayIndex = 0;
        protected Action<string>? onClose = null;
        protected string itemSelected = "";
        protected int listingWidth = 350;
        private int currentSlot = 0;
        private List<IScrollingControl> displaySlots = new();
        //private List<ClickableComponent> controls = new();
        private List<ClickableComponent> clickSlots = new();
        public IScrollingMenu(int x, int y, int width, int height, Action<string>? onClose = null, bool center = false, bool showUpperRightCloseButton = false, bool verticalScroll = true) : base(x, y, width, height, showUpperRightCloseButton)
        {
            this.onClose = onClose;
            centerMenu = center;
            verticalScrollBar = verticalScroll;
            populateClickableComponentList();
            LoadNavigationControls();
            int top = yPositionOnScreen + 84;// + 80 + 4;

            ResizeMenu();
        }

        internal void AddControlToDisplaySlots(IScrollingControl control)
        {
            displaySlots.Add(control);
            allClickableComponents.Add(control);
        }
        protected void ExitMenu()
        {
            if (this == Game1.activeClickableMenu)
            {
                Game1.exitActiveMenu();
                if (onClose != null)
                    onClose(itemSelected);
            }
        }

        private void LoadNavigationControls()
        {
            if (verticalScrollBar)
            {
                upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
                downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
                scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
                scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 128 - upArrow.bounds.Height - 8);
            }
            else
            {
                leftArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421 - 68, 472 + 22, 11, 12), 4f);
                rightArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421 - 66 + 11, 472 + 22, 11, 12), 4f);
                scrollBar = new ClickableTextureComponent(new Rectangle(leftArrow.bounds.X + 12, leftArrow.bounds.Y + leftArrow.bounds.Height + 4, 40, 20), Game1.mouseCursors, new Rectangle(420, 441, 10, 10), 4f);
                scrollBarRunner = new Rectangle(scrollBar.bounds.X, leftArrow.bounds.Y + leftArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 128 - leftArrow.bounds.Height - 8);
            }
        }
        private void ResizeMenu()
        {
            if (centerMenu)
            {
                xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
                yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).Y;// Game1.uiViewport.Height/2 - height/2 - 64;
            }
            if (upperRightCloseButton != null)
            {
                upperRightCloseButton.bounds = new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen - 8, 48, 48);
            }
            if (verticalScrollBar)
            {
                upArrow.setPosition(xPositionOnScreen + width - 90, yPositionOnScreen + 110);
                downArrow.setPosition(xPositionOnScreen + width - 90, yPositionOnScreen + height - 100);
                scrollBar.setPosition(xPositionOnScreen + width - 78, yPositionOnScreen + 160);
                scrollBarRunner.X = xPositionOnScreen + width - 78;
                scrollBarRunner.Y = yPositionOnScreen + 115 + upArrow.bounds.Height;
                scrollBarRunner.Height = height - 268;
                numberOfSlots = height / 80;
            }
            else
            {
                leftArrow.setPosition(xPositionOnScreen + 40, yPositionOnScreen + height - 96);
                rightArrow.setPosition(xPositionOnScreen + width - 100, yPositionOnScreen + height - 96);
                scrollBarRunner.X = xPositionOnScreen + 87;
                scrollBarRunner.Y = yPositionOnScreen + height - 84;
                scrollBarRunner.Height = 26;
                scrollBarRunner.Width = width - 190;
                scrollBar.setPosition(scrollBarRunner.X + 2, scrollBarRunner.Y);
                numberOfSlots = (int)Math.Floor((float)(width - 200) / listingWidth);
            }
            clickSlots = new();
            for (int slot = 0; slot < numberOfSlots; slot++)
            {
                clickSlots.Add(new ClickableComponent(new Rectangle
                (
                xPositionOnScreen + (width - numberOfSlots * (listingWidth + 25)) / 2 + 380 * slot, yPositionOnScreen + 100, 150, 40
                ), $"slot{slot}"));
            }
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            width = Math.Min(1920, Game1.uiViewport.Width) - 20;
            height = Math.Min(1080, Game1.uiViewport.Height) - 20;
            ResizeMenu();
        }
        public override void receiveScrollWheelAction(int direction)
        {
            if (!GameMenu.forcePreventClose)
            {
                base.receiveScrollWheelAction(direction);
                if (direction < 0 && scrollBarPosition > 0)
                {
                    downArrowPressed("shiny4");
                }
                else if (direction > 0 && scrollBarPosition < 1f)
                {
                    upArrowPressed("shiny4");
                }
                if (Game1.options.SnappyMenus)
                {
                    snapCursorToCurrentSnappedComponent();
                }
            }
        }
        private void downArrowPressed(string sound = "shwip")
        {
            Game1.playSound(sound);
            currentSlot--;
            if (displaySlots.Count > numberOfSlots && (!Game1.options.gamepadControls || currentSlot == -1))
            {
                displayIndex--;
                displayIndex = Math.Max(0, displayIndex);
                scrollBarPosition = (float)displayIndex / (displaySlots.Count - numberOfSlots);
                SetScrollPosition();
            }
            currentSlot = Math.Max(currentSlot, 0);

            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                currentlySnappedComponent = clickSlots[currentSlot];
                snapCursorToCurrentSnappedComponent();
            }
        }
        private void upArrowPressed(string sound = "shwip")
        {
            Game1.playSound(sound);
            currentSlot++;
             if (displaySlots.Count > numberOfSlots && (!Game1.options.gamepadControls || currentSlot == numberOfSlots))
            {
                displayIndex++;
                displayIndex = Math.Min(displaySlots.Count - numberOfSlots, displayIndex);
                scrollBarPosition = (float)displayIndex / (displaySlots.Count - numberOfSlots);
                scrollBarPosition = Math.Min(1, scrollBarPosition);
                SetScrollPosition();
            }
            currentSlot = Math.Min(currentSlot, numberOfSlots - 1);
             if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                currentlySnappedComponent = clickSlots[currentSlot];
                snapCursorToCurrentSnappedComponent();
            }
        }
        public override void snapCursorToCurrentSnappedComponent()
        {
            if (currentlySnappedComponent != null)
            {
                displaySlots[displayIndex].snapToMenu(currentlySnappedComponent.bounds.Right, currentlySnappedComponent.bounds.Top);
            }
        }
        public override void populateClickableComponentList()
        {
            allClickableComponents = new List<ClickableComponent>();
            allClickableComponents.AddRange(displaySlots);
        }
     
        private void SetScrollPosition()
        {
            if (verticalScrollBar)
                scrollBar.bounds.Y = yPositionOnScreen + 115 + upArrow.bounds.Height + (int)((scrollBarRunner.Height - scrollBar.bounds.Height) * scrollBarPosition);
            else
                scrollBar.bounds.X = scrollBarRunner.X + 2 + (int)((scrollBarRunner.Width - scrollBar.bounds.Width) * scrollBarPosition);
        }
        public override void releaseLeftClick(int x, int y)
        {
            scrolling = false;
            base.releaseLeftClick(x, y);
        }
        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (scrolling)
            {
                if (verticalScrollBar)
                {
                    int y2 = scrollBar.bounds.Y;
                    scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
                    float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
                    displayIndex = Math.Min(displaySlots.Count - 3, Math.Max(0, (int)((float)displaySlots.Count * percentage)));
                    //setScrollBarToCurrentIndex();
                    if (y2 != scrollBar.bounds.Y)
                    {
                        Game1.playSound("shiny4");
                    }
                }
                else
                {
                    int x2 = scrollBar.bounds.X;
                    scrollBar.bounds.X = Math.Min(xPositionOnScreen + width - 64 - 12 - 26 - scrollBar.bounds.Width, Math.Max(x, xPositionOnScreen + leftArrow.bounds.Width + 44));
                    float percentage = (float)(x - scrollBarRunner.X) / (float)scrollBarRunner.Width;
                    displayIndex = Math.Min(displaySlots.Count - 3, Math.Max(0, (int)((float)displaySlots.Count * percentage)));
                    //setScrollBarToCurrentIndex();
                    if (x2 != scrollBar.bounds.X)
                    {
                        Game1.playSound("shiny4");
                    }

                }
            }
        }
        private void setScrollBarToCurrentIndex()
        {
            if (displaySlots.Count > numberOfSlots)
            {
                if (verticalScrollBar)
                {
                    scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, displaySlots.Count - 3) * displayIndex + upArrow.bounds.Bottom + 4;
                    if (scrollBar.bounds.Y > downArrow.bounds.Y - scrollBar.bounds.Height - 4)
                    {
                        scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                    }
                }
                else
                {
                    scrollBar.bounds.X = scrollBarRunner.Width / Math.Max(1, displaySlots.Count - numberOfSlots) * displayIndex + upArrow.bounds.Left + 4;
                    if (scrollBar.bounds.X > downArrow.bounds.X - scrollBar.bounds.Width - 4)
                    {
                        scrollBar.bounds.X = downArrow.bounds.X - scrollBar.bounds.Width - 4;
                    }
                }
            }
        }
        public override void receiveKeyPress(Keys key)
        {
            bool handled = false;
            if (verticalScrollBar)
            {
                switch (key.ToSButton())
                {
                    case SButton.Down:
                        handled = true;
                        downArrowPressed();
                        break;
                    case SButton.Up:
                        handled = true;
                        upArrowPressed();
                        break;
                }
            }
            else
            {
                switch (key.ToSButton())
                {
                    case SButton.Left:
                    case SButton.A:
                        handled = true;
                        downArrowPressed();
                        break;
                    case SButton.Right:
                    case SButton.D:
                        handled = true;
                        upArrowPressed();
                        break;
                }
            }
            if (!handled)
                base.receiveKeyPress(key);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            bool handled = false;
            if (scrollBar.containsPoint(x, y))
            {
                scrolling = true;
            }
            else if (verticalScrollBar)
            {
                if (downArrow.containsPoint(x, y))
                {
                    downArrowPressed();
                }
                else if (upArrow.containsPoint(x, y))
                {
                    upArrowPressed();
                }
            }
            else
            {
                if (rightArrow.containsPoint(x, y))
                {
                    upArrowPressed();
                }
                else if (leftArrow.containsPoint(x, y))
                {
                    downArrowPressed();
                }
                else if (scrollBarRunner.Contains(x, y))
                {
                    if (Game1.getMousePosition().X > scrollBar.bounds.X)
                        upArrowPressed();
                    else
                        downArrowPressed();
                }
            }
            for (int panel = 0; panel < numberOfSlots; panel++)
            {
                int xPos = x - xPositionOnScreen - 100 - 380 * panel;
                int yPos = y - yPositionOnScreen - 100;
                if (displayIndex + panel < displaySlots.Count && displaySlots[displayIndex + panel].containsPoint(xPos, yPos))
                {
                    displaySlots[displayIndex + panel].receiveLeftClick(xPos, yPos);
                    break;
                }
            }
            if (!handled)
                base.receiveLeftClick(x, y, playSound);
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
            for (int index = 0; index < Math.Min(displaySlots.Count, numberOfSlots); index++)
            {
                displaySlots[index].visible = false;
            }
            for (int index = 0; index < numberOfSlots; index++)
            {
                if (displayIndex + index < displaySlots.Count)
                {
                    Point position = new Point(xPositionOnScreen + (width - numberOfSlots * (listingWidth + 25)) / 2 + 380 * index, yPositionOnScreen + 100);
                    displaySlots[displayIndex + index].draw(b, position.X, position.Y);
                    displaySlots[displayIndex + index].visible = true;
#if DEBUG
                    b.DrawString(Game1.smallFont, $"{displaySlots[displayIndex + index].upNeighborID}", new Vector2(position.X + displaySlots[displayIndex + index].bounds.Width / 2 - 20, position.Y + 5), Color.White);
                    b.DrawString(Game1.smallFont, $"{displaySlots[displayIndex + index].downNeighborID}", new Vector2(position.X + displaySlots[displayIndex + index].bounds.Width / 2 - 20, position.Y + displaySlots[displayIndex + index].bounds.Height - 20), Color.White);
                    b.DrawString(Game1.smallFont, $"{displaySlots[displayIndex + index].leftNeighborID}", new Vector2(position.X + 5, position.Y + displaySlots[displayIndex + index].bounds.Height / 2 - 10), Color.Red);
                    b.DrawString(Game1.smallFont, $"{displaySlots[displayIndex + index].rightNeighborID}", new Vector2(position.X + displaySlots[displayIndex + index].bounds.Width - 40, position.Y + displaySlots[displayIndex + index].bounds.Height / 2 + 10), Color.Red);

                    b.DrawString(Game1.smallFont, $"{displaySlots[displayIndex + index].myID}", new Vector2(position.X + displaySlots[displayIndex + index].bounds.Width / 2 - 40, position.Y + displaySlots[displayIndex + index].bounds.Height / 2 + 10), Color.Red);
#endif
                }
                else
                    break;
            }

            if (!GameMenu.forcePreventClose)
            {
                if (displaySlots.Count > numberOfSlots)
                {
                    if (verticalScrollBar && !Game1.options.snappyMenus)
                    {
                        upArrow.visible = true;
                        downArrow.visible = true;
                        upArrow.draw(b);
                        downArrow.draw(b);
                    }
                    else if (!Game1.options.snappyMenus)
                    {
                        leftArrow.draw(b);
                        rightArrow.draw(b);
                        leftArrow.visible = true;
                        rightArrow.visible = true;
                    }
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
                    scrollBar.draw(b);
                    scrollBar.visible = true;
                }
                else
                {
                    scrollBar.visible = false;
                    if (verticalScrollBar)
                    {
                        upArrow.visible = false;
                        downArrow.visible = false;
                    }
                    else
                    {
                        leftArrow.visible = false;
                        rightArrow.visible = false;
                    }
                }
            }
            base.draw(b);
            drawMouse(b, ignore_transparency: true);
        }
    }
}
