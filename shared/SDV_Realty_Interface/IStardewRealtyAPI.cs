using System;
using System.Collections.Generic;
using StardewValley.Buildings;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Buildings;

namespace StardewRealty.SDV_Realty_Interface
{
    /// <summary>An API provided by Farm Expansion for other mods to use.</summary>
    public interface IStardewRealtyAPI
    {
        //public struct SDRCustomization
        //{
        //    public string ExpansionName;
        //    public string SpringForage;
        //    public string SummerForage;
        //    public string FallForage;
        //    public string WinterForage;
        //    public string SpringFish;
        //    public string SummerFish;
        //    public string FallFish;
        //    public string WinterFish;
        //    public string Artifacts;
        //    public bool StockedPonds;
        //    public string Season;
        //}
        //public class SDRCustomization
        //{
        //    public string ExpansionName { get; set; }
        //    public string SpringForage { get; set; }
        //    public string SummerForage { get; set; }
        //    public string FallForage { get; set; }
        //    public string WinterForage { get; set; }
        //    public string SpringFish { get; set; }
        //    public string SummerFish { get; set; }
        //    public string FallFish { get; set; }
        //    public string WinterFish { get; set; }
        //    public string Artifacts { get; set; }
        //    public bool StockedPonds { get; set; }
        //    public string Season { get; set; }
        //}

        public bool IsSDRMachine(string machineName);
        public List<SpawnFishData> GetFishData(string expansionName, string fishAreaId);
        /// <summary>
        /// Gets a list of Fish areas defined for the expansion
        /// </summary>
        /// <param name="expansionName"></param>
        /// <returns><list type="string">FishAreaId</list></returns>
        //

        /// <summary>Add a blueprint to all future carpenter menus for the farm area.</summary>
        /// <param name="blueprint">The blueprint to add.</param>
        void AddFarmBluePrint(BuildingData blueprint);

        /// <summary>Add a blueprint to all future carpenter menus for the expansion area.</summary>
        /// <param name="blueprint">The blueprint to add.</param>
        void AddExpansionBluePrint(BuildingData blueprint);
        void AddFishToArea(string expansionName, string areaId, string season, string fishId, float chance);
        void DeleteFishFromArea(string expansionName, string areaId, string season, string fishId, float chance);
        public Dictionary<string,Tuple<int,int,int,int>> GetFishAreaDetails(string expansionName);
        /// <summary>
        /// Mod removes itself from game world in BeforeSave and handles saving separately.
        /// Hook this if you need to do some fixup to contained stuff (FurnitureAnywhere, Tractor etc).
        /// </summary>
        /// <param name="handler"></param>
        void AddRemoveListener(EventHandler handler);

        /// <summary>
        /// Second half for AddRemoveListener
        /// </summary>
        void AddAppendListener(EventHandler handler);
        bool IsExpansion(string locationName);
        void SetExpansionWeather(string sExpName, string weather);
        void AllowGiantCrops(string sExpName,bool allowGiantCrops);
        void AllowCrows(string sExpName,bool allowCrows);
        void DeleteArtifactEntry(string expName, string artifactId, float chance, string season);
        void AddArtifactEntry(string expName, string artifactId, float chance, string season);
        void DeleteForageEntry(string expName, string forageId, float chance, string season);
        void AddForageEntry(string expName, string forageId, float chance, string season);
        void DeleteFishEntry(string expName, string fishId, float chance, string season);
        void AddFishEntry(string expName, string fishId, float chance, string season);
        //
        //  get list of installed expansions
        //
        List<string> GetInstalledExpansionNames();
        //
        //  get list of possible location names
        //
        List<string> GetLocationNames();
        Dictionary<string, string> GetCustomBuildingList();

        string GetModList();

        //Dictionary<string, Dictionary<string, string>> GetModList();

        Dictionary<int, string> GetMapGrid();

        string GetExpansionCustomizations(string sExpName);
        void SetExpansionCustomizations(string sExpName, string jsonData);
        string GetExpansionDefinition(string sExpName);
        string SetExpansionDefinition(string sExpName, string sDefinition);
        bool GetExpansionStockedPond(string sExpName);
        void SetExpansionStockedPond(string sExpName, bool stockedPond);
        bool GetExpansionCrowsEnabled(string sExpName);
        void SetExpansionCrowsEnabled(string sExpName, bool crowsEnabled);
        string GetExpansionSeasonOverride(string sExpName);
        void SetExpansionSeasonOverride(string sExpName, string seasonOverride);
        string GetExpansionThumbnailPath(string sExpName);
        void AddExpansionWarp(string locationName, string DisplayName, int TargetX, int TargetY);
        Dictionary<int, SDObject> GetBuildingProduction(Building building);
        //public List<string> GetCustomBuildingList();
        public string GetBuildingMachineName(string buildingName);
        bool AddBuilding(string locationName, BuildingData structureForPlacement, Vector2 tileLocation, Farmer who, bool complete);
    }
}