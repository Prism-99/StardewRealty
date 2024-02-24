using Prism99_Core.Utilities;
using System.Collections.Generic;
using System.Linq;


namespace StardewRealty.SDV_Realty_Interface
{
    public partial class ExpansionDetails
    {
        public ExpansionDetails() { }

        public Point?CaveOut { get; set; } = null;
        public bool? AllowGiantCrops { get; set; }
        public string BaseDefinition {  get; set; }
        public bool? AlwaysSnowing { get; set; }
        public bool? AlwaysRaining { get; set; } 
        public bool? AlwaysSunny { get; set; } 
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
