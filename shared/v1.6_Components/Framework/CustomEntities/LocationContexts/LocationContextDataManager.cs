using Newtonsoft.Json;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using System;
using System.Collections.Generic;
using System.IO;

namespace SDV_Realty_Core.Framework.CustomEntities.LocationContexts
{
    /// <summary>
    /// Manages LocationContextDataa
    /// </summary>
    internal class LocationContextDataManager
    {
        private ILoggerService logger;
        public Dictionary<string, CustomLocationContext> LocationContexts = new();
        private IModHelperService _modHelperService;
        private IUtilitiesService _utilitiesService;
        internal LocationContextDataManager(ILoggerService olog, IModHelperService helper, IUtilitiesService utilitiesService)
        {
            logger = olog;
            _modHelperService = helper;
            _utilitiesService = utilitiesService;
        }
        public void LoadDefinitions()
        {
            LoadLocationContexts();
            _utilitiesService.InvalidateCache("Data/LocationContexts");
        }
        private void LoadLocationContexts()
        {
            //
            //  add custom weather contexts
            //
            string customContextRoot = Path.Combine(_modHelperService.DirectoryPath, "data", "assets", "locationcontexts");
            try
            {
                if (Directory.Exists(customContextRoot))
                {
                    string[] arDefinitions = Directory.GetFiles(customContextRoot, "locationcontext.json", SearchOption.AllDirectories);
                    logger.Log($"Found {arDefinitions.Length} custom location context definitions.", LogLevel.Debug);

                    foreach (string defin in arDefinitions)
                    {
                        try
                        {
                            string fileContent = File.ReadAllText(defin);
                            CustomLocationContext nObject = JsonConvert.DeserializeObject<CustomLocationContext>(fileContent);
                            nObject.ModPath = Path.GetDirectoryName(defin);
                            nObject.translations = _modHelperService.Translation;
                            AddLocationContext(nObject.ContextName, nObject);
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error loading location context defintion: {defin}", LogLevel.Error);
                            logger.Log($"Error: {ex}", LogLevel.Error);
                        }
                    }
                }
                else
                {
                    logger.Log($"Missing custom LocationContext directory '{customContextRoot}'", LogLevel.Warn);
                }
            }
            catch (Exception ex1)
            {
                logger.Log($"Error load location context definitions: {ex1}", LogLevel.Error);
            }

        }

        //private void LoadLocationContexts()
        //{


        //    //LocationContexts.Add("prism99.advize.stardewrealty.always_summer",
        //    //    new LocationContextData
        //    //    {
        //    //        SeasonOverride = StardewValley.Season.Summer,
        //    //        WeatherConditions = new List<WeatherCondition>
        //    //        {
        //    //            new WeatherCondition { Weather="Sun"}
        //    //        }
        //    //    });
        //}
        public void AddLocationContext(string contextId, CustomLocationContext customLocationContext)
        {
            LocationContexts.Add(contextId, customLocationContext);
        }
    }
}
