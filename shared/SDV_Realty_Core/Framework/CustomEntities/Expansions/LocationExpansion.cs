using System;
using System.Collections.Generic;
using System.Text;
using StardewValley;

namespace SDV_Realty_Core.Framework.Expansions
{
    [Serializable]
    public class LocationExpansion : GameLocation, BaseExpansion
    {
        public bool Active { get; set; }
        public bool AutoAdd { get; set; }
        public int GridId { get; set; }
    }
}
