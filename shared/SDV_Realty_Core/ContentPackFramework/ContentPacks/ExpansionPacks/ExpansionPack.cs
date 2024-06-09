using System;
using System.Collections.Generic;
using xTile;
using Microsoft.Xna.Framework.Graphics;
using StardewRealty.SDV_Realty_Interface;
using Newtonsoft.Json;
using System.IO;
using StardewValley.GameData.Locations;
using static StardewRealty.SDV_Realty_Interface.ExpansionDetails;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
    //
    //  common
    //
    internal class ExpansionPack : ISDRContentPack
    {
        public override string PackFileName => "expansion.json";
        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            ExpansionPack newExpansion = JsonConvert.DeserializeObject<ExpansionPack>(fileContent);
            newExpansion.ModPath = Path.GetDirectoryName(fileName);
            return newExpansion;
        }

        public struct MineCartSpot
        {
            public bool MineCartEnabled { get; set; }
            public List<Point> MineCartActionPoints { get; set; }
            public string MineCartDisplayName { get; set; }
            public string MineCartDirection { get; set; }
            public EntranceWarp Exit { get; set; }
            public string Condition { get; set; }
        }
        public struct StarterBuilding
        {
            public string BuldingType { get; set; }
            public Vector2 Location { get; set; }
            public List<string> AnimalsToAdd { get; set; }
        }
        //
        //  data to patch base farms
        //
        public bool IsBaseFarm { get; set; }
        public string BaseFarmMap { get; set; }
        public string BaseLocationName { get; set; }
        public List<MapEdit> BaseMapEdits { get; set; }
        public List<StarterBuilding> StaterBuildings { get; set; }
        public List<string> BasePropertiesToKeep { get; set; } = new List<string>();
    
        //
        //  status flags
        //
        public bool Purchased { get; set; }
        public bool AddedToFarm { get; set; } = false;
        public bool LoadMap { get; set; } = true;
        public bool AddedToDataLocation { get; set; } = false;

        //
        //  purchase details
        //
        public int Cost { get; set; }
        public string Requirements { get; set; }
        public string Vendor { get; set; }
        public string VendorMailId { get; set; }
        public string VendorMailContent { get; set; }
        public string MailId { get; set; }
        public string MailContent { get; set; }
        public Dictionary<string, string> MailContentLocalization { get; set; }
        //
        //  expansion details
        //
        public string LocationName { get; set; }
        public string LocationContextId { get; set; }
        public string LocationDefinition { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> DescriptionLocalization { get; set; }

        //
        //  environmental properties
        //
        public bool AlwaysSnowing { get; set; } = false;
        public bool AlwaysRaining { get; set; } = false;
        public bool AlwaysSunny { get; set; } = false;
        public string SeasonOverride { get; set; }

        //
        //  map properties
        //
        public string MapName { get; set; }
        public Dictionary<string, string> SeatTiles { get; set; }
        public Dictionary<string, string> MapProperties { get; set; } = new ();
        public List<Point> suspensionBridges { get; set; } = new List<Point>();
        public List<ArtifactData> Artifacts { get; set; }
        public List<Tuple<Point, int>> Bushes { get; set; } = new List<Tuple<Point, int>>();
        public EntranceDetails CaveEntrance { get; set; }
        public Dictionary<string, EntrancePatch> EntrancePatches { get; set; }
        public Point DefaultWarp { get; set; }
        public Dictionary<string, string> MapStrings { get; set; }
        public Map ExpansionMap { get; set; }
        public Dictionary<string, string> SubMaps { get; set; }
        public Dictionary<string, FishAreaDetails> FishAreas { get; set; }
        public List<MineCartSpot> MineCarts { get; set; } = new();
        /// <summary>
        /// Areas that can be seeded with Treasure spots
        /// </summary>
        public Dictionary<string, TreasureArea> TreasureSpots { get; set; } = new();
        //
        //  fish data keyed to the FishAreas
        //
        public Dictionary<string, List<SpawnFishData>> FishData { get; set; }
        public double DirtDecayChance { get; set; } = 0.1;
        /// <summary>
        /// Allows the planting and harvesting of grass in the winter
        /// </summary>
        public bool AllowGrassGrowInWinter { get; set; }
        /// <summary>
        /// Allows fall grass to survive into winter
        /// </summary>
        public bool AllowGrassSurviveInWinter { get; set; }
        public bool skipWeedGrowth { get; set; }
        public bool SpawnGrassFromPathsOnNewYear { get; set; }
        public bool SpawnRandomGrassOnNewYear { get; set; }
        public string Treasure { get; set; }
        public bool EnableGrassSpread { get; set; }

        //
        //  location mechanics
        //
        public bool ShowClouds { get; set; } = false;
        public bool ShowBirds { get; set; } = false;
        public bool ShowBunnies { get; set; } = false;
        public bool ShowButterflies { get; set; } = false;
        public bool ShowFrogs { get; set; } = false;
        public bool ShowSquirrels { get; set; } = false;
        public bool ShowWoodPeckers { get; set; } = false;
        public bool StockedPonds { get; set; }
        public bool AllowGiantCrops { get; set; } = true;
        public bool CrowsEnabled { get; set; } = true;
        //
        // expansion graphics
        //
        public string WorldMapTexture { get; set; } = "WorldMap.png";
        public string ForSaleImageName { get; set; }
        public Texture2D ForSaleImage { get; set; }
        public string ThumbnailName { get; set; }
        public Texture2D Thumbnail { get; set; }


        public string GetDescription()
        {
            if (Game1.content.GetCurrentLanguage().ToString() == "en")
            {
                return Description;
            }
            if (DescriptionLocalization != null && DescriptionLocalization.ContainsKey(Game1.content.GetCurrentLanguage().ToString()))
            {
                return DescriptionLocalization[Game1.content.GetCurrentLanguage().ToString()];
            }

            return Description;
        }
    }
}