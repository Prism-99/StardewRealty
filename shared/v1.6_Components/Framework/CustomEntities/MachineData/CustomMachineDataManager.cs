using Newtonsoft.Json;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;

namespace SDV_Realty_Core.Framework.CustomEntities.Machines
{
    public  class CustomMachineDataManager
    {
        /// <summary>
        /// Handles Custom Game MachineData items
        /// </summary>
        private static IModHelperService helper;
        private static ILoggerService logger;
        private static List<CustomMachineData> additonalRecipes = new List<CustomMachineData>();
        public  Dictionary<string, CustomMachineData> Machines = new Dictionary<string, CustomMachineData>();
        private static IContentManagerService conMan;
        internal CustomMachineDataManager(ILoggerService ologger, IModHelperService ohelper, IContentManagerService contentMan)
        {
            helper = ohelper;
            logger = ologger;
            conMan= contentMan;
            //LoadDefinitions();
        }

        public  void LoadDefinitions()
        {
            try
            {
                string machineRootPath = Path.Combine(helper.DirectoryPath, "data", "assets", "machinedata");
                if (Directory.Exists(machineRootPath))
                {
                    string[] machineDefinitions = Directory.GetFiles(machineRootPath, "machinedata.json", SearchOption.AllDirectories);
                    logger.Log($"Found {machineDefinitions.Length} custom machinedata definitions.", LogLevel.Debug);

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
                                CustomMachineData customMachineData = JsonConvert.DeserializeObject<CustomMachineData>(fileContent);
                                customMachineData.ModPath = Path.GetDirectoryName(definition);
                                AddMachine(customMachineData);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Error loading machinedata definition: {definition}", LogLevel.Error);
                            logger.LogError("CustomMachineDataManager.LoadDefinitions", ex);
                        }
                    }
                }
                else
                {
                    logger.Log($"Missing custom MachineData directory '{machineRootPath}'", LogLevel.Warn);
                }
            }
            catch(Exception ex) { }
        }
        /// <summary>
        /// Add additional recipes to a machine
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="recipes"></param>
        private  void AddRecipes(CustomMachineData machine, CustomMachineData recipes)
        {
            foreach (var recipe in recipes.MachineData.OutputRules)
            {
                if (!machine.MachineData.OutputRules.Where(p => p.Id == recipe.Id).Any())
                {
                    machine.MachineData.OutputRules.Add(recipe);
                    logger.Log($"Added additional recipe {recipe.Id} to machine {machine.MachineId}", LogLevel.Debug);
                }
                else
                {
                    // duplicate recipe id
                    logger.Log($"Duplicate recipe id {recipe.Id} for machine {machine.MachineId}", LogLevel.Warn);
                }
            }
        }
        public  void AddMachine(CustomMachineData machine)
        {
            if (machine.IsMachineDefinition)
            {
                logger.Log($"Loading custom machinedata {machine.MachineId}", LogLevel.Debug);
                //
                //  main definition for the machine, not additional recipes
                if (machine.MachineData.WorkingEffects != null)
                {
                    foreach (var effect in machine.MachineData.WorkingEffects)
                    {
                        //
                        //  added any animated sprites
                        //
                        if (effect.TemporarySprites != null)
                        {
                            foreach (var sprite in effect.TemporarySprites)
                            {
                                if (!string.IsNullOrEmpty(sprite.Texture))
                                {
                                    string spriteFullname = $"SDR{FEConstants.AssetDelimiter}bigcraftables{FEConstants.AssetDelimiter}{machine.MachineId}{FEConstants.AssetDelimiter}sprites{FEConstants.AssetDelimiter}{sprite.Id}{FEConstants.AssetDelimiter}{sprite.Texture}";
                                    conMan.ExternalReferences.Add(spriteFullname, Path.Combine(machine.ModPath, sprite.Texture));
                                    logger.Log($"Loaded TemporarySprite '{sprite}' for machine {machine.MachineId}", LogLevel.Trace);
                                    sprite.Texture = spriteFullname;
                                }
                            }
                        }
                        //
                        //  check for sound
                        //
                        //
                        // 2024-01-15
                        //  sounds moved to unified Sounds field
                        //
                        //if (!string.IsNullOrEmpty(effect.Sound))
                        //{
                        //    string filename = Path.Combine(machine.ModPath, effect.Sound);
                        //    if (File.Exists(filename))
                        //    {
                        //        effect.Sound = $"SDR{FEConstants.AssetDelimiter}Audio{FEConstants.AssetDelimiter}{machine.MachineId}{FEConstants.AssetDelimiter}{Path.GetFileName(filename)}";
                        //        AudioCueManager.AddCue(effect.Sound, new StardewValley.GameData.AudioCueData
                        //        {
                        //            Id = effect.Sound,
                        //            FilePaths = new List<string> { filename },
                        //            Looped = false,
                        //            Category = "Ambient"
                        //        });
                        //    }
                        //    else
                        //    {
                        //        logger.Log($"machine {machine.MachineId} is missing sound {effect.Sound}", LogLevel.Error);
                        //    }
                        //}
                    }
                }
                Machines.Add(machine.MachineId, machine);
                //  check for recipes waiting
                var waitingRecipes = additonalRecipes.Where(p => p.MachineId ==machine.MachineId).ToList();
                if (waitingRecipes.Any())
                {
                    logger.Log($"Adding queued recipes to {machine.MachineId}", LogLevel.Debug);
                    foreach (var recipe in waitingRecipes)
                    {
                        AddRecipes(machine, recipe);
                    }
                    additonalRecipes.RemoveAll(p=>p.MachineId == machine.MachineId);
                }
                logger.Log($"Custom Machinedata for {machine.MachineId} added", LogLevel.Info);
            }
            else
            {
                logger.Log($"Adding additional recipes to machine {machine.MachineId}", LogLevel.Debug);
                //  additional recipes for the machine
                if (Machines.TryGetValue(machine.MachineId, out var mainMachine))
                {
                    AddRecipes(mainMachine, machine);
                }
                else
                {
                    logger.Log($"Machine {machine.MachineId} not added, recipes added to queue.", LogLevel.Debug);
                    // machine not added yet, hold recipes
                    additonalRecipes.Add(machine);
                }
            }
        }
    }
}