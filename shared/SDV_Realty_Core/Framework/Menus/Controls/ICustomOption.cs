using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDV_Realty_Core.Framework.Menus.Controls
{
    internal class ICustomOption : OptionsElement
    {
        public int controlId;

        public ICustomOption(string label, Rectangle bounds, int whichOption=-1, int controlId = -1) :this(label,bounds.X,bounds.Y,bounds.Width,bounds.Height,whichOption,controlId)
        {

        }
        public ICustomOption(string label, int x, int y, int whichOption = -1, int controlId = -1) :this(label,x,y,0,0,whichOption,controlId)
        {

        }
        public ICustomOption(string label, int x, int y, int width, int height, int whichOption = -1, int controlId = -1) : base(label, x, y, width, height, whichOption)
        {
            this.controlId= controlId;
        }
    }
}
