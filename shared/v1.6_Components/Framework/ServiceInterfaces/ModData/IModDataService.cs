using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.Objects;
using StardewRealty.SDV_Realty_Interface;
using SDV_Realty_Core.Framework.ServiceInterfaces.Services;
using System;
using System.Collections.Generic;
using xTile;
using SDict = Prism99_Core.Objects.SerializableDictionary<string, SDV_Realty_Core.Framework.Expansions.FarmExpansionLocation>;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using System.Linq;
using StardewValley.GameData.Minecarts;
using SDV_Realty_Core.Framework.ServiceProviders.ModData;

namespace SDV_Realty_Core.Framework.ServiceInterfaces.ModData
{
    /// <summary>
    /// Central data service for all mod shared data
    /// </summary>
    internal abstract class IModDataService : IService
    {
        public readonly SDict farmExpansions = new();
        public readonly Dictionary<string, ExpansionDetails> expDetails = new();
        public readonly Dictionary<string, ExpansionPack> validContents = new();
        public readonly Dictionary<string, string> stringFromMaps = new();
        public readonly Dictionary<string, Map> ExpansionMaps = new();
        public readonly Dictionary<string, Map> BuildingMaps = new();
        public readonly Dictionary<string, GameLocation> SubLocations = new();
        public readonly Dictionary<string, CustomizationDetails> CustomDefinitions = new();
        public readonly Dictionary<int, string> MapGrid = new();
        public readonly Dictionary<string, string> TranslationDict = new();
        public readonly Dictionary<string, string> CustomMail = new();
        public readonly Dictionary<string, string> MineCartNetworksToEdit = new();
        public readonly Dictionary<string, MinecartNetworkData> CustomMineCartNetworks = new();
        public readonly List<string> LandForSale = new List<string>();
        public readonly Dictionary<string, object> ExternalReferences = new();
        public readonly Dictionary<int, IMiniMapService.MiniMapEntry> MiniMapGrid = new();
        public readonly List<string> addedLocationTracker = new List<string>();
        public List<IMiniMapService.MiniMapEntry> CustomMiniMapEntries = new();
        public FEConfig Config;
        public ModStateValues ModState { get; protected set; } = new();
        public List<StationDetails> Stations = new();
        public Dictionary<string, Tuple<int, int>> assetRequests = new();

        public readonly int MaximumExpansions = 13;
        public override Type ServiceType => typeof(IModDataService);
        public abstract void SaveModState();

        public IMiniMapService.MiniMapEntry? GetCustomMiniMapEntry(int index)
        {
            return CustomMiniMapEntries.Where(p => p.Key == $"custom{index}").FirstOrDefault();
        }
        public void DeleteMiniMapCustomEntry(int index)
        {
            var entry = GetCustomMiniMapEntry(index);
            if (entry != null)
                CustomMiniMapEntries.Remove(entry);
        }
        public void InsertCustomMiniMapEntry(int index, IMiniMapService.MiniMapEntry entry)
        {
            DeleteMiniMapCustomEntry(index);
            entry.Key = $"custom{index}";
            CustomMiniMapEntries.Add(entry);
        }
        public bool TryGetExpansionPack(string locationName,out ExpansionPack expansionPack)
        {
            bool result = false;
            expansionPack = null;

            if (validContents.TryGetValue(locationName, out expansionPack)) 
            { 
                result = true;
            }
            else
            {
                var subLocation=validContents.Where(p=>p.Value.SubLocations.Contains(locationName));
                if (subLocation.Any()) 
                { 
                    expansionPack= subLocation.First().Value;
                    result=true;
                }
            }
            return result;
        }
    }
}
