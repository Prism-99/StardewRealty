
namespace SDV_Realty_Core.Framework.Objects
{
    internal class TextScroller
    {
        private int curIndex = 0;
        private string displayText="";
        private int lastTick = -1;
        private int scrollDelay = 50;
        private int maxCharsDisplayed = -1;
        public TextScroller() { }
        public TextScroller(int maxChars)
        {
            this.maxCharsDisplayed = maxChars;
        }
        public TextScroller(string text, int delay, int maxChars)
        {
            DisplayText = text;
            scrollDelay = delay;
            maxCharsDisplayed = maxChars;
        }

        public string DisplayText
        {
            get
            {
                return displayText;
            }

            set
            {
                displayText = value;
                curIndex= 0;
                lastTick = -1;
            }
        }
        public void TickUpdate(int currentTick)
        {
            if (DisplayText.Length > maxCharsDisplayed)
            {
                if (lastTick == -1)
                {
                    lastTick = currentTick;
                }
                else
                {
                    if (currentTick - lastTick > scrollDelay)
                    {
                        curIndex++;
                        if (curIndex > DisplayText.Length - maxCharsDisplayed)
                        {
                            curIndex = 0;
                        }
                        lastTick = currentTick;
                    }
                }
            }
        }

        public string GetText()
        {
            if (DisplayText.Length > maxCharsDisplayed)
            {
                return DisplayText.Substring(curIndex, maxCharsDisplayed);
            }
            else
            {
                return DisplayText;
            }
        }
    }
}
