using Newtonsoft.Json;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewModHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace SDV_Realty_Core.Framework.CustomEntities.Objects
{
    internal class CustomObjectManager
    {
        private  IModHelper helper;
        private  ILoggerService logger;
        public Dictionary<string, object> ExternalReferences = new Dictionary<string, object> { };

        public readonly Dictionary<string, CustomObjectData> objects = new Dictionary<string, CustomObjectData>();
        public CustomObjectManager(ILoggerService ologger, IModHelperService ohelper)
        {
            helper = ohelper.modHelper;
            logger = ologger;
         }
        /// <summary>
        /// Load objects bundled with SDR
        /// </summary>
        public  void LoadObjectDefinitions()
        {
            try
            {
                string customObjectRoot = Path.Combine(helper.DirectoryPath, "data", "assets", "customobjects");
                if (Directory.Exists(customObjectRoot))
                {
                    string[] arDefinitions = Directory.GetFiles(customObjectRoot, "object.json", SearchOption.AllDirectories);
                    logger.Log($"Found {arDefinitions.Length} custom object definitions.", LogLevel.Debug);

                    foreach (string defin in arDefinitions)
                    {
                        try
                        {
                            string fileContent = File.ReadAllText(defin);
                            CustomObjectData nObject = JsonConvert.DeserializeObject<CustomObjectData>(fileContent);
                            nObject.ModPath = Path.GetDirectoryName(defin);
                            nObject.translations = helper.Translation;
                            //nBuilding.LoadExternalReferences();
                            AddObjectDefinition(nObject);
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error loading object defintion: {ex}", LogLevel.Error);
                        }
                    }
                }
                else
                {
                    logger.Log($"Missing custom object directory '{customObjectRoot}'", LogLevel.Warn);
                }
            }
            catch(Exception ex)
            {

            }
        }
        public  void AddObjectDefinition(CustomObjectData nObject)
        {
            //
            //  add external reference to object texture
            //
            try
            {
                ExternalReferences.Add($"SDR{FEConstants.AssetDelimiter}Objects{FEConstants.AssetDelimiter}{nObject.ObjectId}", new StardewBitmap(Path.Combine(nObject.ModPath, nObject.ObjectData.Texture)).Texture());
                // massage texture value to include pathing
                nObject.ObjectData.Texture = $"SDR{FEConstants.AssetDelimiter}Objects{FEConstants.AssetDelimiter}{nObject.ObjectId}";

                objects.Add(nObject.ObjectId, nObject);
                logger.Log($"Added custom object: '{nObject.ObjectId}'", LogLevel.Info);
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading texture for custom object {nObject.ObjectId}", LogLevel.Error);
                logger.Log($"Error: {ex}", LogLevel.Error);
            }

          }
    }
}
