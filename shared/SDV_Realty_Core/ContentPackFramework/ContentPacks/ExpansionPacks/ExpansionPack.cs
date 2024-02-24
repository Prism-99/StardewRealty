using System;
using System.Collections.Generic;
using xTile;
using Microsoft.Xna.Framework.Graphics;
using StardewRealty.SDV_Realty_Interface;
using Newtonsoft.Json;
using System.IO;


namespace SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks
{
    //
    //  common
    //
    internal partial class ExpansionPack:ISDRContentPack
    {
        public override string PackFileName => "expansion.json";
        public override ISDRContentPack ReadContentPack(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            ExpansionPack newExpansion= JsonConvert.DeserializeObject<ExpansionPack>(fileContent);
            newExpansion.ModPath=Path.GetDirectoryName(fileName);
            return newExpansion;
        }
        public Map ExpansionMap { get; set; }
        public Dictionary<string, FishAreaDetails> FishAreas { get; set; }
        public EntranceWarp MineCart { get; set; }
        public string Requirements { get; set; }
        public bool AddedToFarm { get; set; } = false;
        public bool LoadMap { get; set; } = true;
        public bool AddedToDataLocation { get; set; } = false;
        public string MapName { get; set; }
        public string LocationName { get; set; }
        public string LocationContextId { get; set; }
        public string LocationDefinition { get; set; }
        public string DisplayName { get; set; }
        public int Cost { get; set; }
        public bool StockedPonds { get; set; }
        public bool AllowGiantCrops { get; set; } = true;
        public bool CrowsEnabled { get; set; } = true;
        public bool AlwaysSnowing { get; set; } = false;
        public bool AlwaysRaining { get; set; } = false;
        public bool AlwaysSunny { get; set; } = false;
        public string SeasonOverride { get; set; }
        public Point DefaultWarp { get; set; }
        public Dictionary<string, string> MapStrings { get; set; }
        public string Vendor { get; set; }
        public string VendorMailId { get; set; }
        public string VendorMailContent { get; set; }
        public string Description { get; set; }
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
        public List<ArtifactData> Artifacts { get; set; }
        public Dictionary<string, string> DescriptionLocalization { get; set; }
        public List<Tuple<Point, int>> Bushes { get; set; } = new List<Tuple<Point, int>>();
        public EntranceDetails CaveEntrance { get; set; }
        public Dictionary<string, EntrancePatch> EntrancePatches { get; set; }
        public List<Point> suspensionBridges { get; set; } = new List<Point>();
        public string MailId { get; set; }
        public string MailContent { get; set; }
        public Dictionary<string, string> MailContentLocalization { get; set; }
        public Dictionary<string,string> SeatTiles { get; set; }
        public string ThumbnailName { get; set; }
        public Texture2D Thumbnail { get; set; }
        public string WorldMapTexture { get; set; } = "WorldMap.png";
        public string ForSaleImageName { get; set; }
        public Texture2D ForSaleImage { get; set; }
        public bool Purchased { get; set; }
        public Prism99_Core.Objects.SerializableDictionary<string, PatchingDetails> InsertDetails { get; set; }
        public List<InsertRule> InsertRules { get; set; }
        public string ActiveRule { get; set; }
        public bool ShowClouds { get; set; } = false;
        public bool ShowBirds { get; set; } = false;
        public bool ShowBunnies { get; set; } = false;
        public bool ShowButterflies { get; set; } = false;
        public bool ShowFrogs { get; set; } = false;
        public bool ShowSquirrels { get; set; } = false;
        public bool ShowWoodPeckers { get; set; } = false;
        public bool SetActiveRule(IMonitor monitor, IModHelper helper)
        {
            if (InsertRules == null)
            {
                //  log error, no rules, no activation
                //
                monitor.Log($"No Insert rules for {LocationName} - reminder, disable me", LogLevel.Info);
                ActiveRule = "0";
                return true;
            }
            else
            {
                string sActiveRule = "";
                int iActiveWeight = 0;
 
                foreach (InsertRule oRule in InsertRules)
                {
                    bool bAllRulesMet = true;
                    int iTestWeight = 0;
                    foreach (string sKey in oRule.When.Keys)
                    {
                        switch (sKey)
                        {
                            case "HasMod":
#if DEBUG
                                monitor.Log($"HasMod rule.  Value: {oRule.When[sKey]}, has mod: {helper.ModRegistry.IsLoaded(oRule.When[sKey])}", LogLevel.Info);
#endif
                                if (helper.ModRegistry.IsLoaded(oRule.When[sKey]))
                                {
                                    iTestWeight++;
                                }
                                else
                                {
                                    bAllRulesMet = false;
                                }
                                break;
                            case "FarmType":
                                bool bCorrectFarm = false;
                                monitor.Log($"FarmType rule.  Values: {oRule.When[sKey]}", LogLevel.Info);
                                switch (Game1.whichFarm)
                                {
                                    case 0: // standard
                                        bCorrectFarm = oRule.When[sKey].Contains("Standard");
                                        break;
                                    case 1: //fishing farm
                                        bCorrectFarm = oRule.When[sKey].Contains("Riverland");
                                        break;
                                    case 2: //foraging farm
                                        bCorrectFarm = oRule.When[sKey].Contains("Wilderness");
                                        break;
                                    case 3: // mining farm (hilltop)
                                        bCorrectFarm = oRule.When[sKey].Contains("Hilltop");
                                        break;
                                    case 4: // combat farm
                                        bCorrectFarm = oRule.When[sKey].Contains("Combat");
                                        break;
                                    case 5: // four corners
                                        bCorrectFarm = oRule.When[sKey].Contains("FourCorners");
                                        break;
                                    case 6: // island farm
                                        bCorrectFarm = oRule.When[sKey].Contains("Island");
                                        break;
                                }
                                monitor.Log($"FarmType result check {bCorrectFarm}", LogLevel.Info);
                                if (bCorrectFarm) iTestWeight++;
                                bAllRulesMet = bAllRulesMet && bCorrectFarm;
                                break;
                        }
                        if (!bAllRulesMet) break;
                    }
                    if (bAllRulesMet && iTestWeight > iActiveWeight)
                    {
                        iActiveWeight = iTestWeight;
                        sActiveRule = oRule.Use;
                    }
                }
#if DEBUG
                monitor.Log($"Found matching rule: {!string.IsNullOrEmpty(sActiveRule)}, weight: {iActiveWeight}", LogLevel.Info);
#endif
                if (!string.IsNullOrEmpty(sActiveRule))
                {
                    monitor.Log($"ActiveRule set to {sActiveRule}", LogLevel.Info);
                    ActiveRule = sActiveRule;
                    return true;
                }

                return false;
            }

        }
    }
}