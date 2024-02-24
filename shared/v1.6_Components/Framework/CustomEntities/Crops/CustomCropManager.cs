using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewModHelpers;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;


namespace SDV_Realty_Core.Framework.CustomEntities.Crops
{
    internal class CustomCropManager
    {
        private ILoggerService logger;
         private IModHelperService modHelperService;
        public readonly Dictionary<string, CustomCropData> Crops = new Dictionary<string, CustomCropData>();
        public Dictionary<string, object> ExternalReferences=new();
        public CustomCropManager(ILoggerService ologger, IModHelperService helper)
        {
            logger = ologger;
             modHelperService = helper;
        }
        public void LoadObjectDefinitions()
        {
            string buildingRoot = Path.Combine(modHelperService.modHelper.DirectoryPath, "data", "assets", "crops");
            try
            {
                if (Directory.Exists(buildingRoot))
                {
                    string[] arDefinitions = Directory.GetFiles(buildingRoot, "crops.json", SearchOption.AllDirectories);
                    logger.Log($"Found {arDefinitions.Length} custom crop definitions.", LogLevel.Debug);

                    foreach (string defin in arDefinitions)
                    {
                        try
                        {
                            string fileContent = File.ReadAllText(defin);
                            CustomCropData nObject = JsonConvert.DeserializeObject<CustomCropData>(fileContent);
                            nObject.ModPath = Path.GetDirectoryName(defin);
                            nObject.translations = modHelperService.modHelper.Translation;
                            AddCropDefinition(nObject);
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error loading crop defintion: {defin}", LogLevel.Error);
                            logger.Log($"Error: {ex}", LogLevel.Error);
                        }
                    }
                }
                else
                {
                    logger.Log($"Missing custom Crop directory '{buildingRoot}'", LogLevel.Warn);
                }
            }
            catch (Exception ex1)
            {
                logger.Log($"Error load crop definitions: {ex1}", LogLevel.Error);
            }

        }
        public void AddCropDefinition(CustomCropData nObject)
        { //
          //  add external reference to crop texture
          //
            try
            {
                string finalTexture = $"SDR{FEConstants.AssetDelimiter}Objects{FEConstants.AssetDelimiter}crop.{nObject.CropId}";
                ExternalReferences.Add(finalTexture, new StardewBitmap(Path.Combine(nObject.ModPath, nObject.CropData.Texture)).Texture());
                // massage texture value to include pathing
                nObject.CropData.Texture = finalTexture;
                Crops.Add(nObject.CropId, nObject);
                logger.Log($"Added crop: '{nObject.CropId}'", LogLevel.Info);
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading texture for custom crop {nObject.CropId}", LogLevel.Error);

            }
        }
    }
}
