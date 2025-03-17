using Microsoft.Xna.Framework.Graphics;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using SDV_Realty_Core.Framework.ServiceInterfaces.GUI;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using StardewModdingAPI.Utilities;
using SDV_Realty_Core.Framework.Menus;


namespace SDV_Realty_Core.Framework.ServiceProviders.GUI
{
    /// <summary>
    /// Manages MiniMap data for drawing the minimap
    /// on the worldmap menus
    /// </summary>
    internal class MiniMapManagerService : IMiniMapService
    {
        private IModDataService _modDataService;
        private IUtilitiesService _utilitiesService;
        private const string saveKey = "prism99.sdr.minimap";
        private List<MiniMapEntry>? customEntries;
        private int miniGridStartingIndex = -6;
        public override Type[] InitArgs => new Type[]
    {
                typeof(IModDataService),typeof(IUtilitiesService),
                typeof(ISaveManagerService),typeof(IInputService)
    };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;

            _modDataService = (IModDataService)args[0];
            _utilitiesService = (IUtilitiesService)args[1];
            ISaveManagerService saveManagerService = (ISaveManagerService)args[2];
            IInputService inputService = (IInputService)args[3];

            inputService.AddKeyBind(_modDataService.Config.MiniMapKey, HandleHotKey);
            saveManagerService.AddLoadedHook(LoadMiniMapDetails);
        }
        /// <summary>
        /// Pop CustomWarpMenu when activation key pushed
        /// </summary>
        /// <param name="keybind">Pre-filtered keybind</param>
        private void HandleHotKey(KeybindList keybind)
        {
            Game1.activeClickableMenu = new CustomWarpMenu(0, 0, _modDataService, SaveDestinationEdits);
        }
        /// <summary>
        /// Save edited MiniMap entries and reloads the menu display
        /// </summary>
        private void SaveDestinationEdits()
        {
            _utilitiesService.ModHelperService.modHelper.Data.WriteSaveData(saveKey, _modDataService.CustomMiniMapEntries);
            LoadMiniMapDetails();
            _utilitiesService.InvalidateCache("Data/WorldMap");
        }
        /// <summary>
        /// Returns the texture name for the supplied gridId
        /// </summary>
        /// <param name="gridId">GridId for comparison</param>
        /// <returns>Texture name</returns>
        private string GetCustomMapPath(int gridId)
        {
            return gridId switch
            {
                1 => "custom-b",
                2 => "custom-c",
                3 => "custom-d",
                4 => "custom-e",
                _ => "custom-a"
            };
        }
        /// <summary>
        /// Load base and custom MiniMap entries
        /// </summary>
        private void LoadMiniMapDetails()
        {
            _modDataService.MiniMapGrid.Clear();
            _modDataService.MiniMapGrid.Add(-1, new MiniMapEntry
            {
                DisplayName = "Home",
                GridId = -1,
                Warp = _utilitiesService.GetPlayerHomeWarp(),
                Key = "home",
                TexturePath = Path.Combine("data", "assets", "images", "WorldMap", "home")
            });
            _modDataService.MiniMapGrid.Add(-2, new MiniMapEntry
            {
                DisplayName = "Stardew Meadows",
                GridId = -2,
                Warp = new MapWarp(0, 0, WarproomManager.StardewMeadowsLoacationName, WarproomManager.StardewMeadowsSoutherEntrancePoint.X, WarproomManager.StardewMeadowsSoutherEntrancePoint.Y, false),
                Key = WarproomManager.StardewMeadowsLoacationName,
                TexturePath = Path.Combine("data", "assets", "images", "WorldMap", "meadows")

            });
            _modDataService.MiniMapGrid.Add(-3, new MiniMapEntry
            {
                DisplayName = "Town",
                GridId = -3,
                Warp = new MapWarp(0, 0, "Town", 30, 58, false),
                Key = "town",
                TexturePath = Path.Combine("data", "assets", "images", "WorldMap", "town")

            });
            _modDataService.MiniMapGrid.Add(-4, new MiniMapEntry
            {
                DisplayName = "Carpenter",
                GridId = -4,
                Warp = new MapWarp(0, 0, "Mountain", 14, 27, false),
                Key = "carpenter",
                TexturePath = Path.Combine("data", "assets", "images", "WorldMap", "carpenter")

            });
            _modDataService.MiniMapGrid.Add(-5, new MiniMapEntry
            {
                DisplayName = "Animals",
                GridId = -5,
                Warp = new MapWarp(0, 0, "Forest", 90, 17, false),
                Key = "animals",
                TexturePath = Path.Combine("data", "assets", "images", "WorldMap", "animals")

            });
            //
            //  load custom warps
            //
            try
            {
                _modDataService.CustomMiniMapEntries = _utilitiesService.ModHelperService.modHelper.Data.ReadSaveData<List<IMiniMapService.MiniMapEntry>>(saveKey);
            }
            catch { }
            if (_modDataService.CustomMiniMapEntries == null)
                _modDataService.CustomMiniMapEntries = new();

            for (int index = 0; index < 5; index++)
            {
                var cutsomEntry = _modDataService.GetCustomMiniMapEntry(index);
                if (cutsomEntry != null)
                {
                    _modDataService.MiniMapGrid.Add(miniGridStartingIndex - index, new MiniMapEntry
                    {
                        DisplayName = cutsomEntry.DisplayName,
                        GridId = miniGridStartingIndex - index,
                        Warp = cutsomEntry.Warp,
                        Key = cutsomEntry.Key,
                        TexturePath = Path.Combine("data", "assets", "images", "WorldMap", GetCustomMapPath(index))
                    });
                }
            }

            //
            //  load textures
            //
            foreach (var entry in _modDataService.MiniMapGrid.Values)
            {
                if (entry.Texture == null && !string.IsNullOrEmpty(entry.TexturePath))
                {
                    try
                    {
                        entry.Texture = _utilitiesService.ModHelperService.modHelper.ModContent.Load<Texture2D>(entry.TexturePath);
                        _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}WorldMap{FEConstants.AssetDelimiter}{entry.Key}", entry.Texture);
                        _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}WorldMap{FEConstants.AssetDelimiter}{entry.Key}_spring", entry.Texture);
                        _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}WorldMap{FEConstants.AssetDelimiter}{entry.Key}_summer", entry.Texture);
                        _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}WorldMap{FEConstants.AssetDelimiter}{entry.Key}_fall", entry.Texture);
                        _modDataService.ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}WorldMap{FEConstants.AssetDelimiter}{entry.Key}_winter", entry.Texture);
                    }
                    catch { }
                }
            }
        }
    }
}
