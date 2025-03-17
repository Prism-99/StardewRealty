using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SDV_Realty_Core.Framework.Menus.Controls;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDV_Realty_Core.Framework.Menus
{
    internal class CustomMenu : IClickableMenu
    {
        public bool transitionInitialized;
        //public int x;
        //public int y;
        private string hoverText = "";
        private int selectedControl = -1;
        private int hoverControl = -1;
        private List<ICustomOption> controls;
        private List<OptionsElement> labels;
        public CustomMenu(int x, int y, int width, int height, bool center = false, bool showUpperRightCloseButton = false) : base(x, y, width, height, showUpperRightCloseButton)
        {
            if (center)
            {
                xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(width, height).X;
                yPositionOnScreen = Game1.uiViewport.Height - height - 64;
            }
            if (showUpperRightCloseButton && upperRightCloseButton != null)
            {
                upperRightCloseButton.bounds = new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen - 8, 48, 48);
            }
            transitionInitialized = true;
        }
        public CustomMenu(int x, int y, int width, int height, List<ICustomOption> controls, List<OptionsElement> labels, bool center = false, bool showUpperRightCloseButton = false) : this(x, y, width, height, showUpperRightCloseButton)
        {
            this.controls = controls;
            this.labels = labels;
            
        }
        public void SetControls(List<ICustomOption> controls, List<OptionsElement> labels)
        {
            this.controls=controls;
            this.labels=labels;
        }
        public override void releaseLeftClick(int x, int y)
        {
            if (selectedControl > -1)
            {
                controls[selectedControl].leftClickReleased(x - xPositionOnScreen, y - yPositionOnScreen);
                //selectedControl = -1;
            }
            //foreach (var control in controls)
            //{
            //    //if (control.bounds.Contains(x - xPositionOnScreen, y - yPositionOnScreen))
            //    //{
            //    control.leftClickReleased(x - xPositionOnScreen, y - yPositionOnScreen);
            //    //break;
            //    //}
            //}
            base.releaseLeftClick(x, y);
        }
        public override void leftClickHeld(int x, int y)
        {
            if (selectedControl > -1)
            {
                controls[selectedControl].leftClickHeld(x - xPositionOnScreen, y - yPositionOnScreen);
            }
            //foreach (var control in controls)
            //{
            //    if (control.bounds.Contains(x - xPositionOnScreen, y - yPositionOnScreen))
            //    {
            //        control.leftClickHeld(x - xPositionOnScreen, y - yPositionOnScreen);
            //        break;
            //    }
            //}
            base.leftClickHeld(x, y);
        }
        public override void performHoverAction(int x, int y)
        {
            hoverControl = -1;
            for (int index = 0; index < controls.Count; index++)
            {
                if (controls[index].bounds.Contains(x - xPositionOnScreen, y - yPositionOnScreen))
                {
                    hoverControl = index;
                    break;
                }
            }
            base.performHoverAction(x, y);
        }
        public override void receiveKeyPress(Keys key)
        {
            if (selectedControl > -1)
            {
                controls[selectedControl].receiveKeyPress(key);
            }
            else
                base.receiveKeyPress(key);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (selectedControl > -1 && controls[selectedControl] is TextEntryOption text)
            {
                text.LostFocus();
            }
            selectedControl = -1;
            for (int index = 0; index < controls.Count; index++)
            {
                if (controls[index].bounds.Contains(x - xPositionOnScreen, y - yPositionOnScreen))
                {
                    controls[index].receiveLeftClick(x - xPositionOnScreen, y - yPositionOnScreen);
                    selectedControl = index;
                    break;
                }
            }

            //foreach (var control in controls) 
            //{
            //    if (control.bounds.Contains(x- xPositionOnScreen, y  - yPositionOnScreen)) 
            //    { 
            //        control.receiveLeftClick(x  - xPositionOnScreen, y  - yPositionOnScreen); 
            //        break;
            //    }
            //}
            if (selectedControl == -1)
                base.receiveLeftClick(x, y, playSound);
            //if (Game1.activeClickableMenu.Equals(this))
            //{
            //    Game1.exitActiveMenu();
            //    Game1.dialogueUp = false;
            //}
        }
        public bool TryGetControl(int controlId,out  ICustomOption? control)
        {
            control= null;

            var selected=controls.Where(p=>p.controlId == controlId);
            if (selected.Any())
            {
                control= selected.FirstOrDefault();
                return true;
            }

            return false;
        }
        public override void draw(SpriteBatch b)
        {
            //drawBox(b, x, y , width, 200);
            Game1.DrawBox(xPositionOnScreen, yPositionOnScreen, width, height);

            foreach (var control in controls)
            {
                //control.draw(b, control.bounds.X + xPositionOnScreen, control.bounds.Y + yPositionOnScreen);
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
            if (hoverControl > -1)
                b.DrawString(Game1.smallFont, $"{hoverControl}", new Vector2(10, 10), Color.White);
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
