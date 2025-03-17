using SDV_Realty_Core.Framework.Locations;
using StardewModdingAPI.Events;
using System.IO;
using System;
using StardewModHelpers;
using SDV_Realty_Core.ContentPackFramework.ContentPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.DataProviders;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;


namespace SDV_Realty_Core.Framework.DataProviders
{
    /// <summary>
    /// Manager class for handling Game Data edits
    /// </summary>
    internal partial class DataProviderManager
    {
        //
        //  1.6 code
        //
        //private LocalizationStrings localizationStrings;
        private IGameDataProviderService gameDataProviderService;
        private IUtilitiesService utilitiesService;
        private IModDataService modDataService;
        internal void Initialize(IModDataService modDataService,ContentPackLoader cPacks, SDRContentManager conManager, IUtilitiesService utilitiesService, ILoggerService olog, IGameDataProviderService gameDataService)
        {
            this.utilitiesService = utilitiesService;
            logger = olog;
            //config = modConfig;
            //this.helper = helper;
            gameDataProviderService = gameDataService;
            contentManager = conManager;
            externalReferences = modDataService.ExternalReferences;
            this.modDataService = modDataService;
            //
            //  add common data providers
            //
            contentPacks = cPacks;
            //stringFromMaps = conManager.stringFromMaps;   

            //chairTiles = new ChairTilesDataProviders(config.AddBridgeSeat, conManager);
            NPCTastes = new NPCGiftTastesDataProvider(conManager);

            AddVersionSpecificProviders();
            //
            //  set game lore flag
            //
            foreach (IGameDataProvider provider in dataProviders)
            {
                provider.UseLore = modDataService.Config.UseLore;
                provider.SetLogger(logger);
            }

            utilitiesService.GameEventsService.AddSubscription(new GameLaunchedEventArgs(), On_GameLaunched);
            utilitiesService.GameEventsService.AddSubscription("AssetRequestedEventArgs", Content_AssetRequested);
            //utilitiesService.ModHelperService.modHelper.Events.Content.
            //helper.Events.GameLoop.GameLaunched += On_GameLaunched;
            //helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        //internal void SetLocalizationString(LocalizationStrings locStrings)
        //{
        //    //localizationStrings = locStrings;
        //}
        internal void AddVersionSpecificProviders()
        {
            // added for Services
            //localizationStrings = new LocalizationStrings();
            //localizationStrings.Intialize(utilitiesService.ModHelperService.Translation);

            dataProviders = gameDataProviderService.dataProviders;
        }
        /// <summary>
        /// Adds required data managers for data providers
        /// </summary>
        internal void SetCustomObjects()
        {
        }
       
        private void Content_AssetRequested(EventArgs ea)
        {
            AssetRequestedEventArgs e = (AssetRequestedEventArgs)ea;
            //
            //  edit handled by standard handlers
            //
            if (HandleDataEdit(e))
                return;

            //
            //  check for speciality handling
            //
            string cleanAssetName = e.NameWithoutLocale.Name;
            switch (cleanAssetName)
            {
                case string map_name when map_name.StartsWith("Maps"):
                    if (externalReferences.ContainsKey(cleanAssetName))
                    {
                        e.LoadFrom(() => { return externalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                    }
                    break;
                case string event_name when event_name.StartsWith("Data/Events"):

                    break;
                case string sdr_item when sdr_item.StartsWith("SDR/"):
                    string[] urlParts = sdr_item.Split('/');
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
                                    logger.Log($"DataProviderManager, could not find Building component: '{cleanAssetName}'", LogLevel.Debug);
                                }
                                break;
                            case "Objects":
                                if (externalReferences.ContainsKey(cleanAssetName))
                                {
                                    e.LoadFrom(() => { return externalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                                }
                                else
                                {
                                    logger.Log($"DataProviderManager, could not find Objects component: '{cleanAssetName}'", LogLevel.Debug);
                                }
                                break;
                            case WarproomManager.StardewMeadowsLoacationName:
                                if (externalReferences.ContainsKey(cleanAssetName))
                                {
                                    e.LoadFrom(() => { return externalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                                }
                                else
                                {
                                    logger.Log($"DataProviderManager, could not find sdr_warproom component: '{cleanAssetName}'", LogLevel.Debug);
                                }
                                break;
                            case "Expansion":
                                if (externalReferences.ContainsKey(cleanAssetName))
                                {
                                    e.LoadFrom(() => { return externalReferences[cleanAssetName]; }, AssetLoadPriority.Medium);
                                }
                                else
                                {
                                    logger.Log($"DataProviderManager, could not find Expansion component: '{cleanAssetName}'", LogLevel.Debug);
                                }
                                break;
                            case "Maps":
                                string mapName = Path.GetFileNameWithoutExtension(urlParts[2]);
                                if (modDataService.ExpansionMaps.ContainsKey(mapName))
                                {
                                    e.LoadFrom(() => { return modDataService.ExpansionMaps[mapName]; }, AssetLoadPriority.Medium);
                                }
                                break;
                            case "images":
                                string imagePath = Path.Combine(utilitiesService.GameEnvironment.ModPath, "data", "assets", "images", urlParts[2]);
                                if (File.Exists(imagePath))
                                {
                                    e.LoadFrom(() => { return new StardewBitmap(imagePath).Texture(); }, AssetLoadPriority.Medium);
                                }
                                break;
                            case "bigcraftables":
                                if (externalReferences.ContainsKey(sdr_item))
                                {
                                    e.LoadFrom(() => { return new StardewBitmap(externalReferences[sdr_item].ToString()).Texture(); }, AssetLoadPriority.Medium);
                                }
                                break;
                            case "Strings":
                                //localizationStrings.HandleEdit(e);
                                int xx = 1;
                                break;
                            case "movies":
                                if (externalReferences.ContainsKey(sdr_item))
                                {
                                    logger.Log($"Getting movie item {externalReferences[sdr_item]}", LogLevel.Debug);
                                    e.LoadFrom(() => { return new StardewBitmap(externalReferences[sdr_item].ToString()).Texture(); }, AssetLoadPriority.Medium);
                                }
                                else
                                {
                                    logger.Log($"DataProviderManager, could not find movies component: '{sdr_item}'", LogLevel.Debug);
                                }
                                break;
                            default:
                                logger.Log($"Unknown asset requested: {sdr_item}", LogLevel.Warn);
                                break;
                        }
                    }
                    break;
                default:
                    //CheckForLooseFiles(e, cleanAssetName);
                    break;
            }
        }

    }
}
