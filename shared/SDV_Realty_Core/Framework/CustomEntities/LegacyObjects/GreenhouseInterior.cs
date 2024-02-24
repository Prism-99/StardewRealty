using System;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
#if v16
#else
using StardewValley;
#endif

namespace SDV_Realty_Core.Framework.Buildings.Greenhouses
{
    [Serializable]
    public class GreenhouseInterior:Building
    {
        public GreenhouseInterior() { }
#if v16
        public GreenhouseInterior(string type, Vector2 loc) : base(type, loc) { }
#else
        public GreenhouseInterior(BluePrint blueprint, Vector2 loc) : base(blueprint, loc) { }
#endif
    }
}
