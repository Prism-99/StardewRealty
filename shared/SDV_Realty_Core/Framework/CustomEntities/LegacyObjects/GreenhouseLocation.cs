using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;



namespace SDV_Realty_Core.Framework.Buildings.Greenhouses
{
    /// <summary>
    /// Used for legacy saves only (1.5.6)
    /// </summary>
    public class GreenhouseLocation : GameLocation
    {
        public GreenhouseLocation() : base()
        {
            IsGreenhouse = true;
            base.IsGreenhouse = true;
            IsOutdoors = false;
        }
        public GreenhouseLocation(string path, string id) : base(path, id)
        {
            IsGreenhouse = true;
            base.IsGreenhouse = true;
            IsOutdoors = false;
        }
        public new bool IsGreenhouse { get { return true; } set { } }
        public override void updateWarps()
        {
            base.updateWarps();
            if (modData.ContainsKey(IModDataKeysService.FELocationName))
            {
                //FEFramework.FixWarps(this, modData[IModDataKeysService.FELocationName]);
            }
        }   
        public new bool SeedsIgnoreSeasonsHere()
        {
            return true;
        }
    }
}
