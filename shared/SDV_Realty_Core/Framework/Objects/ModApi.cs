using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Buildings;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.CustomEntities.Buildings;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using System.IO;
using Prism99_Core.Utilities;
using StardewRealty.SDV_Realty_Interface;
using Newtonsoft.Json;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.ServiceInterfaces;


namespace SDV_Realty_Core.Framework.Objects
{
    /// <summary>An API provided by Farm Expansion for other mods to use.</summary>
    public partial class ModApi : IStardewRealtyAPI
    {
        //
        //  common
        //
        /*********
        ** Properties
        *********/
        /// <summary>The Farm Expansion core framework.</summary>
        //private readonly FEFramework Framework;
        //private FEContent content;
        private SDRContentManager contentManager;        
        private ILoggerService logger;
        private CustomBuildingManager _customBuildingManager=null;
        private IExpansionManager expansionManager;
        //private IContentPackService contentPackService;
        private IWarproomService warproomService;
        //private IExpansionCustomizationService expansionCustomizationService;
        /********* 
             ** Public methods
             *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="framework">The Farm Expansion core framework.</param>
        internal ModApi( SDRContentManager cMan, ILoggerService olog, IExpansionManager expansionManager,IContentPackService contentPackService, IWarproomService warproomService, IExpansionCustomizationService expansionCustomizationService)
        {
            logger = olog;
            contentManager = cMan;
            this.expansionManager = expansionManager;
            //this.contentPackService = contentPackService;
            this.warproomService = warproomService;
            //this.expansionCustomizationService = expansionCustomizationService;
        }

        /// <summary>
        /// Gets a list of Fish areas defined for the expansion
        /// </summary>
        /// <param name="expansionName"></param>
        /// <returns><list type="string">FishAreaId</list></returns>
        //
        public Dictionary<string, Tuple<int, int, int, int>> GetFishAreaDetails(string expansionName)
        {
            if (_modDataService.validContents.ContainsKey(expansionName))
            {
                return _modDataService.validContents[expansionName].FishAreas.ToDictionary(p => p.Value.DisplayName,
                    p => Tuple.Create(p.Value.Position.X, p.Value.Position.Y, p.Value.Position.Width, p.Value.Position.Height));
            }

            return null;

        }
        public Dictionary<string, string> GetCustomBuildingList()
        {
            return _customBuildingManager.CustomBuildings.Select(p => new KeyValuePair<string, string>(p.Key, p.Value.DisplayName)).ToDictionary(x => x.Key, x => x.Value);
        }
        public void AddFishToArea(string expansionName, string areaId, string season, string fishId, float chance)
        {
            //
            //  verify expansion exists
            //
            if (expansionManager.expansionManager.farmExpansions.ContainsKey(expansionName))
            {
                string cleanSeason = SDVUtilities.GetCleanSeason(season);
                //
                //  check if expansion has existing customization
                //
                if (!_modDataService.CustomDefinitions.ContainsKey(expansionName))
                {
                    AddCustomizationEntry(expansionName);
                }
                //
                //  check if area exists
                //
                if (expansionManager.expansionManager.ExpDetails[expansionName].FishAreas.ContainsKey(areaId))
                {
                    //
                    //  add area to custom data, if it does not exist
                    //
                    if (!_modDataService.CustomDefinitions[expansionName].FishAreas.ContainsKey(areaId))
                        _modDataService.CustomDefinitions[expansionName].FishAreas.Add(areaId, new FishAreaDetails());

                    //
                    //  add new fish to area
                    //
                    _modDataService.CustomDefinitions[expansionName].FishAreas[areaId].StockData.Add(new FishStockData()
                    {
                        FishId = fishId,
                        Chance = chance,
                        Season = cleanSeason
                    });
                    _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
                    //_modDataService.SaveDefinitions();
                    //
                    //  update active definition
                    //
                    expansionManager.expansionManager.ExpDetails[expansionName].FishAreas[areaId].StockData.Add(new FishStockData
                    {
                        Chance = chance,
                        FishId = fishId,
                        Season = cleanSeason
                    });
                }
            }
        }
        public void DeleteFishFromArea(string expansionName, string areaId, string season, string fishId, float chance)
        {
            //
            //  check expansion exists
            //
            if (expansionManager.expansionManager.farmExpansions.ContainsKey(expansionName))
            {
                string cleanSeason = SDVUtilities.GetCleanSeason(season);

                //
                //  check if expansion has customization
                //
                if (_modDataService.CustomDefinitions.ContainsKey(expansionName))
                {
                    //
                    //  verify area exists
                    //
                    if (expansionManager.expansionManager.farmExpansions[expansionName].FishAreas.ContainsKey(areaId))
                    {
                        //
                        //  check for area customization
                        //
                        if (_modDataService.CustomDefinitions[expansionName].FishAreas.ContainsKey(areaId))
                        {
                            //
                            //  remove fish entry
                            //
                            var fish = _modDataService.CustomDefinitions[expansionName].FishAreas[areaId].StockData.Where(p => p.FishId == fishId && ((p.Season == null && season == null) || (p.Season != null && p.Season == season)));
                            if (fish.Any())
                            {
                                foreach (var item in fish)
                                {
                                    _modDataService.CustomDefinitions[expansionName].FishAreas[areaId].StockData.Remove(item);
                                    //
                                    //  update definition cache
                                    //
                                    if(expansionManager.expansionManager.ExpDetails.ContainsKey(expansionName) && expansionManager.expansionManager.ExpDetails[expansionName].FishAreas.ContainsKey(areaId))
                                    {
                                        expansionManager.expansionManager.ExpDetails[expansionName].FishAreas[areaId].StockData.Remove(item);
                                    }
                                }
                            }
                            //
                            //  save customizations
                            //
                            _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
                            //_modDataService.SaveDefinitions();
                        }
                        else
                        {
                            // remove from base definition
                        }
                    }
                }
                else
                {
                    // remove from base definition
                    //  need to make this persistent
                }
            }
        }

        public bool IsExpansion(string locationName)
        {
            return expansionManager.expansionManager.farmExpansions.ContainsKey(locationName);
        }

        public void AddRemoveListener(EventHandler handler)
        {
            //FEFramework.BeforeRemoveEvent += handler;
        }

        public void AddAppendListener(EventHandler handler)
        {
            //FEFramework.AfterAppendEvent += handler;
        }
        public void AddFishEntry(string expName, string fishID, float chance, string season)
        {
            if (expansionManager.expansionManager.farmExpansions.ContainsKey(expName))
            {
                string cleanSeason = SDVUtilities.GetCleanSeason(season);

                if (!_modDataService.CustomDefinitions.ContainsKey(expName))
                {
                    AddCustomizationEntry(expName);
                }
                _modDataService.CustomDefinitions[expName].AddFishItem(cleanSeason, fishID, chance);
                _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
                //_modDataService.SaveDefinitions();

            }
        }
        public void DeleteFishEntry(string expName, string fishID, float chance, string season)
        {
            if (!_modDataService.CustomDefinitions.ContainsKey(expName))
            {
                AddCustomizationEntry(expName);
            }
            string cleanSeason = SDVUtilities.GetCleanSeason(season);
            var entry = _modDataService.CustomDefinitions[expName].FishData[cleanSeason].Where(p => p.FishId == fishID && p.Chance == chance);
            if (entry.Any())
            {
                _modDataService.CustomDefinitions[expName].FishData[cleanSeason].Remove(entry.First());
                _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
                //_modDataService.SaveDefinitions();
            }
        }
        public void AddForageEntry(string expName, string forageID, float chance, string season)
        {
            if (expansionManager.expansionManager.farmExpansions.ContainsKey(expName))
            {
                string cleanSeason = SDVUtilities.GetCleanSeason(season);

                if (!_modDataService.CustomDefinitions.ContainsKey(expName))
                {
                    AddCustomizationEntry(expName);
                }
                _modDataService.CustomDefinitions[expName].AddForageItem(season, forageID, chance);
                _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
                //_modDataService.SaveDefinitions();
            }
        }
        public void DeleteForageEntry(string expName, string forageID, float chance, string season)
        {
            if (!_modDataService.CustomDefinitions.ContainsKey(expName))
            {
                AddCustomizationEntry(expName);
            }
            string cleanSeason = SDVUtilities.GetCleanSeason(season);
            var entry = _modDataService.CustomDefinitions[expName].ForageData[cleanSeason].Where(p => p.ForageId == forageID && p.Chance == chance);
            if (entry.Any())
            {
                _modDataService.CustomDefinitions[expName].ForageData[cleanSeason].Remove(entry.First());
                _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
                //_modDataService.SaveDefinitions();
            }
        }
        public void DeleteArtifactEntry(string expName, string artifactId, float chance, string season)
        {
            if (!_modDataService.CustomDefinitions.ContainsKey(expName))
            {
                AddCustomizationEntry(expName);
            }
            var entry = _modDataService.CustomDefinitions[expName].ArtifactList.Where(p => p.ArtifactId == artifactId && (p.Season == season || (p.Season == null && season == "")) && p.Chance == chance);
            if (entry.Any())
            {
                _modDataService.CustomDefinitions[expName].ArtifactList.Remove(entry.First());
                _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
               // _modDataService.SaveDefinitions();
            }
        }

        public void AddArtifactEntry(string expName, string artifactId, float chance, string season)
        {
            //
            //  add custom artifact data entry to the location
            //
            //  cannot validy artifactId as it may not be
            //  a simple fully qualified id
            //
            if (expansionManager.expansionManager.farmExpansions.ContainsKey(expName))
            {
                string cleanSeason = SDVUtilities.GetCleanSeason(season);

                if (!_modDataService.CustomDefinitions.ContainsKey(expName))
                {
                    AddCustomizationEntry(expName);
                }
                _modDataService.CustomDefinitions[expName].ArtifactList.Add(new ArtifactData
                {
                    ArtifactId = artifactId,
                    Chance = Math.Max(0, Math.Min(chance, 1)),
                    Season = cleanSeason
                });
                _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
                //_modDataService.SaveDefinitions();
            }
        }

        public string GetExpansionDefinition(string sExpName)
        {
            //
            // get the details of an expansion
            // including any customizations
            //
            if (!expansionManager.expansionManager.farmExpansions.ContainsKey(sExpName))
                return null;
            //
            //  get base definition
            //
            ExpansionDetails oDetails = expansionManager.expansionManager.ExpDetails[sExpName];//  new ExpansionDetails(FEFramework.ContentPacks.ValidContents[sExpName], FEFramework.farmExpansions[sExpName].Active, FEFramework.LandForSale.Contains(sExpName));

            //if (ExpansionCustomizations.CustomDefinitions.ContainsKey(sExpName))
            //{
            //    //
            //    //  add customizations
            //    //
            //    oDetails.Update(ExpansionCustomizations.CustomDefinitions[sExpName]);
            //}

            return JsonConvert.SerializeObject(oDetails);
        }
        public void AllowGiantCrops(string sExpName, bool allowGiantCrops)
        {
            if (!_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                AddCustomizationEntry(sExpName);
            }
            _modDataService.CustomDefinitions[sExpName].AllowGiantCrops = allowGiantCrops;
            _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
            //_modDataService.SaveDefinitions();
        }
        public void AllowCrows(string sExpName, bool allowCrows)
        {
            if (!_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                AddCustomizationEntry(sExpName);
            }
            _modDataService.CustomDefinitions[sExpName].CrowsEnabled = allowCrows;
            _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
            //_modDataService.SaveDefinitions();
        }
        public string SetExpansionDefinition(string sExpName, string sDefinition)
        {
            if (!expansionManager.expansionManager.farmExpansions.ContainsKey(sExpName))
                return null;

            try
            {
                ExpansionDetails oDetails = JsonConvert.DeserializeObject<ExpansionDetails>(sDefinition);
                if (oDetails != null)
                {
                    bool hasCustom = false;

                    if (!_modDataService.CustomDefinitions.ContainsKey(sExpName))
                    {
                        AddCustomizationEntry(sExpName);
                    }
                    if (oDetails.CrowsEnabled.HasValue)
                    {
                        _modDataService.CustomDefinitions[sExpName].CrowsEnabled = oDetails.CrowsEnabled;
                    }
                    if (oDetails.AllowGiantCrops.HasValue)
                    {
                        _modDataService.CustomDefinitions[sExpName].AllowGiantCrops = oDetails.AllowGiantCrops;
                    }
                    _modDataService.CustomDefinitions[sExpName].AlwaysRaining = oDetails.AlwaysRaining;
                    _modDataService.CustomDefinitions[sExpName].AlwaysSnowing = oDetails.AlwaysSnowing;
                    _modDataService.CustomDefinitions[sExpName].AlwaysSunny = oDetails.AlwaysSunny;
                    _modDataService.CustomDefinitions[sExpName].SeasonOverride = oDetails.SeasonOverride;
                    _modDataService.CustomDefinitions[sExpName].ArtifactList = oDetails.Artifacts;

                }
            }
            catch { }

            //ExpansionCustomizations.CustomDefinitions[sExpName].SetForaging(arParts[0] + "/" + arParts[1] + "/" + arParts[2] + "/" + arParts[3]);
            //ExpansionCustomizations.CustomDefinitions[sExpName].SetFishing(arParts[4] + "/" + arParts[5] + "/" + arParts[6] + "/" + arParts[7]);
            //ExpansionCustomizations.CustomDefinitions[sExpName].Artifacts = arParts[8];

            _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
            //_modDataService.SaveDefinitions();


            return "";
        }

        public List<string> GetLocationNames()
        {
            var list = _modDataService.validContents.Select(p => p.Key).ToList();
            //var list = contentPackService.contentPackLoader.ValidContents.Select(p => p.Key).ToList();

            list.Add("fe.winery");
            list.Add("fe.fromagerie");
            list.Add("fe.greenhouse");
            list.Add("sdr_warproom");
            list.Add("fe.largegreenhouse");
            list.Add("fe.breadfactory");

            return list;
        }
  
        public List<string> GetInstalledExpansionNames()
        {
            var list = _modDataService.validContents.Select(p => p.Key).ToList();

            return list;
        }

        //public Dictionary<string, Dictionary<string, string>> GetModList()
        //{
        //    Dictionary<string, Dictionary<string, string>> dcRet = new Dictionary<string, Dictionary<string, string>> { };

        //    foreach (Content oExp in FEFramework.ContentPacks.ValidContents.Values)
        //    {
        //        dcRet.Add(oExp.LocationName, new Dictionary<string, string> {

        //            {"Active",((BaseExpansion) FEFramework.farmExpansions[oExp.LocationName]).Active.ToString() },
        //            {"Cost", oExp.Cost.ToString() },
        //            {"Description", oExp.Description },
        //            {"ForSale", $"{FEFramework.LandForSale.Contains(oExp.LocationName)}" },
        //            {"Name", oExp.LocationName },
        //            {"Requirements", oExp.Requirements??"" },
        //            {"StockedPond",ExpansionCustomizations.CustomDefinitions.ContainsKey(oExp.LocationName)?ExpansionCustomizations.CustomDefinitions[oExp.LocationName].StockedPond.ToString(): oExp.StockedPonds.ToString()},
        //            {"DisplayName",oExp.DisplayName },
        //            {"CrowsEnabled",oExp.CrowsEnabled.ToString()},
        //            {"ThumbnailName",oExp.ThumbnailName },
        //            {"SeasonOverride",ExpansionCustomizations.CustomDefinitions.ContainsKey(oExp.LocationName)?ExpansionCustomizations.CustomDefinitions[oExp.LocationName].SeasonOverride: oExp.SeasonOverride },
        //            {"LocationDefinition",ExpansionCustomizations.CustomDefinitions.ContainsKey(oExp.LocationName)?ExpansionCustomizations.CustomDefinitions[oExp.LocationName].GetGameDefintion() : oExp.LocationDefinition }
        //        });
        //    }

        //    return dcRet;
        //}
        public string GetModList()
        {
            //Dictionary<string, Dictionary<string, string>> dcRet = new Dictionary<string, Dictionary<string, string>> { };
            List<ExpansionDetails> mods = new List<ExpansionDetails> { };

            foreach (ExpansionPack oExp in _modDataService.validContents.Values)
            {
                //bool stockedPond = oExp.StockedPonds;
                //string season = oExp.SeasonOverride;
                //string locdefn = oExp.LocationDefinition;
                //ExpansionDetails ed = new ExpansionDetails(oExp, FEFramework.farmExpansions[oExp.LocationName].Active, FEFramework.LandForSale.Contains(oExp.LocationName));
                //ExpansionDetails ed = FEFramework.ExpDetails[oExp.LocationName];
                //if (ExpansionCustomizations.CustomDefinitions.ContainsKey(oExp.LocationName))
                //{
                //    ed.Update(ExpansionCustomizations.CustomDefinitions[oExp.LocationName]);
                //    //CustomizationDetails cd = ExpansionCustomizations.CustomDefinitions[oExp.LocationName];
                //    //stockedPond = cd.StockedPond;
                //    //if (!string.IsNullOrEmpty(cd.SeasonOverride))
                //    //{
                //    //    season = cd.SeasonOverride;
                //    //}
                //    //if(cd.AllowGiantCrops.HasValue)

                //    //if (!string.IsNullOrEmpty(cd.GetGameDefintion()))
                //    //{
                //    //    locdefn = cd.GetGameDefintion();
                //    //}
                //}
                mods.Add(_modDataService.expDetails[oExp.LocationName]);
                //mods.Add(expansionManager.expansionManager.ExpDetails[oExp.LocationName]);
                //dcRet.Add(oExp.LocationName, new Dictionary<string, string> {

                //    {"Active", FEFramework.farmExpansions[oExp.LocationName].Active.ToString() },
                //    {"Cost", oExp.Cost.ToString() },
                //    {"Description", oExp.Description },
                //    {"ForSale", $"{FEFramework.LandForSale.Contains(oExp.LocationName)}" },
                //    {"Name", oExp.LocationName },
                //    {"Requirements", oExp.Requirements??"" },
                //    {"StockedPond",stockedPond.ToString()},
                //    {"DisplayName",oExp.DisplayName },
                //    {"CrowsEnabled",ed.CrowsEnabled.ToString()},
                //    {"ThumbnailName",oExp.ThumbnailName },
                //    {"SeasonOverride",ed.SeasonOverride },
                //    {"LocationDefinition",locdefn},
                //    {"AlwaysRaining",ed.AlwaysRaining?"Y":""},
                //    {"AlwaysSnowing",ed.AlwaysSnowing?"Y":""},
                //    {"AlwaysSunny",ed.AlwaysSunny?"Y":""},
                //    {"AllowGiantCrops",ed.AllowGiantCrops?"Y":""}
                //});
            }

            return JsonConvert.SerializeObject(mods);
        }

        public Dictionary<int, string> GetMapGrid()
        {
            return _modDataService.MapGrid;
        }

        public void AddCustomizationEntry(string sExpName)
        {
            if (!_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                _modDataService.CustomDefinitions.Add(sExpName, new CustomizationDetails(_modDataService.validContents[sExpName])
                {
                    StockedPond = _modDataService.validContents[sExpName].StockedPonds,
                    CrowsEnabled = _modDataService.validContents[sExpName].CrowsEnabled
                });
            }
        }
        public void SetExpansionStockedPond(string sExpName, bool stockedPond)
        {
            logger.Log($"Setting StockedPond [{stockedPond}] for {sExpName}", LogLevel.Debug);
            if (!_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                AddCustomizationEntry(sExpName);
            }
            _modDataService.CustomDefinitions[sExpName].StockedPond = stockedPond;

            _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
            //_modDataService.SaveDefinitions();
        }
        public bool GetExpansionStockedPond(string sExpName)
        {
            if (_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                return _modDataService.CustomDefinitions[sExpName].StockedPond;
            }

            return _modDataService.validContents[sExpName].StockedPonds;

        }
        public void SetExpansionWeather(string sExpName, string weather)
        {
            logger.Log($"Setting Expansion weather to {(string.IsNullOrEmpty(weather) ? "Default" : weather)} for {sExpName}", StardewModdingAPI.LogLevel.Debug);
            if (!_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                AddCustomizationEntry(sExpName);
            }

            _modDataService.CustomDefinitions[sExpName].AlwaysRaining = (weather.ToLower() == "raining");
            _modDataService.CustomDefinitions[sExpName].AlwaysSnowing = (weather.ToLower() == "snowing");
            _modDataService.CustomDefinitions[sExpName].AlwaysSunny = (weather.ToLower() == "sunny");

            _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
            //_modDataService.SaveDefinitions();
        }

        public void SetExpansionCrowsEnabled(string sExpName, bool stockedPond)
        {
            logger.Log($"Setting ExpansionCrowsEnabled [{stockedPond}] for {sExpName}", StardewModdingAPI.LogLevel.Debug);
            if (!_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                AddCustomizationEntry(sExpName);
            }

            _modDataService.CustomDefinitions[sExpName].CrowsEnabled = stockedPond;

            _eventsService.TriggerCustomEvent("SaveExpansionCustomizations", null);
            //_modDataService.SaveDefinitions();
        }
        public bool GetExpansionCrowsEnabled(string sExpName)
        {
            if (_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                return _modDataService.CustomDefinitions[sExpName].CrowsEnabled ?? false;
            }

            return _modDataService.validContents[sExpName].CrowsEnabled;

        }

        public void SetExpansionCustomizations(string sExpName, string jsonData)
        {
            try
            {
                SDRCustomization cust = JsonConvert.DeserializeObject<SDRCustomization>(jsonData);

                if (_modDataService.CustomDefinitions.ContainsKey(sExpName))
                {
                    _modDataService.CustomDefinitions[sExpName].Update(cust);
                }
                else if (_modDataService.farmExpansions.ContainsKey(sExpName))
                {
                    _modDataService.CustomDefinitions.Add(sExpName, new CustomizationDetails(cust));
                }

                //if (expansionCustomizationService.CustomDefinitions.ContainsKey(sExpName))
                //{
                //    expansionCustomizationService.CustomDefinitions[sExpName].Update(cust);
                //}
                //else if (_modDataService.farmExpansions.ContainsKey(sExpName))
                //{
                //    expansionCustomizationService.CustomDefinitions.Add(sExpName, new CustomizationDetails(cust));
                //}
            }
            catch { }
        }
        public string GetExpansionCustomizations(string sExpName)
        {
            if (_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                SDRCustomization cust = new SDRCustomization(_modDataService.CustomDefinitions[sExpName]);
                //SDRCustomization cust = new SDRCustomization(expansionCustomizationService.CustomDefinitions[sExpName]);
                return JsonConvert.SerializeObject(cust);
                //return new Dictionary<string, string>
                //{
                //    {"Artifacts", ExpansionCustomizations.CustomDefinitions[sExpName].Artifacts },
                //    {"Season", ExpansionCustomizations.CustomDefinitions[sExpName].SeasonOverride },
                //    {"SpringFish", ExpansionCustomizations.CustomDefinitions[sExpName].GetFishData("spring") },
                //    {"SummerFish", ExpansionCustomizations.CustomDefinitions[sExpName].GetFishData("summer") },
                //    {"FallFish", ExpansionCustomizations.CustomDefinitions[sExpName].GetFishData("fall") },
                //    {"WinterFish", ExpansionCustomizations.CustomDefinitions[sExpName].GetFishData("winter") },
                //    {"SpringForage", ExpansionCustomizations.CustomDefinitions[sExpName].GetForageData("spring") },
                //    {"SummerForage", ExpansionCustomizations.CustomDefinitions[sExpName].GetForageData("summer") },
                //    {"FallForage", ExpansionCustomizations.CustomDefinitions[sExpName].GetForageData("fall") },
                //    {"WinterForage", ExpansionCustomizations.CustomDefinitions[sExpName].GetForageData("winter") },
                //    {"AllowGiantCrops", ExpansionCustomizations.CustomDefinitions[sExpName].AllowGiantCrops.ToString() },
                //    {"AlwaysRaining", ExpansionCustomizations.CustomDefinitions[sExpName].AlwaysRaining.ToString() },
                //    {"AlwaysSnowing", ExpansionCustomizations.CustomDefinitions[sExpName].AlwaysSnowing.ToString() },
                //    {"AlwaysSunny", ExpansionCustomizations.CustomDefinitions[sExpName].AlwaysSunny.ToString() },
                //};
            }
            if (_modDataService.validContents.ContainsKey(sExpName))
            {
                SDRCustomization defdetails = new SDRCustomization(_modDataService.validContents[sExpName]);
                return JsonConvert.SerializeObject(defdetails);
            }

            return null;
        }

        public string GetExpansionThumbnailPath(string sExpName)
        {
            //
            //  returns the full path to the expansion pack mini-map
            //
            if (_modDataService.validContents.ContainsKey(sExpName) && !string.IsNullOrEmpty(_modDataService.validContents[sExpName].ThumbnailName))
            {
                return Path.Combine(_modDataService.validContents[sExpName].ModPath, "assets", _modDataService.validContents[sExpName].ThumbnailName);
            }

            return null;
        }

        //public bool AddBuilding(string locationName, BluePrint structureForPlacement, Vector2 tileLocation, Farmer who, bool complete)
        //{

        //    BuildableGameLocation glBuild = Game1.getLocationFromName(locationName) as BuildableGameLocation;

        //    if (glBuild.buildStructure(structureForPlacement, tileLocation, who, false, true))
        //    {
        //        Building jBuilding = glBuild.getBuildingAt(tileLocation);
        //        if (jBuilding == null)
        //        {
        //            logger.Log($"   Could find new building {locationName}.{structureForPlacement.displayName}", StardewModdingAPI.LogLevel.Debug);
        //            return false;
        //        }
        //        if (!jBuilding.modData.ContainsKey(FEModDataKeys.FELocationName))
        //        {
        //            jBuilding.modData.Add(FEModDataKeys.FELocationName, locationName);
        //            logger.Log($"   added expansion tag to building.", StardewModdingAPI.LogLevel.Debug);
        //        }
        //        if (jBuilding.indoors.Value != null && !jBuilding.indoors.Value.modData.ContainsKey(FEModDataKeys.FELocationName))
        //        {
        //            jBuilding.indoors.Value.modData.Add(FEModDataKeys.FELocationName, locationName);
        //            logger.Log($"   added expansion tag to building indoors.", StardewModdingAPI.LogLevel.Debug);
        //        }
        //        if (complete) jBuilding.daysOfConstructionLeft.Value = 0;
        //        if (jBuilding.indoors.Value != null && CustomBuildingManager.buildings.ContainsKey(jBuilding.nameOfIndoors))
        //        {
        //            jBuilding.indoors.Value.name.Value = CustomBuildingManager.buildings[jBuilding.nameOfIndoors].Name;
        //        }
        //        FEFramework.FixWarps(jBuilding, locationName);
        //        if (structureForPlacement.name == "Big Coop" || structureForPlacement.name == "Deluxe Coop")
        //        {
        //            //
        //            //  add the incubator
        //            //

        //            Coop coop = (Coop)jBuilding;

        //            coop.indoors.Value.moveObject(2, 3, 14, 8);
        //            coop.indoors.Value.moveObject(1, 3, 14, 7);
        //            coop.indoors.Value.moveObject(10, 4, 14, 4);
        //            coop.indoors.Value.objects.Add(new Vector2(2f, 3f), new SDObject(new Vector2(2f, 3f), 101));

        //        }
        //        else if (jBuilding is Stable stb)
        //        {
        //            stb.grabHorse();
        //        }
        //        return true;
        //    }

        //    return false;
        //}

        public string GetExpansionSeasonOverride(string sExpName)
        {
            if (_modDataService.CustomDefinitions.ContainsKey(sExpName))
            {
                return _modDataService.CustomDefinitions[sExpName].SeasonOverride;
            }

            return _modDataService.validContents[sExpName].SeasonOverride;
        }


        public void AddExpansionWarp(string locationName, string displayName, int TargetX, int TargetY)
        {
            warproomService.warproomManager.AddCaveEntrance(locationName, Tuple.Create(displayName, new EntranceDetails { WarpIn = new EntranceWarp { X = TargetX, Y = TargetY }, WarpOut = new EntranceWarp { X = TargetX, Y = TargetY } }));
        }
        public Dictionary<int, SDObject> GetBuildingProduction(Building building)
        {

            //            Building building = location.getBuildingAt(position);

            if (building == null || building.indoors.Value == null) return null;

            if (_customBuildingManager.CustomBuildings.ContainsKey(building.indoors.Value.Name))
            {
                return _customBuildingManager.CustomBuildings[building.indoors.Value.Name].GetBuildingProduction(building.indoors.Value);
            }

            return null;
        }
        //public List<string> GetCustomBuildingList()
        //{
        //    var list = CustomBuildings.buildings.Select(p => p.Value.Name).ToList();
        //     return list;
        //}
        public string GetBuildingMachineName(string buildingName)
        {
            if (_customBuildingManager.CustomBuildings.ContainsKey(buildingName))
            {
                return _customBuildingManager.CustomBuildings[buildingName].MachineName;
            }

            return null;
        }
    }
}

