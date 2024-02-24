using StardewValley.GameData.Locations;


namespace SDV_Realty_Core.Framework.Objects
{
    /// <summary>
    /// Tracks player customizations of farm expansions
    /// </summary>
    internal partial class CustomizationDetails
    {
        //
        // version 1.6
        //    
        public double? GetDirtDecayChance { get; set; } = null;

        public LocationData GetLocationData()
        {
            LocationData lData = new LocationData
            {
                DisplayName = ExpansionName
            };
            if (FishAreas != null)
            {
                foreach (string area in FishAreas.Keys)
                {
                    SpawnFishData spData = new SpawnFishData
                    {
                        FishAreaId = area
                    };
                    foreach (var fish in FishAreas[area].StockData)
                    {
                        spData.RandomItemId.Add(fish.FishId);
                    }
                    lData.Fish.Add(spData);
                }
            }
            return lData;
        }
    }
}
