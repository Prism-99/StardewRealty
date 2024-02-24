using System;
#if v16
#else
using StardewValley;
#endif
using StardewValley.Buildings;
using Microsoft.Xna.Framework;

namespace SDV_Realty_Core.Framework.Buildings.Greenhouses
{
    [Serializable]
    public class LargeGreenhouseInterior:Building
    {
        public LargeGreenhouseInterior() { }
#if v16
        public LargeGreenhouseInterior(string type, Vector2 loc) : base(type, loc) { }
#else
        public LargeGreenhouseInterior(BluePrint blueprint, Vector2 loc) : base(blueprint, loc) { }
#endif
    }
}
