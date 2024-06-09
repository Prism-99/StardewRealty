using StardewModdingAPI.Events;
using System.Collections.Generic;
using System.IO;
using System;
using StardewModHelpers;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.Locations;

namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class SDRDataProvider : IGameDataProvider
    {
        private ContentPackLoader ContentPacks;
        private IModHelper helper;
        private Dictionary<string, object> externalReferences;
        private Dictionary<string, string> translationDict;
        //private LocalizationStrings localizationStrings;
        public SDRDataProvider(ContentPackLoader cPacks, IModHelper ohelper, Dictionary<string, object> externalReferences, Dictionary<string, string> translationDict)
        {
            ContentPacks = cPacks;
            this.translationDict= translationDict;
            helper = ohelper;
            this.externalReferences = externalReferences;
        }
        public override string Name => "SDR";
        public override bool Handles(string assetName)
        {
            return assetName.StartsWith("SDR/");
        }
        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            string cleanAssetName = e.NameWithoutLocale.Name;
            string[] urlParts = cleanAssetName.Split('/');
            if (urlParts.Length > 1)
            {
                switch (urlParts[1])
                {
                    case "Buildings":
                        if (externalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return externalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"SDRDataProvider, could not find Building component: '{cleanAssetName}'", LogLevel.Debug);
                        }
                        break;
                    case "Objects":
                        if (externalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return externalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"SDRDataProvider, could not find Objects component: '{cleanAssetName}'", LogLevel.Debug);
                        }
                        break;
                    case WarproomManager.WarpRoomLoacationName:
                        if (externalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return externalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"SDRDataProvider, could not find sdr_warproom component: '{cleanAssetName}'", LogLevel.Debug);
                        }
                        break;
                    case "Expansion":
                        if (externalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return externalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"SDRDataProvider, could not find Expansion component: '{cleanAssetName}'", LogLevel.Debug);
                        }
                        break;
                    case "Maps":
                        string mapName = urlParts[2].Replace(".tmx","",StringComparison.InvariantCultureIgnoreCase);
                        if (ContentPacks.ExpansionMaps.ContainsKey(mapName))
                        {
                            e.LoadFrom(() => { return ContentPacks.ExpansionMaps[mapName]; }, AssetLoadPriority.Medium);
                        }
                        break;
                    case "images":
                        string imagePath = Path.Combine(helper.DirectoryPath, "data", "assets", "images", urlParts[2]);
                        if (File.Exists(imagePath))
                        {
                            e.LoadFrom(() => { return new StardewBitmap(imagePath).Texture(); }, AssetLoadPriority.Medium);
                        }
                        break;
                    case "bigcraftables":
                        if (externalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return new StardewBitmap(externalReferences[cleanAssetName].ToString()).Texture(); }, AssetLoadPriority.Medium);
                        }
                        break;
                    case "Strings":
                        e.LoadFrom(() => { return translationDict; }, AssetLoadPriority.Medium);
                        //localizationStrings.HandleEdit(e);
                        break;
                    case "movies":
                        if (externalReferences.ContainsKey(cleanAssetName))
                        {
                            logger.Log($"Getting movie item {externalReferences[cleanAssetName]}", LogLevel.Debug);
                            e.LoadFrom(() => { return new StardewBitmap(externalReferences[cleanAssetName].ToString()).Texture(); }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"SDRDataProvider, could not find movies component: '{cleanAssetName}'", LogLevel.Debug);
                        }
                        break;
                }
            }

        }

        public override void OnGameLaunched()
        {

        }
    }
}
