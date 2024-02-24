using Prism99_Core.Utilities;
using StardewRealty.SDV_Realty_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;

namespace SDV_Realty_Core.Framework.Objects
{
    internal partial class CustomizationDetails
    {
        //
        // common
        //
        public Dictionary<string, List<ForageData>> ForageData;
        // used for 1.5
        public Dictionary<string, List<FishStockData>> FishData { get; set; }
 

        public CustomizationDetails()
        {
            ForageData = new Dictionary<string, List<ForageData>>
            {
                {"Spring",new List<ForageData>{} },
                {"Summer",new List<ForageData>{} },
                {"Fall",new List<ForageData>{} },
                {"Winter",new List<ForageData>{} }
            };
            FishData = new Dictionary<string, List<FishStockData>>
            {
                {"Spring",new List<FishStockData>{} },
                {"Summer",new List<FishStockData>{} },
                {"Fall",new List<FishStockData>{} },
                {"Winter",new List<FishStockData>{} }
            };
            FishAreas = new Dictionary<string, FishAreaDetails> { };
        }

        public CustomizationDetails(SDRCustomization cust) : this()
        {
            Update(cust);
        }
        public CustomizationDetails(ExpansionPack oPack) : this()
        {
            ExpansionName = oPack.DisplayName;
            CrowsEnabled = oPack.CrowsEnabled;
            AllowGiantCrops = oPack.AllowGiantCrops;
            AlwaysRaining = oPack.AlwaysRaining;
            AlwaysSnowing = oPack.AlwaysSnowing;
            AlwaysSunny = oPack.AlwaysSunny;
        }
        public int Version { get; set; } = 0;
        public string ExpansionName { get; set; }
        public string Fishing() { return string.Join('/', GetFishArray()); }
        [Obsolete("Method is deprecated.  Use GetFishArray")]
        public string[] FishingParts = new string[4];
        public string Foraging() { return string.Join('/', GetForageArray()); }
        [Obsolete("Method is deprecated.  Use GetForageArray")]
        public string[] ForagingParts = new string[4];
        public string Artifacts { get; set; }
        public List<ArtifactData> ArtifactList { get; set; } = new List<ArtifactData>();
        public bool? AlwaysRaining { get; set; } = null;
        public bool? AlwaysSunny { get; set; } = null;
        public bool? AlwaysSnowing { get; set; } = null;
        public bool? AllowGiantCrops { get; set; } = null;
        public bool StockedPond;
        public string SeasonOverride;
        public bool? CrowsEnabled;
        //public Dictionary<string, string> FishAreas { get; set; }
        public Dictionary<string, FishAreaDetails> FishAreas { get; set; }

