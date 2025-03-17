using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.Framework.MenuOptions;
using SDV_Realty_Core.Framework.Objects;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.Menus
{
    internal class ExpansionOptionsMenu : IClickableMenu
    {
        private ClickableTextureComponent upArrow;

        private ClickableTextureComponent downArrow;

        private ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private bool scrolling;
        public List<OptionsElement> options = new List<OptionsElement>();
        public List<ClickableComponent> optionSlots = new List<ClickableComponent>();
        public int currentItemIndex = 0;
        private int optionsSlotHeld = -1;
        private GameLocationCustomizations customizations;
        public ExpansionOptionsMenu(int x, int y, int width, int height, bool showCloseButton, GameLocationCustomizations customizations) : base(x, y, width, height, showCloseButton)
        {
            this.customizations = customizations;
            upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
            downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
            scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 128 - upArrow.bounds.Height - 8);
            for (int i = 0; i < 7; i++)
            {
                optionSlots.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 80 + 4 + i * ((height - 128) / 7) + 16, width - 32, (height - 128) / 7 + 4), i.ToString() ?? "")
                {
                    myID = i,
                    downNeighborID = ((i < 6) ? (i + 1) : (-7777)),
                    upNeighborID = ((i > 0) ? (i - 1) : (-7777)),
                    fullyImmutable = true
                });
            }
            options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11234"), 0));
            options.Add(new DropDownOption("Season Override",GetOptionsList(1),customizations.SeasonOverride, 1, OptionPicked));

        }
        private void OptionPicked(int option,string value)
        {
            switch (option) 
            {
                case 1:
                    customizations.SeasonOverride = value;
                    break;
            }
            customizations.SaveCustomizations();
        }
        private Dictionary<string, string> GetOptionsList(int optionId)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            switch (optionId)
            {
                case 1:
                    // Seasons
                    results.Add("none", "None");
                    results.Add("Spring", "Spring");
                    results.Add("Summer", "Summer");
                    results.Add("Fall", "Fall");
                    results.Add("Winter", "Winter");
                    break;
            };

            return results;
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            currentItemIndex = Math.Max(0, Math.Min(options.Count - 7, currentItemIndex));
            //UnsubscribeFromSelectedTextbox();
            bool handled = false;
            for (int i = 0; i < optionSlots.Count; i++)
            {
                if (optionSlots[i].bounds.Contains(x, y) && currentItemIndex + i < options.Count && options[currentItemIndex + i].bounds.Contains(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y))
                {
                    options[currentItemIndex + i].receiveLeftClick(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y);
                    handled = true;
                    optionsSlotHeld = i;
                    break;
                }
            }
            if (!handled)
                base.receiveLeftClick(x, y, playSound);
        }
        public override void releaseLeftClick(int x, int y)
        {
            if (!GameMenu.forcePreventClose)
            {
                base.releaseLeftClick(x, y);
                if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count)
                {
                    options[currentItemIndex + optionsSlotHeld].leftClickReleased(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
                }
                optionsSlotHeld = -1;
                scrolling = false;
            }
        }
        public override void leftClickHeld(int x, int y)
        {
            if (GameMenu.forcePreventClose)
            {
                return;
            }
            base.leftClickHeld(x, y);
            if (scrolling)
            {
                int y2 = scrollBar.bounds.Y;
                scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
                float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
                currentItemIndex = Math.Min(options.Count - 7, Math.Max(0, (int)((float)options.Count * percentage)));
                setScrollBarToCurrentIndex();
                if (y2 != scrollBar.bounds.Y)
                {
                    Game1.playSound("shiny4");
                }
            }
            else if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count)
            {
                options[currentItemIndex + optionsSlotHeld].leftClickHeld(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
            }
        }
        private void setScrollBarToCurrentIndex()
        {
            if (options.Count > 0)
            {
                scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, options.Count - 7 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
                if (scrollBar.bounds.Y > downArrow.bounds.Y - scrollBar.bounds.Height - 4)
                {
                    scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                }
            }
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
        
            for (int i = 0; i < optionSlots.Count; i++)
            {
                if (currentItemIndex >= 0 && currentItemIndex + i < options.Count)
                {
                    options[currentItemIndex + i].draw(b, optionSlots[i].bounds.X, optionSlots[i].bounds.Y, this);
                }
            }
            if (!GameMenu.forcePreventClose)
            {
                upArrow.draw(b);
                downArrow.draw(b);
            }
            base.draw(b);
            drawMouse(b, ignore_transparency: true);
        }
        public virtual void UnsubscribeFromSelectedTextbox()
        {
            if (Game1.keyboardDispatcher.Subscriber == null)
            {
                return;
            }
            foreach (OptionsElement option in options)
            {
                if (option is OptionsTextEntry entry && Game1.keyboardDispatcher.Subscriber == entry.textBox)
                {
                    Game1.keyboardDispatcher.Subscriber = null;
                    break;
                }
            }
        }
    }
}
