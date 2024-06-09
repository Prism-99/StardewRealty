using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.DataProviders;
using System.IO;

namespace SDV_Realty_Core.Framework.AssetUtils
{
    //
    //  version 1.6
    //
    internal partial class SDRContentManager
    {
        internal LocalizationStrings localizationStrings;
   
        private void VersionSpecificSetup()
        {
            // added for Services
            localizationStrings = new LocalizationStrings();
            localizationStrings.Intialize(helper.Translation);
            // disabled for Services
            //dataProviderManager.SetLocalizationString(localizationStrings);
            //
            //  add version data handlers
            //
            //  graphics moved to WorldMapDataProviders
            //ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap.png", Path.Combine(helper.DirectoryPath,"data","assets","worldmap.png"));
            //ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForSale.png", Path.Combine(helper.DirectoryPath, "data", "assets", "WorldMap_ForSale.png"));
            //ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ForFuture.png", Path.Combine(helper.DirectoryPath, "data", "assets", "WorldMap_ForFuture.png"));
            //ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}images{FEConstants.AssetDelimiter}WorldMap_ComingSoon.png", Path.Combine(helper.DirectoryPath, "data", "assets", "WorldMap_ComingSoon.png"));
            ////
            ////  add expansions
            ////
            //foreach (string expansionKey in ContentPacks.ExpansionMaps.Keys)
            //{
            //    //
            //    //  add map source file
            //    //
            //    if (!string.IsNullOrEmpty(ContentPacks.ValidContents[expansionKey].WorldMapTexture))
            //    {
            //        //string worldMap = SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}{expansionKey}{FEConstants.AssetDelimiter}{ContentPacks.ValidContents[expansionKey].WorldMapTexture}");
            //        string worldMap = $"SDR{FEConstants.AssetDelimiter}Expansion{FEConstants.AssetDelimiter}{expansionKey}{FEConstants.AssetDelimiter}assets{FEConstants.AssetDelimiter}{ContentPacks.ValidContents[expansionKey].WorldMapTexture}";
            //        //AssetsServed.Add(worldMap, expansionKey);
            //        ExternalReferences.Add(worldMap, new StardewBitmap(Path.Combine( ContentPacks.ValidContents[expansionKey].Owner.DirectoryPath,"assets",Path.GetFileName(ContentPacks.ValidContents[expansionKey].WorldMapTexture))).Texture());
            //    }
            //    if (!string.IsNullOrEmpty(ContentPacks.ValidContents[expansionKey].MapName))
            //    {
            //        string sMapPath = SDVPathUtilities.NormalizePath($"{FEConstants.MapPathPrefix}{expansionKey}{FEConstants.AssetDelimiter}{ContentPacks.ValidContents[expansionKey].MapName}");
            //        AssetsServed.Add(sMapPath, expansionKey);

            //        logger?.Log($"Served image= cp: {expansionKey}, key: {sMapPath}", LogLevel.Debug);
            //    }
            //    Map expansionMap = ContentPacks.ExpansionMaps[expansionKey];
            //    foreach (TileSheet tileSheet in expansionMap.TileSheets)
            //    {
            //        if (string.IsNullOrEmpty(tileSheet.ImageSource)) logger?.Log($"Tilesheet: {tileSheet.ImageSource}", LogLevel.Debug);
            //        if (!string.IsNullOrEmpty(tileSheet.ImageSource))
            //        {
            //            tileSheet.ImageSource = tileSheet.ImageSource.Replace("\\", "/");
            //            // fix up old expansion maps
            //            if (tileSheet.ImageSource.StartsWith($"Maps{FEConstants.AssetDelimiter}femaps{FEConstants.AssetDelimiter}{expansionKey}"))
            //            {
            //                tileSheet.ImageSource = tileSheet.ImageSource.Replace($"Maps{FEConstants.AssetDelimiter}femaps", $"SDR{FEConstants.AssetDelimiter}Expansion");
            //            }
            //            if (tileSheet.ImageSource.StartsWith("SDR/"))
            //            {
            //                logger?.Log($"Added image= cp: {expansionKey}, key: {tileSheet.ImageSource}", LogLevel.Debug);
            //                AssetsServed.Add(tileSheet.ImageSource, expansionKey);
            //                string rawSource =Path.Join( ContentPacks.ValidContents[expansionKey].Owner.DirectoryPath,"assets",Path.GetFileName( tileSheet.ImageSource)+".png");
            //                ExternalReferences.Add(tileSheet.ImageSource, new StardewBitmap(rawSource).Texture() );
            //            }
            //        }
            //    }
            //}
            //
            //  add big craftable texture details
            //
            //foreach (var bigCraftable in customBigCraftableManager.BigCraftables)
            //{
            //    if (!string.IsNullOrEmpty(bigCraftable.BigCraftableData.Texture))
            //    {
            //        //
            //        //  fully qualify name to ensure uniqueness
            //        //
            //        bigCraftable.BigCraftableData.Texture = $"SDR/bigcraftables/{bigCraftable.Id}/{bigCraftable.BigCraftableData.Texture}";
            //        // add bigcraftable Texture to items served
            //        ExternalReferences.Add(bigCraftable.BigCraftableData.Texture, Path.Combine(bigCraftable.ModPath, Path.GetFileName(bigCraftable.BigCraftableData.Texture)));
            //    }
            //}
        }

