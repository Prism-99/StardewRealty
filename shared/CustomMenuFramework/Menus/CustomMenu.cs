using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;
using CustomMenuFramework.Controls;

namespace CustomMenuFramework.Menus
{
    internal class CustomMenu : IClickableMenu
    {
        public bool transitionInitialized;
        private int? snapControl = null;
        private string hoverText = "";
        private int selectedControl = -1;
        private int heldControl = -1;
        private int hoverControl = -1;
        private int defaultControlId = -1;
        private ICustomOption? activeControl = null;
        private List<ICustomOption> controls;
        private List<OptionsElement> labels;
        private bool centerMenu = false;
        public CustomMenu(int x, int y, int width, int height, bool center = false, bool showUpperRightCloseButton = false) : base(x, y, width, height, showUpperRightCloseButton)
        {
            centerMenu = center;
            ResizeMenu();
            transitionInitialized = true;
        }
        public CustomMenu(int x, int y, int width, int height, List<ICustomOption> controls, List<OptionsElement> labels, bool center = false, bool showUpperRightCloseButton = false) : this(x, y, width, height, center, showUpperRightCloseButton)
        {
            SetControls(controls, labels);
        }
        public void SetControls(List<ICustomOption> controls, List<OptionsElement> labels, int defaultControlId = -1)
        {
            this.controls = controls;
            this.labels = labels;
            this.defaultControlId = defaultControlId;
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
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            ResizeMenu();
        }
        public override void releaseLeftClick(int x, int y)
        {
            if (heldControl > -1)
            {
                controls[heldControl].leftClickReleased(x - xPositionOnScreen, y - yPositionOnScreen);
                heldControl = -1;
            }
            base.releaseLeftClick(x, y);
        }
        public override void leftClickHeld(int x, int y)
        {
            if (heldControl > -1)
            {
                controls[heldControl].leftClickHeld(x - xPositionOnScreen, y - yPositionOnScreen);
            }

            base.leftClickHeld(x, y);
        }
        public override void performHoverAction(int x, int y)
        {
#if DEBUG
            hoverControl = -1;
            for (int index = 0; index < controls.Count; index++)
            {
                if (controls[index].bounds.Contains(x - xPositionOnScreen, y - yPositionOnScreen))
                {
                    hoverControl = controls[index].controlId;
                    break;
                }
            }
#endif
            base.performHoverAction(x, y);
        }
        public override void receiveKeyPress(Keys key)
        {
            if (activeControl == null || !(activeControl is TextEntryOption))
            {
                if (selectedControl > -1)
                {
                    controls[selectedControl].receiveKeyPress(key);
                }
                else
                    if (!snapControl.HasValue)
                {
                    snapControl = defaultControlId;// 400;
                    SnapToCurrentControl();
                }
                else
                {
                    switch (key)
                    {
                        case Keys.A:
                            if (TryGetControl(snapControl.Value, out ICustomOption? acontrol))
                            {
                                if (acontrol.leftNeighborID != -1)
                                {
                                    snapControl = acontrol.leftNeighborID;
                                    SnapToCurrentControl();
                                }
                            }
                            break;
                        case Keys.D:
                            if (TryGetControl(snapControl.Value, out ICustomOption? dcontrol))
                            {
                                if (dcontrol.rightNeighborID != -1)
                                {
                                    snapControl = dcontrol.rightNeighborID;
                                    SnapToCurrentControl();
                                }
                            }
                            break;
                        case Keys.S:
                            if (TryGetControl(snapControl.Value, out ICustomOption? scontrol))
                            {
                                if (scontrol.downNeighborID != -1)
                                {
                                    snapControl = scontrol.downNeighborID;
                                    SnapToCurrentControl();
                                }
                            }
                            break;
                        case Keys.W:
                            if (TryGetControl(snapControl.Value, out ICustomOption? wcontrol))
                            {
                                if (wcontrol.upNeighborID != -1)
                                {
                                    snapControl = wcontrol.upNeighborID;
                                    SnapToCurrentControl();
                                }
                            }
                            break;
                        default:
                            base.receiveKeyPress(key);
                            break;
                    }
                }
            }
        }
        private void SnapToCurrentControl()
        {
            if (snapControl.HasValue && TryGetControl(snapControl.Value, out ICustomOption control))
            {
                Game1.setMousePosition(xPositionOnScreen + control.bounds.X+ control.bounds.Width/2, yPositionOnScreen + control.bounds.Y+control.bounds.Height/2);
            }
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (selectedControl > -1 && controls[selectedControl] is TextEntryOption text)
            {
                text.LostFocus();
            }
            activeControl = null;
            selectedControl = -1;
            heldControl = -1;
            for (int index = 0; index < controls.Count; index++)
            {
                if (controls[index].bounds.Contains(x - xPositionOnScreen, y - yPositionOnScreen))
                {
                    controls[index].receiveLeftClick(x - xPositionOnScreen, y - yPositionOnScreen);
                    //if (controls[index] is TextEntryOption)
                    //    selectedControl = index;
                    heldControl = index;
                    snapControl = controls[index].controlId;
                    activeControl = controls[index];
                    break;
                }
            }

            if (selectedControl == -1)
                base.receiveLeftClick(x, y, playSound);
            //if (Game1.activeClickableMenu.Equals(this))
            //{
            //    Game1.exitActiveMenu();
            //    Game1.dialogueUp = false;
            //}
        }
        public bool TryGetControl(int controlId, out ICustomOption? control)
        {
            control = null;

            var selected = controls.Where(p => p.controlId == controlId);
            if (selected.Any())
            {
                control = selected.FirstOrDefault();
                return true;
            }

            return false;
        }
        public override void draw(SpriteBatch b)
        {
            Game1.DrawBox(xPositionOnScreen, yPositionOnScreen, width, height);

            foreach (var control in controls)
            {
                control.draw(b, xPositionOnScreen, yPositionOnScreen);
            }
            foreach (var label in labels)
            {
                label.draw(b, label.bounds.X + xPositionOnScreen, label.bounds.Y + yPositionOnScreen);
            }
            string text = hoverText;
            if (text != null && text.Length > 0)
            {
                drawHoverText(b, hoverText, Game1.dialogueFont);
            }
            base.draw(b);
            drawMouse(b);
#if DEBUG
            if (snapControl.HasValue || hoverControl > -1)
                b.DrawString(Game1.smallFont, $"{snapControl.Value},{hoverControl}", new Vector2(10, 10), Color.White);
#endif
        }
        public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
        {
            if (transitionInitialized)
            {
                b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle(306, 320, 16, 16), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle(275, 313, 1, 6), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle(275, 328, 1, 8), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White);
                b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            }
        }
    }
}
