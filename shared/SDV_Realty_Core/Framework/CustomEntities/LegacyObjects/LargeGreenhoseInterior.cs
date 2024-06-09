using System;
using StardewValley.Buildings;


namespace SDV_Realty_Core.Framework.Buildings.Greenhouses
{
    /// <summary>
    /// For loading legacy saves (1.5.6)
    /// </summary>
    [Serializable]
    public class LargeGreenhouseInterior:Building
    {
        public LargeGreenhouseInterior() { }
        public LargeGreenhouseInterior(string type, Vector2 loc) : base(type, loc) { }
    }
}
