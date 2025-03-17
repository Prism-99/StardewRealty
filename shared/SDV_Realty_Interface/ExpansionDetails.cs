using Prism99_Core.Utilities;
using System.Collections.Generic;
using System.Linq;


namespace StardewRealty.SDV_Realty_Interface
{
    public partial class ExpansionDetails
    {
        public struct TreasureAreaItem
        {
            public string ItemId { get; set; }
            public string Condition { get; set; }
            public int Quantity { get; set; }
        }
        public struct TreasureArea
        {
            public Rectangle Area { get; set; }
            public List<TreasureAreaItem> Items { get; set; }
            public int MaxItems { get; set; }
        }
        public ExpansionDetails() { }

        public Point?CaveOut { get; set; } = null;
        public Vector2?ShippingBin { get; set; } = null;
        public bool? AllowGiantCrops { get; set; }
        public string BaseDefinition {  get; set; }
        public bool? AlwaysSnowing { get; set; }
        public bool? AlwaysRaining { get; set; } 
        public bool? AlwaysSunny { get; set; } 
        public bool AllowGrassGrowInWinter { get; set; }
        public bool AllowGrassSurviveInWinter { get; set; }
        public bool skipWeedGrowth { get; set; }
        public bool SpawnGrassFromPathsOnNewYear { get; set; }
        public bool SpawnRandomGrassOnNewYear { get; set; }
        public string Treasure { get; set; }
        public Dictionary<string, TreasureArea> TreasureSpots { get; set; }
        public bool EnableGrassSpread { get; set; }
        public SDVModVersion Format { get; set; }
        public string Requirements { get; set; }
        public string MapName { get; set; }
        public string LocationName { get; set; }
        public string DisplayName { get; set; }
        public int Cost { get; set; }
        public bool StockedPonds { get; set; }
        public bool? CrowsEnabled { get; set; }
        public string SeasonOverride { get; set; }
        public string Vendor { get; set; }
        public string Description { get; set; }
        public string ThumbnailName { get; set; }
        public bool Active { get; set; }
        public bool ForSale { get; set; }
        public long PurchasedBy {  get; set; }
        public int PurchaseDate { get; set; }
        public List<ArtifactData> Artifacts { get; set; }
        public List<ForageData> ForageData { get; set; }
        public string SpringForage { get; set; }
        public string SummerForage { get; set; }
        public string FallForage { get; set; }
        public string WinterForage { get; set; }
        public Dictionary<string, FishAreaDetails> FishAreas { get; set; }
        public List<ForageData> ForageBySeason(string season)
        {
            return ForageData.Where(p => p.Season.ToLower() == season.ToLower()).ToList();
        }

    }
}
