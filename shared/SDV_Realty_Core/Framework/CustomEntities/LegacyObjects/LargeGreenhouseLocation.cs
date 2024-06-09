

namespace SDV_Realty_Core.Framework.Buildings.Greenhouses
{
    /// <summary>
    /// For load legacy saves only (1.5.6)
    /// </summary>
    public class LargeGreenhouseLocation : GameLocation
    {
        public LargeGreenhouseLocation() : base()
        {
            IsGreenhouse = true;
            base.IsGreenhouse = true;
            IsOutdoors = false;
        }
        public LargeGreenhouseLocation(string path, string id) : base(path, id)
        {
            IsGreenhouse = true;
            base.IsGreenhouse = true;
            IsOutdoors = false;
        }
        public new bool IsGreenhouse { get { return true; } set { } }
        public new bool SeedsIgnoreSeasonsHere()
        {
            return true;
        }
    }
}
