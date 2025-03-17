
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModHelpers;
using System;
using System.IO;
using System.Collections.Generic;
using xTile.Tiles;
using xTile;
using System.Linq;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using Microsoft.Xna.Framework.Graphics;

namespace SDV_Realty_Core.Framework.AssetUtils
{
    /// <summary>
    /// Handles assets to be accessed by the game
    /// -Map Strings
    /// -External References
    /// -Chair Tiles
    /// -NPC Tastes
    /// -Expansion Maps
    /// -Translation Strings
    /// </summary>
    internal class SDRContentManager
    {
        private ILoggerService logger;
        private IModHelperService helper;
        public Dictionary<string, string> stringFromMaps = null;
        //public Dictionary<string, object> ExternalReferences = new();
        public Dictionary<string, string> ChairTiles = new();
        public Dictionary<string, Dictionary<string, List<string>>> NPCTastes = new();
        public Dictionary<string, Map> Maps = new();
        private IModDataService modDataService;
        private IUtilitiesService utilitiesService;
        //internal LocalizationStrings localizationStrings;
        internal IStringService stringService;
        public SDRContentManager(IUtilitiesService utilitiesService, IModDataService modDataService)
        {
            this.utilitiesService = utilitiesService;
            logger = utilitiesService.logger;
            helper = utilitiesService.ModHelperService;
            this.modDataService = modDataService;
            Maps = modDataService.BuildingMaps;
            stringFromMaps = modDataService.stringFromMaps;
            //Initialize();
        }
        //public void Initialize()
        //{
        //    localizationStrings = new LocalizationStrings();
        //    localizationStrings.Intialize(helper.Translation, utilitiesService);
        //}
        public void AddMap(string mapName, Map map)
        {
            Maps.Add(mapName, map);
        }
        public void AddNPCTastes(string objectId, Dictionary<string, List<string>> tastes)
        {
            NPCTastes.Add(objectId, tastes);
        }
        public void AddExpansionFiles(string expansionKey)
        {
            try
            {
                //
                //  add map source file
                //
                if (modDataService.validContents[expansionKey].InternalWorldMapTexture)
                {
                    if (!string.IsNullOrEmpty(modDataService.validContents[expansionKey].WorldMapTexture))
                    {
                        string worldMap = $"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{expansionKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{utilitiesService.RemoveTextureExtensions(modDataService.validContents[expansionKey].WorldMapTexture)}";
                        logger?.Log($"Loaded WorldMap Texture: '{worldMap}' for expansion {expansionKey}", LogLevel.Trace);
                        //ExternalReferences.Add(worldMap, new StardewBitmap(Path.Combine(modDataService.validContents[expansionKey].ModPath, "assets", Path.GetFileName(modDataService.validContents[expansionKey].WorldMapTexture))).Texture());
                        if (modDataService.validContents[expansionKey].isAdditionalFarm)
                        {
                            try
                            {
                                modDataService.ExternalReferences.Add(worldMap, utilitiesService.ModHelperService.modHelper.GameContent.Load<Texture2D>(modDataService.validContents[expansionKey].WorldMapTexture));
                            }
                            catch
                            {
                                if (!modDataService.validContents[expansionKey].isAdditionalFarm)
                                    modDataService.validContents[expansionKey].WorldMapTexture = "";
                            }
                        }
                        else
                            modDataService.ExternalReferences[worldMap] = new StardewBitmap(Path.Combine(modDataService.validContents[expansionKey].ModPath, "assets", modDataService.validContents[expansionKey].WorldMapTexture)).Texture();
                    }
                    foreach (string mapTexture in modDataService.validContents[expansionKey].SeasonalWorldMapTextures.Keys)
                    {
                        string worldMap = $"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{expansionKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{utilitiesService.RemoveTextureExtensions(modDataService.validContents[expansionKey].SeasonalWorldMapTextures[mapTexture])}";
                        logger?.Log($"Loaded WorldMap Texture: '{worldMap}' for expansion {expansionKey}", LogLevel.Trace);
                        //ExternalReferences.Add(worldMap, new StardewBitmap(Path.Combine(modDataService.validContents[expansionKey].ModPath, "assets", Path.GetFileName(modDataService.validContents[expansionKey].WorldMapTexture))).Texture());
                        modDataService.ExternalReferences.Add(worldMap, new StardewBitmap(Path.Combine(modDataService.validContents[expansionKey].ModPath, "assets", modDataService.validContents[expansionKey].SeasonalWorldMapTextures[mapTexture])).Texture());
                    }
                }
                Map expansionMap = modDataService.ExpansionMaps[utilitiesService.GetMapUniqueName(modDataService.validContents[expansionKey])];
                if (!string.IsNullOrEmpty(modDataService.validContents[expansionKey].MapName))
                {
                    string sMapPath = $"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{expansionKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{modDataService.validContents[expansionKey].MapName}";
                    logger?.Log($"Loaded Map: '{sMapPath}' for expansion {expansionKey}", LogLevel.Trace);
                    modDataService.ExternalReferences.Add(sMapPath, expansionMap);
                }
                //
                //  check for seat tiles
                //
                if (modDataService.validContents[expansionKey].SeatTiles != null)
                {
                    foreach (KeyValuePair<string, string> tile in modDataService.validContents[expansionKey].SeatTiles)
                    {
                        AddChairTile(tile.Key, tile.Value);
                    }
                }
                foreach (TileSheet tileSheet in expansionMap.TileSheets)
                {
                    if (string.IsNullOrEmpty(tileSheet.ImageSource)) logger?.Log($"Tilesheet: {tileSheet.ImageSource}", LogLevel.Debug);
                    if (!string.IsNullOrEmpty(tileSheet.ImageSource))
                    {
                        tileSheet.ImageSource = tileSheet.ImageSource.Replace("\\", FEConstants.AssetDelimiter);
                        // fix up old expansion maps
                        if (tileSheet.ImageSource.StartsWith($"Maps{FEConstants.AssetDelimiter}femaps{FEConstants.AssetDelimiter}{expansionKey}", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string[] arParts = tileSheet.ImageSource.Split(FEConstants.AssetDelimiter);
                            tileSheet.ImageSource = $"SDR{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}" + arParts[arParts.Length - 1];
                        }
                        if (tileSheet.ImageSource.StartsWith($"SDR{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tileSheet.ImageSource = $"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{expansionKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{Path.GetFileNameWithoutExtension(tileSheet.ImageSource)}";
                            string rawSource = Path.Join(modDataService.validContents[expansionKey].ModPath, "assets", Path.GetFileName(tileSheet.ImageSource) + ".png");
                            modDataService.ExternalReferences.Add(tileSheet.ImageSource, new StardewBitmap(rawSource).Texture());
                            //if (Path.GetFileName(tileSheet.ImageSource) != Path.GetFileNameWithoutExtension(tileSheet.ImageSource))
                            //{
                            //    int end = tileSheet.ImageSource.LastIndexOf('.');
                            //    if (end > -1)
                            //    {
                            //        ExternalReferences.Add(tileSheet.ImageSource.Substring(0, end - 1), new StardewBitmap(rawSource).Texture());
                            //    }
                            //}
                            logger?.Log($"Loaded TileSheet texture '{tileSheet.ImageSource}' for expansion {expansionKey}", LogLevel.Trace);
                            //
                            //  check for seasonal tilesheets
                            //
                            string filename = Path.GetFileNameWithoutExtension(tileSheet.ImageSource);
                            string[] seasons = new string[] { "spring_", "summer_", "fall_", "winter_" };
                            int seasonPointer = 0;
                            if (filename.StartsWith(seasons[seasonPointer], StringComparison.CurrentCultureIgnoreCase))
                            {
                                while (seasonPointer < seasons.Length - 1)
                                {
                                    int divider = filename.IndexOf("_");
                                    if (divider > -1 && seasons.Contains(filename.Substring(0, divider + 1).ToLower()))
                                    {
                                        //filename = filename.Replace(seasons[seasonPointer], seasons[seasonPointer + 1], StringComparison.CurrentCultureIgnoreCase);
                                        filename = seasons[seasonPointer + 1] + filename.Substring(divider + 1);

                                        rawSource = Path.Join(modDataService.validContents[expansionKey].ModPath, "assets", filename + ".png");
                                        if (File.Exists(rawSource))
                                        {
                                            string assetFinalName = $"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{expansionKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{filename}";
                                            modDataService.ExternalReferences.Add(assetFinalName, new StardewBitmap(rawSource).Texture());
                                            logger?.Log($"Loaded Seasonal TileSheet: '{assetFinalName}' for expansion {expansionKey}", LogLevel.Trace);
                                        }
                                        else
                                        {
                                            logger?.Log($"Missing Seasonal TileSheet: '{rawSource}' for expansion {expansionKey}", LogLevel.Warn);
                                        }
                                    }
                                    seasonPointer++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.Log($"Error loading expansion {expansionKey}", LogLevel.Error);
                logger?.LogError(ex);
            }

        }
        public void AddAllExpansionFiles()
        {
            //
            //  add expansions
            //
            foreach (string expansionKey in modDataService.validContents.Keys)
            {
                AddExpansionFiles(expansionKey);
            }

        }

        public void AddChairTile(string tileRef, string seatDetails)
        {
            ChairTiles.Add(tileRef, seatDetails);
        }
        //private void CheckForLooseFiles(AssetRequestedEventArgs e, string cleanAssetName)
        //{
        //    string sModName = "";
        //    string normName = SDVPathUtilities.NormalizeKey(cleanAssetName);

        //    //
        //    //  custom locations added by the mod
        //    //
        //    string[] assetNameParts = normName.Split(FEConstants.AssetDelimiter[0]);
        //    if (assetNameParts.Length > 2)
        //    {
        //        sModName = assetNameParts[2];
        //    }
        //    //}

        //    if (!string.IsNullOrEmpty(sModName))
        //    {

        //        string fileExtension = ".png";
        //        if (Path.GetExtension(normName) != "") fileExtension = "";
        //        string sAssetPath = normName.Replace(FEConstants.AssetDelimiter, Path.DirectorySeparatorChar.ToString()) + fileExtension;

        //        sAssetPath = sAssetPath.Replace($"Maps{Path.DirectorySeparatorChar}femaps{Path.DirectorySeparatorChar}{sModName}{Path.DirectorySeparatorChar}", "");
        //        string filePath;
        //        if (sModName == "SDR")
        //        {
        //            //sPath = SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}stardewrealty{FEConstants.AssetDelimiter}WorldMap.png");
        //            filePath = Path.Combine(helper.DirectoryPath, "data", "assets", sAssetPath);
        //        }
        //        else
        //        {
        //            if (modDataService.validContents.ContainsKey(sModName))
        //            {
        //                logger.Log($"    Mod Name: {sModName}", LogLevel.Debug);
        //                filePath = Path.Combine(modDataService.validContents[sModName].ModPath, "assets", sAssetPath);
        //            }
        //            else
        //            {
        //                filePath = sAssetPath;
        //            }
        //        }
        //        try
        //        {
        //            switch (Path.GetExtension(filePath))
        //            {
        //                case ".png":
        //                case "":
        //                    // return asset png

        //                    if (modDataService.ExpansionMaps.ContainsKey(sModName))
        //                    {
        //                        e.LoadFrom(() => { return new StardewBitmap(filePath).Texture(); }, AssetLoadPriority.Medium);
        //                    }
        //                    else if (ExternalReferences.ContainsKey(sModName + "." + filePath))
        //                    {
        //                        e.LoadFrom(() => { return new StardewBitmap(ExternalReferences[sModName + "." + filePath].ToString()).Texture(); }, AssetLoadPriority.Medium);
        //                    }
        //                    else if (File.Exists(filePath))
        //                    {
        //                        e.LoadFrom(() => { return new StardewBitmap(filePath).Texture(); }, AssetLoadPriority.Medium);
        //                    }
        //                    break;
        //                case ".tbin":
        //                case ".tmx":
        //                    // get pre-loaded map file

        //                    if (modDataService.ExpansionMaps.ContainsKey(sModName))
        //                    {
        //                        e.LoadFrom(() => { return modDataService.ExpansionMaps[sModName]; }, AssetLoadPriority.Medium);
        //                    }
        //                    else if (ExternalReferences.ContainsKey(sModName))
        //                    {
        //                        e.LoadFrom(() => { return ExternalReferences[sModName]; }, AssetLoadPriority.Medium);
        //                    }
        //                    break;
        //                default:
        //                    // asume is png and serve asset png
        //                    if (File.Exists(filePath))
        //                    {
        //                        e.LoadFrom(() => { return new StardewBitmap(filePath).Texture(); }, AssetLoadPriority.Medium);
        //                    }
        //                    break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.LogError($"CheckForLooseFiles.  Asset={normName}", ex);
        //        }
        //    }
        //}
        //public void RemoveStringFromMap(string assetName)
        //{
        //    stringFromMaps.Remove(assetName);
        //}
        //public void UpsertStringFromMap(string assetName, string assValue)
        //{
        //    if (stringFromMaps.ContainsKey(assetName))
        //    {
        //        stringFromMaps[assetName] = assValue;
        //    }
        //    else
        //    {
        //        stringFromMaps.Add(assetName, assValue);
        //    }
        //}
    }
}