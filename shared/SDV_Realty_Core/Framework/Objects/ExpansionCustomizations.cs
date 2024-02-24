using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.Objects
{
    internal class ExpansionCustomizations
    {
    
        public  Dictionary<string, CustomizationDetails> CustomDefinitions;
        private static string sFilename;
        private static ILoggerService logger;
        private static IUtilitiesService _utilitiesService;
        public ExpansionCustomizations(ILoggerService olog,IModHelperService modHelperService, IModDataService modDataService, IUtilitiesService utilitiesService)
        {
            CustomDefinitions = modDataService.CustomDefinitions;
            sFilename = Path.Combine(modHelperService.DirectoryPath, "custom_settings.json");
            logger = olog;
            _utilitiesService=utilitiesService;
        }

        public  void Load()
        {

            logger.Log($"Loading customization file '{sFilename}'", LogLevel.Debug);

            if (File.Exists(sFilename))
            {
                try
                {
                    string sContent = File.ReadAllText(sFilename);
                    CustomDefinitions = JsonConvert.DeserializeObject<Dictionary<string, CustomizationDetails>>(sContent);
                    //
                    //  check for upgrades
                    //
                    bool upgrades = false;
                    foreach(CustomizationDetails customizationDetails in CustomDefinitions.Values)
                    {
                        upgrades = upgrades || customizationDetails.Upgrade();
                    }
                    if(upgrades)
                    {
                        logger.Log($"SDR Expansion customizations upgraded to version 2", LogLevel.Info);
                        Save();
                    }
                  
                    logger.Log($"Loaded {CustomDefinitions.Count} expansion customizations", LogLevel.Debug);
                }
                catch (Exception ex)
                {
                    logger.Log("Customization load error: " + ex.ToString(), LogLevel.Error);
                    CustomDefinitions = new Dictionary<string, CustomizationDetails> { };
                }
            }
        }
        public  void SetSeasonalOverride(string expansionName,string seasonalOverride)
        {
            if(!CustomDefinitions.ContainsKey(expansionName))
            {
                CustomDefinitions.Add(expansionName,new CustomizationDetails());
            }
            CustomDefinitions[expansionName].SeasonOverride = seasonalOverride;
            Save();
        }
        public  void Save()
        {
            logger.Log($"Save CustomizationDetailsFile", LogLevel.Debug);
            string sContent = JsonConvert.SerializeObject(CustomDefinitions);
            try
            {
                File.WriteAllText(sFilename, sContent);
                _utilitiesService.InvalidateCache("Data/Locations");
            }
            catch (Exception ex)
            {
                logger.Log($"Error saving customizations: {ex}",LogLevel.Error);
            }
        }

    }
}