using System.Collections.Generic;
using StardewValley.GameData.Locations;
using static StardewRealty.SDV_Realty_Interface.ExpansionDetails;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
    internal partial class ExpansionPack
    {
        public struct MineCartSpot
        {
            public bool MineCartEnabled { get; set; }
            public List<Point> MineCartActionPoints { get; set; }
            public string MineCartDisplayName { get; set; }
            public string MineCartDirection { get; set; }
            public EntranceWarp Exit { get; set; }
            public string Condition { get; set; }
        }
        //
        //  version 1.6
        //
        public List<MineCartSpot> MineCarts { get; set; } = new();
        public double DirtDecayChance { get; set; } = 0.1;
        public Dictionary<string, TreasureArea> TreasureSpots { get; set; }=new();

        //public Dictionary<string,FishAreaData> FishAreas { get; set; }
        //
        //  fish data keyed to the FishAreas
        //
        public Dictionary<string, List<SpawnFishData>> FishData { get; set; }
        public bool AllowGrassGrowInWinter { get; set; }
        public bool AllowGrassSurviveInWinter { get; set; }
        public bool skipWeedGrowth { get; set; }
        public bool SpawnGrassFromPathsOnNewYear { get; set; }
        public bool SpawnRandomGrassOnNewYear { get; set; }
        public string Treasure { get; set; }
        public bool EnableGrassSpread { get; set; }
    }
}