        public void Update(SDRCustomization cust)
        {
            AllowGiantCrops = cust.AllowGiantCrops;
            AlwaysRaining = cust.AlwaysRaining;
            AlwaysSnowing = cust.AlwaysSnowing;
            AlwaysSunny = cust.AlwaysSunny;
            ParseForageString("Spring", cust.SpringForage);
            ParseForageString("Summer", cust.SummerForage);
            ParseForageString("Fall", cust.FallForage);
            ParseForageString("Winter", cust.WinterForage);
            ParseFishString("Spring", cust.SpringFish);
            ParseFishString("Summer", cust.SummerFish);
            ParseFishString("Fall", cust.FallFish);
            ParseFishString("Winter", cust.WinterFish);
        }
        public void AddForageItem(string season, string forageId, float chance)
        {
            ForageData[season].Add(new ForageData
            {
                Chance = chance,
                ForageId = forageId,
                Season = season
            });
        }
        public void AddFishItem(string season, string forageId, float chance)
        {
            FishData[season].Add(new FishStockData
            {
                Chance = chance,
                FishId = forageId,
                Season = season
            });
        }
        private string[] GetForageArray()
        {
            return new string[]
            {
                GetForageString("Spring"),
                GetForageString("Summer"),
                GetForageString("Fall"),
                GetForageString("Winter")
            };
        }
        private string[] GetFishArray()
        {
            return new string[]
            {
                GetFishString("Spring"),
                GetFishString("Summer"),
                GetFishString("Fall"),
                GetFishString("Winter")
            };
        }
        public bool Upgrade()
        {
            bool upgraded = false;

            switch (Version)
            {
                case 0:
                    if (ArtifactList == null)
                    {
                        ArtifactList = new List<ArtifactData>();
                    }
                    if (!string.IsNullOrEmpty(Artifacts))
                    {
                        //
                        //  upgrade artifacts
                        //
                        upgraded = true;
                        string[] arParts = Artifacts.Split(' ');
                        for (int index = 0; index < arParts.Length; index += 2)
                        {
                            string objId = arParts[index];
                            float chance = 1;
                            if (index + 1 < arParts.Length)
                            {
                                float.TryParse(arParts[index + 1], out chance);
                            }
                            ArtifactList.Add(new ArtifactData
                            {
                                ArtifactId = $"(O){objId}",
                                Chance = chance
                            });
                        }
                        //
                        //  clear value
                        //
                        Artifacts = null;
                    }
                    upgraded = upgraded || UpgradeForageDataV1("Spring");
                    ForagingParts[0] = "";
                    upgraded = upgraded || UpgradeForageDataV1("Summer");
                    ForagingParts[1] = "";
                    upgraded = upgraded || UpgradeForageDataV1("Fall");
                    ForagingParts[2] = "";
                    upgraded = upgraded || UpgradeForageDataV1("Winter");
                    ForagingParts[3] = "";
                    //
                    //  upgrade fish data
                    //
                    upgraded = upgraded || UpgradeFishDataV1("Spring");
                    FishingParts[0] = "";
                    upgraded = upgraded || UpgradeFishDataV1("Summer");
                    FishingParts[1] = "";
                    upgraded = upgraded || UpgradeFishDataV1("Fall");
                    FishingParts[2] = "";
                    upgraded = upgraded || UpgradeFishDataV1("Winter");
                    FishingParts[3] = "";
                    //
                    //  update version #
                    //
                    //Version = 1;
                    break;
            }
            return upgraded;
        }
        private bool UpgradeFishDataV1(string season)
        {
            bool upgradegraded = false;
            string data = season switch
            {
                "Spring" => FishingParts[0],
                "Summer" => FishingParts[1],
                "Fall" => FishingParts[2],
                "Winter" => FishingParts[3],
                _ => null
            };

            if (!string.IsNullOrEmpty(data))
            {
                upgradegraded = true;
                ParseFishString(season, data);
            }

            return upgradegraded;
        }
        private bool UpgradeForageDataV1(string season)
        {
            bool upgradegraded = false;
            string data = season switch
            {
                "Spring" => ForagingParts[0],
                "Summer" => ForagingParts[1],
                "Fall" => ForagingParts[2],
                "Winter" => ForagingParts[3],
                _ => null
            };

            if (!string.IsNullOrEmpty(data))
            {
                upgradegraded = true;
                ParseForageString(season, data);
            }

            return upgradegraded;
        }
        private void ParseFishString(string season, string data)
        {
            string[] arData = data.Split(" ");
            for (int ipos = 0; ipos < arData.Length; ipos += 2)
            {
                if (!string.IsNullOrEmpty(arData[ipos]))
                {
                    FishData[season].Add(new FishStockData
                    {
                        Chance = (float)(ipos + 1 < arData.Length ? Convert.ToDouble(arData[ipos + 1]) : 0),
                        FishId = arData[ipos]
                    });
                }
            }

        }
        private void ParseForageString(string season, string data)
        {
            string[] arData = data.Split(" ");
            for (int ipos = 0; ipos < arData.Length; ipos += 2)
            {
                if (!string.IsNullOrEmpty(arData[ipos]))
                {
                    ForageData[season].Add(new ForageData
                    {
                        Chance = ipos + 1 < arData.Length ? Convert.ToDouble(arData[ipos + 1]) : 0,
                        ForageId = arData[ipos]
                    });
                }
            }
        }
        public string GetFishData(string season)
        {
            return GetFishString(SDVUtilities.GetCleanSeason(season));
        }
        public string GetForageData(string season)
        {
            return GetForageString(SDVUtilities.GetCleanSeason(season));
        }
        private string GetForageString(string season)
        {
            string cleanSeason = SDVUtilities.GetCleanSeason(season);

            if (ForageData.ContainsKey(cleanSeason) && ForageData[cleanSeason].Count>0)
            {
                return string.Join(' ', ForageData[cleanSeason].Select(p => p.ForageId + " " + p.Chance.ToString()).ToArray());
            }
            else
            {
                return "-1";
            }
        }
        private string GetFishString(string season)
        {
            string cleanSeason = SDVUtilities.GetCleanSeason(season);

            if (FishData.ContainsKey(cleanSeason) && FishData[cleanSeason].Count>0)
            {
                return string.Join(' ', FishData[cleanSeason].Where(p=>p.FishId !=null).Select(p => p.FishId.Replace("(O)","") + " -1").ToArray());
            }
            else
            {
                return "-1";
            }

        }
    }
}
