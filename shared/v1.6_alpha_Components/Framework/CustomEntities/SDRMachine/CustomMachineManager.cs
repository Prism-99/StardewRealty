using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;

namespace SDV_Realty_Core.Framework.CustomEntities.Machines
{
    public class CustomMachineManager
    {
        /// <summary>
        /// Manager custom machines definitions.
        /// Custom machines contain both BC data and MachineData details
        /// </summary>
        private IModHelperService helper;
        private ILoggerService logger;
        private ICustomBigCraftableService bigCraftableService;
        private ICustomMachineDataService customMachineDataService;
        internal CustomMachineManager(ILoggerService ologger, IModHelperService ohelper, ICustomBigCraftableService bigCraftableService,ICustomMachineDataService customMachineDataService)
        {
            helper = ohelper;
            logger = ologger;
            this.bigCraftableService = bigCraftableService;
            this.customMachineDataService = customMachineDataService;
            //LoadDefinitions();
        }

        public void LoadDefinitions()
        {
            try
            {
                string machineRootPath = Path.Combine(helper.DirectoryPath, "data", "assets", "machines");
                if (Directory.Exists(machineRootPath))
                {
                    string[] machineDefinitions = Directory.GetFiles(machineRootPath, "machine.json", SearchOption.AllDirectories);
                    logger.Log($"Found {machineDefinitions.Length} custom machine definitions.", LogLevel.Debug);

                    foreach (string definition in machineDefinitions)
                    {
                        try
                        {
                            if (definition.Split(Path.DirectorySeparatorChar).Where(p => p.StartsWith('.')).Any())
                            {
                                logger.Log($"Directory contains '.', skipping machine {Path.GetFileNameWithoutExtension(definition)}", LogLevel.Info);
                            }
                            else
                            {
                                string fileContent = File.ReadAllText(definition);
                                CustomMachine customMachine = JsonConvert.DeserializeObject<CustomMachine>(fileContent);
                                customMachine.ModPath = Path.GetDirectoryName(definition);
                                customMachine.translations = helper.Translation;
                                AddMachine(customMachine);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error loading machine definition: {definition}", LogLevel.Error);
                            logger.LogError("CustomMachineManager.LoadDefinitions", ex);
                        }
                    }
                }
                else
                {
                    logger.Log($"Missing custom Machine directory '{machineRootPath}'", LogLevel.Warn);
                }
            }
            catch(Exception ex)
            {
                logger.LogError("CustomMachineManager.LoadDefinitions", ex);
            }
        }

        public void AddMachine(CustomMachine machine)
        {
            //
            //  add BigCraftable record
            //
            bigCraftableService.customBigCraftableManager.AddBigCraftable(new CustomBigCraftableData(machine));
            //
            //  add MachineData record
            //
            customMachineDataService.AddMachine(new CustomMachineData(machine));
        }
    }
}