        //private void Content_AssetRequested(object? sender, AssetRequestedEventArgs e)
        //{
        //    //
        //    //  edit handled by standard handlers
        //    //
        //    if (dataProviderManager.HandleDataEdit(e))
        //        return;

        //    //
        //    //  check for speciality handling
        //    //
        //    string cleanAssetName = e.NameWithoutLocale.Name;
        //    switch (cleanAssetName)
        //    {
        //        case string map_name when map_name.StartsWith("Maps"):
        //            if (ExternalReferences.ContainsKey(cleanAssetName))
        //            {
        //                e.LoadFrom(() => { return ExternalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
        //            }
        //            break;
        //        case string event_name when event_name.StartsWith("Data/Events"):

        //            break;
        //        case string sdr_item when sdr_item.StartsWith("SDR/"):
        //            string[] urlParts = sdr_item.Split('/');
        //            if (urlParts.Length > 1)
        //            {
        //                switch (urlParts[1])
        //                {
        //                    case "Buildings":
        //                        if (ExternalReferences.ContainsKey(cleanAssetName))
        //                        {
        //                            e.LoadFrom(() => { return ExternalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
        //                        }
        //                        else
        //                        {
        //                            logger.Log($"CMan, could not find Building component: '{cleanAssetName}'", LogLevel.Debug);
        //                        }
        //                        break;
        //                    case "Objects":
        //                        if (ExternalReferences.ContainsKey(cleanAssetName))
        //                        {
        //                            e.LoadFrom(() => { return ExternalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
        //                        }
        //                        else
        //                        {
        //                            logger.Log($"CMan, could not find Objects component: '{cleanAssetName}'", LogLevel.Debug);
        //                        }
        //                        break;
        //                    case WarproomManager.WarpRoomLoacationName:
        //                        if (ExternalReferences.ContainsKey(cleanAssetName))
        //                        {
        //                            e.LoadFrom(() => { return ExternalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
        //                        }
        //                        else
        //                        {
        //                            logger.Log($"CMan, could not find sdr_warproom component: '{cleanAssetName}'", LogLevel.Debug);
        //                        }
        //                        break;
        //                    case "Expansion":
        //                        if (ExternalReferences.ContainsKey(cleanAssetName))
        //                        {
        //                            e.LoadFrom(() => { return ExternalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
        //                        }
        //                        else
        //                        {
        //                            logger.Log($"CMan, could not find Expansion component: '{cleanAssetName}'", LogLevel.Debug);
        //                        }
        //                        break;
        //                    case "Maps":
        //                        string mapName = Path.GetFileNameWithoutExtension(urlParts[2]);
        //                        if (ContentPacks.ExpansionMaps.ContainsKey(mapName))
        //                        {
        //                            e.LoadFrom(() => { return ContentPacks.ExpansionMaps[mapName]; }, AssetLoadPriority.Medium);
        //                        }
        //                        break;
        //                    case "images":
        //                        string imagePath = Path.Combine(helper.DirectoryPath, "data", "assets", "images", urlParts[2]);
        //                        if (File.Exists(imagePath))
        //                        {
        //                            e.LoadFrom(() => { return new StardewBitmap(imagePath).Texture(); }, AssetLoadPriority.Medium);
        //                        }
        //                        break;
        //                    case "bigcraftables":
        //                        if (ExternalReferences.ContainsKey(sdr_item))
        //                        {
        //                            e.LoadFrom(() => { return new StardewBitmap(ExternalReferences[sdr_item].ToString()).Texture(); }, AssetLoadPriority.Medium);
        //                        }
        //                        break;
        //                    case "Strings":
        //                        localizationStrings.HandleEdit(e);
        //                        break;
        //                    case "movies":
        //                        if (ExternalReferences.ContainsKey(sdr_item))
        //                        {
        //                            logger.Log($"Getting movie item {ExternalReferences[sdr_item]}", LogLevel.Debug);
        //                            e.LoadFrom(() => { return new StardewBitmap(ExternalReferences[sdr_item].ToString()).Texture(); }, AssetLoadPriority.Medium);
        //                        }
        //                        else
        //                        {
        //                            logger.Log($"CMan, could not find movies component: '{sdr_item}'", LogLevel.Debug);
        //                        }
        //                        break;
        //                }
        //            }
        //            break;
        //        default:
        //            //CheckForLooseFiles(e, cleanAssetName);
        //            break;
        //    }
        //}

    }
}
