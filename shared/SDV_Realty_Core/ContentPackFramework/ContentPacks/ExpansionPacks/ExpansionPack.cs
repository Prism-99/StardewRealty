using System;
using System.Collections.Generic;
using xTile;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewRealty.SDV_Realty_Interface;
using Newtonsoft.Json;
using System.IO;
using StardewValley.GameData.Locations;
using static StardewRealty.SDV_Realty_Interface.ExpansionDetails;
using StardewValley.GameData;
using Prism99_Core.Utilities;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewValley.GameData.Minecarts;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
    //
    //  common
    //
    internal class ExpansionPack : ISDRContentPack
    {
        public ExpansionPack() { }
        public ExpansionPack(string locationName, ModFarmType modFarmType)
        {
            ModPath = modFarmType.MapName;
            LocationName = locationName;
            isAdditionalFarm = true;
        }
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
        //  base properties
        //
        public Vector2? ShippingBinLocation { get; set; } = null;
        public List<string> FormerLocationNames { get; set; } = new List<string>();
        public int? MinDailyWeeds { get; set; } = null;
        public int? MaxDailyWeeds { get; set; } = null;
        public int? FirstDayWeedMultiplier { get; set; } = null;
        public int? MinDailyForageSpawn { get; set; } = null;
        public int? MaxDailyForageSpawn { get; set; } = null;
        public double DirtDecayChance { get; set; } = 0.1;
        public double? ChanceForClay { get; set; } = null;
        public bool CanHaveGreenRainSpawns { get; set; } = true;
        //
        //  data to patch base farms
        //
        public bool IsBaseFarm { get; set; }
        public string BaseFarmMap { get; set; }
        public string BaseLocationName { get; set; }
        public List<MapEdit> BaseMapEdits { get; set; }
        public List<StarterBuilding> StaterBuildings { get; set; }
        public List<string> BasePropertiesToKeep { get; set; } = new List<string>();
        public bool isAdditionalFarm { get; set; }
        public string OriginalFarmKey { get; set; }
        //
        //  internal mine cart network
        //
        public MinecartNetworkData? InternalMineCarts = null;
        //
        //  stations
        //
        public Point? TrainStationIn { get; set; } = null;
        public Point? BoatDockIn { get; set; } = null;
        public Point? BusIn { get; set; } = null;
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
        public bool AllowMultipleInstances { get; set; }
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
        public string ForSaleDescription = string.Empty;
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
        public Vector2 MapSize { get; set; } = Vector2.Zero;
        public Dictionary<string, string> SeatTiles { get; set; }
        public Dictionary<string, string> MapProperties { get; set; } = new();
        public List<Point> suspensionBridges { get; set; } = new List<Point>();
        public List<ArtifactData> Artifacts { get; set; }
        public List<Tuple<Point, int>> Bushes { get; set; } = new List<Tuple<Point, int>>();
        public EntranceDetails CaveEntrance { get; set; } = new();
        public Dictionary<string, EntrancePatch> EntrancePatches { get; set; }
        public EntrancePatch? GetEntrancePatch(EntranceDirection direction)
        {
            if (EntrancePatches.TryGetValue(((int)direction).ToString(), out EntrancePatch patch))
            {
                return patch;
            }
            return null;
        }
        public Point DefaultWarp { get; set; }
        public Dictionary<string, string> MapStrings { get; set; }
        public Map ExpansionMap { get; set; }
        public Dictionary<string, string> SubMaps { get; set; } = new();
        public List<string> SubLocations { get; set; } = new();
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
        /// <summary>
        /// Allows the planting and harvesting of grass in the winter
        /// </summary>
        public bool AllowGrassGrowInWinter { get; set; } = true;
        /// <summary>
        /// Allows fall grass to survive into winter
        /// </summary>
        public bool AllowGrassSurviveInWinter { get; set; } = true;
        public bool skipWeedGrowth { get; set; }
        public bool SpawnGrassFromPathsOnNewYear { get; set; } = true;
        public bool SpawnRandomGrassOnNewYear { get; set; } = true;
        public string Treasure { get; set; }
        public bool EnableGrassSpread { get; set; } = true;
        public bool EnableBlueGrass { get; set; } = true;
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
        public bool InternalWorldMapTexture { get; set; } = true;
        public Dictionary<string, string> SeasonalWorldMapTextures { get; set; } = new();
        public string GetSeasonalWorldMapTexture(string season = null)
        {
            string testSeason = string.IsNullOrEmpty(season) ? Game1.season.ToString().ToLower() : season.ToLower();

            if (SeasonalWorldMapTextures.TryGetValue(testSeason, out string textureName))
            {
                return SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{LocationName}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{textureName}");
            }
            else
            {
                if (string.IsNullOrEmpty(WorldMapTexture) || ForSaleImage == null)
                {
                    return SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}no_world_map");
                }
                else
                {
                    if (InternalWorldMapTexture)
                        return SDVPathUtilities.NormalizePath($"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{LocationName}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{WorldMapTexture}");
                    else
                        return WorldMapTexture;
                }
            }
        }
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