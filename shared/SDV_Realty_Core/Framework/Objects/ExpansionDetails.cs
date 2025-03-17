using SDV_Realty_Core.Framework.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;

namespace StardewRealty.SDV_Realty_Interface
{
    public partial class ExpansionDetails
    {
        //
        //  common
        //
        internal ExpansionDetails(ExpansionPack oPack, bool active, bool forsale)
        {
            AllowGiantCrops = oPack.AllowGiantCrops;
            AlwaysRaining = oPack.AlwaysRaining;
            AlwaysSnowing = oPack.AlwaysSnowing;
            AlwaysSunny = oPack.AlwaysSunny;
            LocationName = oPack.LocationName;
            DisplayName = oPack.DisplayName;
            Requirements = oPack.Requirements;
            Description = oPack.Description;
            ShippingBin = oPack.ShippingBinLocation;
            CaveOut = oPack.CaveEntrance != null ? new Point(oPack.CaveEntrance.WarpIn.X, oPack.CaveEntrance.WarpIn.Y) : null;
            Cost = oPack.Cost;
            CrowsEnabled = oPack.CrowsEnabled;
            Format = oPack.FileFormat;
            MapName = oPack.MapName;
            SeasonOverride = oPack.SeasonOverride;
            StockedPonds = oPack.StockedPonds;
            ThumbnailName = oPack.ThumbnailName;
            Vendor = oPack.Vendor;
            Artifacts = oPack.Artifacts;
            Active = active;
            ForSale = forsale;
            BaseDefinition = oPack.LocationDefinition;
            TreasureSpots = oPack.TreasureSpots;
            if (oPack.FishAreas != null)
            {
                FishAreas = new Dictionary<string, FishAreaDetails> { };
                FishAreas = oPack.FishAreas;
                //foreach (string area in oPack.FishAreas.Keys)
                //{
                //    FishAreaDetails fad = new FishAreaDetails
                //    {
                //        DisplayName = oPack.FishAreas[area].DisplayName,
                //        Id = area
                //    };
                //    if (oPack.FishData != null && oPack.FishData.ContainsKey(area))
                //    {
                //        foreach (var item in oPack.FishData[area])
                //        {
                //            fad.StockData.Add(new FishStockData
                //            {
                //                FishId = item.Id,
                //                Chance = item.Chance,
                //                Season = item.Season.ToString()
                //            });
                //        }
                //    }

                //}
            }
            ParseDefinition();
        }
        private void ParseDefinition()
        {
            if (string.IsNullOrEmpty(BaseDefinition))
                return;

            string[] arParts = BaseDefinition.Split("/");

            if (arParts.Length > 4)
                ParseFishString("Spring", arParts[4]);
            if (arParts.Length > 5)
                ParseFishString("Summer", arParts[5]);
            if (arParts.Length > 6)
                ParseFishString("Fall", arParts[6]);
            if (arParts.Length > 7)
                ParseFishString("Spring", arParts[7]);
        }
        private void ParseFishString(string season, string data)
        {
            string[] arData = data.Split(" ");
            for (int ipos = 0; ipos < arData.Length; ipos += 2)
            {
                if (!string.IsNullOrEmpty(arData[ipos]))
                {
                    if (FishAreas == null)
                    {
                        FishAreas = new Dictionary<string, FishAreaDetails> { };
                    }
                    if (!FishAreas.ContainsKey("default"))
                        FishAreas.Add("default", new FishAreaDetails());

                    FishAreas["default"].StockData.Add( new FishStockData
                    {
                        Chance = (float)(ipos + 1 < arData.Length ?Math.Abs( Convert.ToDouble(arData[ipos + 1])) : 0),
                        FishId = arData[ipos],
                        Season = season
                    });
                }
            }

        }
        internal void Update(CustomizationDetails oCustom)
        {
            if (oCustom != null)
            {
                if (oCustom.CrowsEnabled.HasValue)
                    CrowsEnabled = oCustom.CrowsEnabled.Value;
                if (oCustom.AllowGiantCrops.HasValue)
                    AllowGiantCrops = oCustom.AllowGiantCrops.Value;

                if (oCustom.AlwaysSunny.HasValue)
                    AlwaysSunny = oCustom.AlwaysSunny.Value;

                if (oCustom.AlwaysSnowing.HasValue)
                    AlwaysSnowing = oCustom.AlwaysSnowing.Value;

                if (oCustom.AlwaysRaining.HasValue)
                    AlwaysRaining = oCustom.AlwaysRaining.Value;

                if (oCustom.SeasonOverride != null)
                {
                    SeasonOverride = oCustom.SeasonOverride;
                }

                if (oCustom.ArtifactList != null)
                {
                    Artifacts = oCustom.ArtifactList;
                }
                if (oCustom.ForageData != null)
                {
                    if (ForageData == null)
                        ForageData = new List<ForageData> { };

                    if (oCustom.ForageData.ContainsKey("Spring"))
                        ForageData.AddRange(oCustom.ForageData["Spring"].Select(p => new ForageData { Chance = p.Chance, ForageId = p.ForageId, Season = "Spring" }));
                    if (!oCustom.ForageData.ContainsKey("Summer"))
                        ForageData.AddRange(oCustom.ForageData["Summer"].Select(p => new ForageData { Chance = p.Chance, ForageId = p.ForageId, Season = "Summer" }));
                    if (!oCustom.ForageData.ContainsKey("Fall"))
                        ForageData.AddRange(oCustom.ForageData["Fall"].Select(p => new ForageData { Chance = p.Chance, ForageId = p.ForageId, Season = "Fall" }));
                    if (!oCustom.ForageData.ContainsKey("Winter"))
                        ForageData.AddRange(oCustom.ForageData["Winter"].Select(p => new ForageData { Chance = p.Chance, ForageId = p.ForageId, Season = "Winter" }));
                }
                if (oCustom.FishData != null)
                {
                    if (FishAreas == null)
                    {
                        FishAreas = new Dictionary<string, FishAreaDetails> { };
                    }
                    if (!FishAreas.ContainsKey("default"))
                        FishAreas.Add("default", new FishAreaDetails());

                    FishAreas["default"].StockData.AddRange(oCustom.FishData["Spring"].Select(p => new FishStockData { Chance = p.Chance, FishId = p.FishId, Season = "Spring" }));
                    FishAreas["default"].StockData.AddRange(oCustom.FishData["Summer"].Select(p => new FishStockData { Chance = p.Chance, FishId = p.FishId, Season = "Summer" }));
                    FishAreas["default"].StockData.AddRange(oCustom.FishData["Fall"].Select(p => new FishStockData { Chance = p.Chance, FishId = p.FishId, Season = "Fall" }));
                    FishAreas["default"].StockData.AddRange(oCustom.FishData["Winter"].Select(p => new FishStockData { Chance = p.Chance, FishId = p.FishId, Season = "Winter" }));
                }
                if (oCustom.FishAreas != null)
                {
                    foreach (string key in oCustom.FishAreas.Keys)
                    {
                        if (FishAreas.ContainsKey(key))
                        {
                            foreach (var fish in oCustom.FishAreas[key].StockData)
                            {
                                if (!FishAreas[key].StockData.Contains(fish))
                                    FishAreas[key].StockData.Add(fish);
                            }
                        }
                    }
                }
            }
        }
        internal void Update(SDRCustomization oCustom)
        {
            if (oCustom != null)
            {
                if (oCustom.AllowGiantCrops.HasValue)
                    AllowGiantCrops = oCustom.AllowGiantCrops.Value;

                if (oCustom.AlwaysSunny.HasValue)
                    AlwaysSunny = oCustom.AlwaysSunny.Value;

                if (oCustom.AlwaysSnowing.HasValue)
                    AlwaysSnowing = oCustom.AlwaysSnowing.Value;

                if (oCustom.AlwaysRaining.HasValue)
                    AlwaysRaining = oCustom.AlwaysRaining.Value;

                if (oCustom.Season != null)
                {
                    SeasonOverride = oCustom.Season;
                }

            }
        }
    }
}
