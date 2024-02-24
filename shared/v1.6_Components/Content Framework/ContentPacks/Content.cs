using System.Collections.Generic;
using StardewValley.GameData.Locations;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
    internal partial class ExpansionPack
    {
        //
        //  version 1.6
        //
        public bool MineCartEnabled { get; set; }
        public List<Point> MineCartActionPoints { get; set; }
        public string MineCartDisplayName { get; set; }
        public string MineCartDirection { get; set; }
        public double DirtDecayChance { get; set; } = 0.1;
        //public Dictionary<string,FishAreaData> FishAreas { get; set; }
        //
        //  fish data keyed to the FishAreas
        //
        public Dictionary<string,List<SpawnFishData>> FishData { get; set; }
      }
}
