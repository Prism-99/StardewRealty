using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;


namespace SDV_Realty_Core.Framework.CustomEntities.BigCraftables
{
    /// <summary>
    /// Manages details of CustomBigCraftables
    /// </summary>
    internal  class CustomBigCraftableManager
    {
        private  ILoggerService logger;
        private IModHelperService modHelperService;
        public  Dictionary<string, CustomBigCraftableData> BigCraftables = new Dictionary<string, CustomBigCraftableData>();
        //private  SDRContentManager conMan;
        public Dictionary<string, object> ExternalReferences = new Dictionary<string, object> { };

        /// <summary>
        /// Entry point to initialize the manager
        /// </summary>
        /// <param name="ologger">Error logger</param>
        /// <param name="modBaseDirectory">Top directory of the SDR mod</param>
        public CustomBigCraftableManager(ILoggerService ologger, IModHelperService helper)
        {
            logger = ologger;
            modHelperService = helper;
            //conMan = contentMan;
            //LoadDefinitions(helper.DirectoryPath, helper.Translation);
        }
        /// <summary>
        /// Loads any CustomBigCraftable definition files bundled with the game
        /// </summary>
        /// <param name="modBaseDirectory">Top directory of the SDR mod</param>
        public void LoadDefinitions( )
        {
            try
            {
                // find and load any definition files
                string bigCraftableRootPath = Path.Combine(modHelperService.modHelper.DirectoryPath, "data", "assets", "bigcraftables");
                if (Directory.Exists(bigCraftableRootPath))
                {
                    string[] bigCraftableDefinitions = Directory.GetFiles(bigCraftableRootPath, "bigcraftable.json", SearchOption.AllDirectories);
                    logger.Log($"Found {bigCraftableDefinitions.Length} custom bigcraftable definitions.", LogLevel.Debug);


                    // parse definitions
                    //
                    foreach (string definition in bigCraftableDefinitions)
                    {
                        try
                        {
                            //  check for disabled directories
                            if (definition.Split(Path.DirectorySeparatorChar).Where(p => p.StartsWith('.')).Any())
                            {
                                logger.Log($"Directory contains '.', skipping bigcraftable {Path.GetFileNameWithoutExtension(definition)}", LogLevel.Info);
                            }
                            else
                            {
                                string fileContent = File.ReadAllText(definition);
                                CustomBigCraftableData customMachine = JsonConvert.DeserializeObject<CustomBigCraftableData>(fileContent);
                                customMachine.ModPath = Path.GetDirectoryName(definition);
                                customMachine.translations = modHelperService.modHelper.Translation;
                                AddBigCraftable(customMachine);
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    logger.Log($"Missing custom BigCraftable directory {bigCraftableRootPath}", LogLevel.Warn);
                }
            }
            catch(Exception ex) { }
        }
        /// <summary>
        /// Load an external CustomBigCraftable content back
        /// </summary>
        /// <param name="contentPack">Content pack to be added</param>
        /// <returns></returns>
        public  CustomBigCraftableData ReadContentPack(IContentPack contentPack)
        {
            CustomBigCraftableData content = contentPack.ReadJsonFile<CustomBigCraftableData>("bigcraftable.json");

            content.Owner = contentPack;
            content.ModPath = contentPack.DirectoryPath;

            return content;
        }
        /// <summary>
        /// Add a CustomBigCraftable to the manager
        /// </summary>
        /// <param name="customBigCraftableData">CustomBigCraftable object to be added.</param>
        public  void AddBigCraftable(CustomBigCraftableData customBigCraftableData)
        {
            try
            {
                customBigCraftableData.BigCraftableData.Texture = $"SDR{FEConstants.AssetDelimiter}bigcraftables{FEConstants.AssetDelimiter}{customBigCraftableData.Id}{FEConstants.AssetDelimiter}{customBigCraftableData.BigCraftableData.Texture}";
                // add bigcraftable Texture to items served
                ExternalReferences.Add(customBigCraftableData.BigCraftableData.Texture, Path.Combine(customBigCraftableData.ModPath, Path.GetFileName(customBigCraftableData.BigCraftableData.Texture)));
                BigCraftables.Add(customBigCraftableData.Id, customBigCraftableData);
                logger.Log($"Add custom BigCraftable {customBigCraftableData.Id}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                logger.Log($"Error loading {customBigCraftableData.Id}", LogLevel.Error);
                logger.Log($"Error: {ex}", LogLevel.Error);
            }
        }

    }
}
