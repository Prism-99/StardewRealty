using StardewModdingAPI.Events;
using System.IO;
using System;
using StardewModHelpers;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.ServiceProviders.ModMechanics;
using System.Linq;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using Microsoft.Xna.Framework.Graphics;

namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class SDRDataProvider : IGameDataProvider
    {
        private IModHelper helper;
        private IModDataService modDataService;
        public SDRDataProvider(IModDataService modDataService, IModHelper modHelper)
        {
            helper = modHelper;
            this.modDataService = modDataService;
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
                        if (modDataService.ExternalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return modDataService.ExternalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"Could not find Building asset: '{cleanAssetName}'", LogLevel.Debug);
                        }
                        break;
                    case "Objects":
                        if (modDataService.ExternalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return modDataService.ExternalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"Could not find Objects asset: '{cleanAssetName}'", LogLevel.Debug);
                        }
                        break;
                    case WarproomManager.StardewMeadowsLoacationName:
                    //case ExpansionBridgingService.BridgingLocationName:
                        if (modDataService.ExternalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return modDataService.ExternalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"SDRDataProvider, could not find sdr_warproom component: '{cleanAssetName}'", LogLevel.Debug);
                        }
                        break;
                    case "Expansion":
                        if (modDataService.ExternalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return modDataService.ExternalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.LogOnce($"Could not find Expansion asset: '{cleanAssetName}'", LogLevel.Trace);
                        }
                        break;
                    case "WorldMap":
                        // return a Texture2D object
                        string textureName = string.Join('/', urlParts.Skip(2).Take(urlParts.Length - 2)).Replace(".tmx", "", StringComparison.InvariantCultureIgnoreCase);
                        if (modDataService.ExternalReferences.TryGetValue($"{cleanAssetName}_{Game1.season.ToString().ToLower()}", out var expansionTexture))
                        {
                            e.LoadFrom(() => (Texture2D)expansionTexture, AssetLoadPriority.Medium);
                        }
                        else if (modDataService.ExternalReferences.TryGetValue(cleanAssetName, out var expansionTexture2))
                        {
                            e.LoadFrom(() => (Texture2D)expansionTexture2, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            //  provide an error image to stop game from throwing
                            //  error fit
                            logger.Log($"Could not find WorldMap asset: '{cleanAssetName}'",LogLevel.Error);
                        }
                        break;
                    case "Maps":
                        // return a Map object
                        string mapName = string.Join('/', urlParts.Skip(2).Take(urlParts.Length - 2)).Replace(".tmx", "", StringComparison.InvariantCultureIgnoreCase);
                        if (modDataService.ExpansionMaps.ContainsKey(mapName))
                        {
                            e.LoadFrom(() => { return modDataService.ExpansionMaps[mapName]; }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"Could not find Maps asset: '{cleanAssetName}'",LogLevel .Error);
                        }
                        break;
                    case "images":
                        // return a Texture2D object
                        string imagePath = Path.Combine(helper.DirectoryPath, "data", "assets", "images", urlParts[2]);
                        if (modDataService.ExternalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return new StardewBitmap(modDataService.ExternalReferences[cleanAssetName].ToString()).Texture(); }, AssetLoadPriority.Medium);
                        }
                        else if (File.Exists(imagePath))
                        {
                            // should not be used, but keeping until
                            // verified not being used
                            logger.Log($"Image using old calling method: {imagePath}", LogLevel.Debug);
                            e.LoadFrom(() => { return new StardewBitmap(imagePath).Texture(); }, AssetLoadPriority.Medium);
                        }
                        break;
                    case "bigcraftables":
                        if (modDataService.ExternalReferences.ContainsKey(cleanAssetName))
                        {
                            e.LoadFrom(() => { return new StardewBitmap(modDataService.ExternalReferences[cleanAssetName].ToString()).Texture(); }, AssetLoadPriority.Medium);
                        }
                        break;
                    case "Strings":
                        e.LoadFrom(() => { return modDataService.TranslationDict; }, AssetLoadPriority.Medium);
                        //localizationStrings.HandleEdit(e);
                        break;
                    case "movies":
                        if (modDataService.ExternalReferences.ContainsKey(cleanAssetName))
                        {
                            logger.Log($"Getting movie item {modDataService.ExternalReferences[cleanAssetName]}", LogLevel.Debug);
                            e.LoadFrom(() => { return new StardewBitmap(modDataService.ExternalReferences[cleanAssetName].ToString()).Texture(); }, AssetLoadPriority.Medium);
                        }
                        else
                        {
                            logger.Log($"Could not find Movies asset: '{cleanAssetName}'", LogLevel.Debug);
                        }
                        break;
                }
            }

        }
        private string AppendFileExtension(string fileName, string extension)
        {
            if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return $"{fileName}{extension}";
            }

            return fileName;
        }
        public override void OnGameLaunched()
        {

        }
    }
}
