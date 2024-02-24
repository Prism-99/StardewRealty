
using System.Collections.Generic;

namespace StardewRealty.SDV_Realty_Interface
{
    internal partial class SDRCustomization
    {
        public SDRCustomization() { }

        public Dictionary<string, FishAreaDetails> FishAreas { get; set; }
        public string ExpansionName { get; set; }
        public string SpringForage { get; set; }
        public string SummerForage { get; set; }
        public string FallForage { get; set; }
        public string WinterForage { get; set; }
        public string SpringFish { get; set; }
        public string SummerFish { get; set; }
        public string FallFish { get; set; }
        public string WinterFish { get; set; }
        public List<ArtifactData> Artifacts { get; set; }
        public bool StockedPonds { get; set; }
        public string Season { get; set; }
        public bool? AlwaysRaining { get; set; }
        public bool? AlwaysSnowing { get; set; }
        public bool? AlwaysSunny { get; set; }
        public bool? AllowGiantCrops { get; set; }
     }
}
