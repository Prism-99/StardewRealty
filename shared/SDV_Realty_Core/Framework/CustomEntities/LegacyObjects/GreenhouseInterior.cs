using System;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
#if v16
#else
using StardewValley;
#endif

namespace SDV_Realty_Core.Framework.Buildings.Greenhouses
{
    /// <summary>
    /// For loading legacy saves only (1.5.6)
    /// </summary>
    [Serializable]
    public class GreenhouseInterior:Building
    {
        public GreenhouseInterior() { }

        public GreenhouseInterior(string type, Vector2 loc) : base(type, loc) { }
    }
}
