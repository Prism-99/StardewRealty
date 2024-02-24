using SDV_Realty_Core.Framework.CustomEntities.BigCraftables;
using System.Collections.Generic;
using SDV_Realty_Core.Framework.CustomEntities.Buildings;
using System.Linq;
using StardewValley.TerrainFeatures;
using Prism99_Core.Utilities;
using StardewValley.Objects;
using Prism99_Core.PatchingFramework;
using System;
using StardewValley.Delegates;
using SDV_Realty_Core.Framework.Utilities;
using StardewValley.GameData.Machines;
using StardewValley.Inventories;
using HarmonyLib;
using StardewValley.Internal;
using System.Data;
using StardewValley.Extensions;
using StardewValley.GameData;
using SDV_Realty_Core.Framework.ServiceInterfaces.CustomEntities;
using StardewValley;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceProviders;

namespace SDV_Realty_Core.Framework.CustomEntities
{
    internal class CustomProductionManager
    {
        private static SDVLogger logger;
        private static string key_BuildingOutputCount = "sdr.BuildingOutput.Count";
        private static string key_BuildingOutputItem = "sdr.BuildingOutput.Item";
        private static string key_BuildingOutputQuantity = "sdr.BuildingOutput.Quantity";
        private static string key_BuildingOutputTime = "sdr.BuildingOutput.Time";
        private static string key_Crop = "sdr.Crop";
        private static ICustomEntitiesServices customEntitiesServices;
        public void Initialize(SDVLogger ologger, GamePatches Patches,ICustomEntitiesServices entitiesServices)
        {
            customEntitiesServices= entitiesServices;
            logger = ologger;
            //
            //  gamelocation patch to increase building production
            //
            Patches.AddPatch(false, typeof(GameLocation), "passTimeForObjects",
                new Type[] { typeof(int) }, typeof(CustomProductionManager), nameof(passTimeForObjects),
                "Modify machine production.",
                "Location");
            Patches.AddPatch(false, typeof(GameLocation), "DayUpdate",
                new Type[] { typeof(int) }, typeof(CustomProductionManager), nameof(DayUpdate),
                "Modify machine production.",
                "Location");
            Patches.AddPatch(true, typeof(SDObject), "PlaceInMachine",
                null, typeof(CustomProductionManager), nameof(PlaceInMachine),
                "Modify player adding items to machines.",
                "MachineData");
            Patches.AddPatch(true, typeof(SDObject), "AttemptAutoLoad",
                new Type[] { typeof(IInventory), typeof(Farmer) }, typeof(CustomProductionManager), nameof(AttemptAutoLoad),
                "Modify auto adding items to machines.",
                "MachineData");
            //
            //  add building weather condition
            //
            GameStateQuery.Register("prism99.advize.stardewrealty.buildingweather", HandleParentWeatherQuery);
        }
        public static bool HandleParentWeatherQuery(string[] query, GameStateQueryContext context)
        {
            if (query.Length < 3)
            {
                GameStateQuery.Helpers.ErrorResult(query, "Not enough parameters");
                return false;
            }
            GameLocation testLocation = null;
            if (query[1].ToLower() == "here")
            {
                testLocation = context.Location;
            }
            else if (Game1.getLocationFromName(query[1]) != null)
            {
                testLocation = Game1.getLocationFromName(query[1]);
            }
            if (testLocation != null)
            {
                foreach (string weather in query.Skip(2))
                {
                    if (testLocation.GetWeather().Weather == weather)
                        return true;
                }

            }
            return false;
        }
        /// <summary>
        /// Check to see if any rules are met
        /// </summary>
        /// <param name="selectedItem">Item in focus</param>
        /// <param name="items">Inventory to be searched</param>
        /// <param name="machine">Machine being used</param>
        /// <param name="outputRule">Selected MachineOutputRule</param>
        /// <param name="triggerRules">Selected MachineOutputTriggerRules</param>
        /// <returns>True if any rule it met</returns>
        private static bool FindAnyRules(Item selectedItem, IInventory items, SDObject machine, out MachineOutputRule outputRule, out List<MachineOutputTriggerRule> triggerRules)
        {
            outputRule = null;
            triggerRules = null;

            MachineData machineData = machine.GetMachineData();

            if (machineData != null)
            {
                if (machineData.AdditionalConsumedItems != null)
                {
                    // check for AdditionalItems
                    foreach (var fuel in machineData.AdditionalConsumedItems)
                    {
                        var haveFuel = items.Where(p => (p?.QualifiedItemId ?? "") == fuel.ItemId && p.Stack >= fuel.RequiredCount).FirstOrDefault();
                        if (haveFuel == null)
                            return false;
                    }
                }
                foreach (var rule in machineData.OutputRules)
                {
                    if (rule.Triggers?.Count() > 0)
                    {
                        //
                        //  check for selected item, if any
                        //
                        if (selectedItem != null)
                        {
                            var haveItem = rule.Triggers.Where(p => (p?.RequiredItemId ?? "") == selectedItem.QualifiedItemId && selectedItem.Stack >= p.RequiredCount).FirstOrDefault();
                            if (haveItem == null)
                                continue;
                            //
                            //  check all requirements
                            if (IsRuleMet(items, rule))
                            {
                                outputRule = rule;
                                triggerRules = rule.Triggers;
                                return true;
                            }
                            return false;
                        }
                        foreach (var trigger in rule.Triggers)
                        {
                            if (IsRuleMet(items, rule))
                            {
                                outputRule = rule;
                                triggerRules = rule.Triggers;
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check it the supplied inventory meets the
        /// requirements of the supplied rule
        /// </summary>
        /// <param name="items">Inventory to be checked.</param>
        /// <param name="rule">Rule to be verified.</param>
        /// <returns>True if the rule is met</returns>
        private static bool IsRuleMet(IInventory items, MachineOutputRule rule)
        {
            bool isValid = true;
            foreach (var trigger in rule.Triggers)
            {
                if (!string.IsNullOrEmpty(trigger.Condition))
                {
                    if (!GameStateQuery.CheckConditions(trigger.Condition))
                    {
                        isValid = false;
                        logger.Log($"Trigger rule failed, condition '{trigger.Condition}' not met.", LogLevel.Debug);
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(trigger.RequiredItemId))
                {
                    var haveItem = items.Where(p => (p?.QualifiedItemId ?? "") == trigger.RequiredItemId && p.Stack >= trigger.RequiredCount).FirstOrDefault();
                    if (haveItem == null)
                    {
                        isValid = false;
                        break;
                    }
                }
                else if (trigger.RequiredTags != null)
                {
                    // check for tags
                    var haveTag = items.Where(p => p.GetContextTags().Intersect(trigger.RequiredTags).Any() && p.Stack >= trigger.RequiredCount).FirstOrDefault();
                    if (haveTag == null)
                    {
                        isValid = false;
                        break;
                    }
                }
                else
                {
                    //  bad rule should not happen
                    isValid = false;
                    break;
                }

            }
            return isValid;
        }

        /// <summary>
        /// Checks to see if the machine can be loaded.
        /// The logic is changed so that multiple triggers in a rule
        /// are treated as ANDs instead of ORs as the vanilla code does
        /// </summary>
        /// <param name="inventory">Inventory to be searched</param>
        /// <param name="inputItem">Item in focues, if any</param>
        /// <param name="who">Farmer attempting the load</param>
        /// <param name="machine">Machine being loaded</param>
        /// <param name="probe">Flag to indicate if this is a probe call</param>
        /// <param name="playSounds">Flag for sound playing if new cycle is started</param>
        /// <returns>True if the machine was loaded</returns>
        private static bool AttemptLoading(IInventory inventory, Item inputItem, Farmer who, SDObject machine, bool probe, bool playSounds = true)
        {
            MachineData machinedata = machine.GetMachineData();
            //  check if machine is already in production
            if (machine.heldObject.Value != null)
            {
                if (!machinedata.AllowLoadWhenFull)
                {
                    return false;
                }
                if (inputItem != null && inputItem.QualifiedItemId == machine.lastInputItem.Value?.QualifiedItemId)
                {
                    return false;
                }
            }
            if (FindAnyRules(inputItem, inventory, machine, out var outputRule, out var triggerRule))
            {
                if (probe)
                    return true;

                //
                //  it is assumed that the first ingredient listed is the primary ingredient
                //  this will be used for flavoured items
                //
                Item primaryItem = null;
                foreach (var item in triggerRule)
                {
                    if (!string.IsNullOrEmpty(item.RequiredItemId))
                    {
                        primaryItem ??= ItemRegistry.Create(item.RequiredItemId, 1);
                        (SDObject.autoLoadFrom ?? who.Items).ReduceId(item.RequiredItemId, item.RequiredCount);
                    }
                    else if (item.RequiredTags != null)
                    {
                        var taggedItem = (SDObject.autoLoadFrom ?? who.Items).Where(p => p.GetContextTags().Intersect(item.RequiredTags).Any() && p.Stack >= item.RequiredCount).FirstOrDefault();
                        if (taggedItem != null)
                        {
                            primaryItem ??= ItemRegistry.Create(taggedItem.ItemId, 1);
                            (SDObject.autoLoadFrom ?? who.Items).ReduceId(taggedItem.ItemId, item.RequiredCount);
                        }
                    }
                }
                // remove fuel
                MachineData machineData = machine.GetMachineData();
                if (machineData.AdditionalConsumedItems != null)
                {
                    foreach (var fuel in machineData.AdditionalConsumedItems)
                    {
                        (SDObject.autoLoadFrom ?? who.Items).ReduceId(fuel.ItemId, fuel.RequiredCount);
                    }
                }
                SDObject.autoLoadFrom?.RemoveEmptySlots();
                Item newHeldItem = null;
                //
                // get machine output
                //
                MachineItemOutput selectedOutput = null;

                if (outputRule.OutputItem.Count == 1)
                {
                    if (!string.IsNullOrEmpty(outputRule.OutputItem[0].Condition))
                    {
                        if (GameStateQuery.CheckConditions(outputRule.OutputItem[0].Condition))
                            selectedOutput = outputRule.OutputItem[0];
                    }
                    else
                    {
                        selectedOutput = outputRule.OutputItem[0];
                    }
                }
                else
                {
                    List<MachineItemOutput> potentailRules = new List<MachineItemOutput>();
                    foreach (var outRule in outputRule.OutputItem)
                    {
                        if (string.IsNullOrEmpty(outRule.Condition))
                        {
                            potentailRules.Add(outRule);
                        }
                        else if (GameStateQuery.CheckConditions(outRule.Condition))
                        {
                            potentailRules.Add(outRule);
                        }
                    }
                    selectedOutput = potentailRules.FirstOrDefault();
                }

                int? overrideMinutesUntilReady = null;
                if (selectedOutput != null)
                {
                    if (!string.IsNullOrEmpty(selectedOutput.ItemId))
                        newHeldItem = ItemRegistry.Create(selectedOutput.ItemId, selectedOutput.MinStack);
                    else if (!string.IsNullOrEmpty(selectedOutput.OutputMethod))
                    {
                        if (!StaticDelegateBuilder.TryCreateDelegate<MachineOutputDelegate>(selectedOutput.OutputMethod, out var method, out var error2))
                        {
                            //IGameLogger log = Game1.log;
                            //DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 3);
                            //defaultInterpolatedStringHandler.AppendLiteral("Machine ");
                            //defaultInterpolatedStringHandler.AppendFormatted(machine.QualifiedItemId);
                            //defaultInterpolatedStringHandler.AppendLiteral(" has invalid item output method '");
                            //defaultInterpolatedStringHandler.AppendFormatted(outputData.OutputMethod);
                            //defaultInterpolatedStringHandler.AppendLiteral("': ");
                            //defaultInterpolatedStringHandler.AppendFormatted(error2);
                            //log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
                            //return null;
                        }
                        newHeldItem = method(machine, primaryItem, false, selectedOutput, out overrideMinutesUntilReady);
                    }
                }
                if (newHeldItem != null)
                {
                    ItemQueryContext context = new ItemQueryContext(machine.Location, who, Game1.random);
                    newHeldItem = (Item)ItemQueryResolver.ApplyItemFields(newHeldItem, selectedOutput, context, inputItem);

                    if (selectedOutput.CopyColor)
                    {
                        ColoredObject droppedInColoredObj = inputItem as ColoredObject;
                        if (droppedInColoredObj != null)
                        {
                            Color color = droppedInColoredObj.color.Value;
                            ColoredObject newColoredObj = newHeldItem as ColoredObject;
                            if (newColoredObj != null)
                            {
                                newColoredObj.color.Value = color;
                            }
                            else if (newHeldItem.HasTypeObject())
                            {
                                newHeldItem = new ColoredObject(newHeldItem.ItemId, 1, color);
                                newHeldItem = (Item)ItemQueryResolver.ApplyItemFields(newHeldItem, selectedOutput, context, inputItem);
                            }
                        }
                    }
                    if (selectedOutput.CopyQuality)
                    {
                        newHeldItem.Quality = inputItem.Quality;
                    }
                    SDObject heldObj = newHeldItem as SDObject;
                    if (selectedOutput.CopyPrice)
                    {
                        SDObject inputObj = inputItem as SDObject;
                        heldObj.Price = inputObj.Price;
                    }
                    List<QuantityModifier> priceModifiers = selectedOutput.PriceModifiers;
                    if (priceModifiers != null && priceModifiers.Count > 0)
                    {
                        heldObj.Price = (int)Utility.ApplyQuantityModifiers(heldObj.Price, selectedOutput.PriceModifiers, selectedOutput.PriceModifierMode, machine.Location, who, newHeldItem, inputItem);
                    }
                    if (overrideMinutesUntilReady.HasValue)
                        machine.MinutesUntilReady = overrideMinutesUntilReady.Value;
                    else
                    {
                        machine.MinutesUntilReady = outputRule.MinutesUntilReady;
                    }
                    //
                    //  need to check modifier rules for quantity and quality
                    //
                    newHeldItem.FixQuality();
                    newHeldItem.FixStackSize();
                    machine.heldObject.Value = (SDObject)newHeldItem;

                    if (machineData.LoadEffects != null)
                    {
                        foreach (MachineEffects effect in machineData.LoadEffects)
                        {
                            if (machine.PlayMachineEffect(effect, playSounds))
                            {
                                Traverse.Create(machine).Field("_machineAnimation").SetValue(effect);
                                Traverse.Create(machine).Field("_machineAnimationLoop").SetValue(false);
                                Traverse.Create(machine).Field("_machineAnimationIndex").SetValue(0);
                                Traverse.Create(machine).Field("_machineAnimationFrame").SetValue(-1);
                                Traverse.Create(machine).Field("_machineAnimationInterval").SetValue(0);
                                break;
                            }
                        }
                    }

                    MachineDataUtility.UpdateStats(machineData.StatsToIncrementWhenLoaded, inputItem, 1);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// SDObject override for automated inputs
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="who"></param>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        /// 
        public static bool AttemptAutoLoad(IInventory inventory, Farmer who, SDObject __instance, ref bool __result)
        {
            if (!customEntitiesServices.customBigCraftableService.customBigCraftableManager.BigCraftables.ContainsKey(__instance.Name))
                return true;

            SDObject.autoLoadFrom = inventory;
            __result = false;

            if (AttemptLoading(inventory, null, who, __instance, false))
            {
                //
                //  machine has been freshly loaded
                //
                __result = true;
            }

            SDObject.autoLoadFrom = null;

            return false;
        }

        /// <summary>
        /// SDObject override for player inputs
        /// </summary>
        /// <param name="machineData"></param>
        /// <param name="inputItem"></param>
        /// <param name="probe"></param>
        /// <param name="who"></param>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <param name="showMessages"></param>
        /// <param name="playSounds"></param>
        /// <returns>True if machine was loaded</returns>
        public static bool PlaceInMachine(MachineData machineData, Item inputItem, bool probe, Farmer who, SDObject __instance, ref bool __result, bool showMessages = true, bool playSounds = true)
        {
            //
            //  check for custom machine
            //
            if (!customEntitiesServices.customBigCraftableService.customBigCraftableManager.BigCraftables.ContainsKey(__instance.Name))
                return true;

            if (AttemptLoading(who.Items, inputItem, who, __instance, probe, playSounds))
            {
                //
                //  machine has been freshly loaded
                //
                __result = true;
            }

            return false;
        }

        /// <summary>
        /// Apply daily bonuses that are defined
        /// -water locations
        /// -Custom Building output
        /// -Crop speed ups
        /// </summary>
        /// <param name="__instance">GameLocation instance</param>
        /// <param name="dayOfMonth">Current day of the month</param>
        public static void DayUpdate(GameLocation __instance, int dayOfMonth)
        {
            if (customEntitiesServices.customBuildingService.customBuildingManager.CustomBuildings.TryGetValue(__instance.Name, out var building))
            {
                if (building.ProductionModifiers != null)
                {
                    foreach (string ruleId in building.ProductionModifiers.Keys)
                    {
                        switch (ruleId)
                        {
                            case "WaterMe":
                                foreach (var rule in building.ProductionModifiers[ruleId])
                                {
                                    switch (rule.ModificationTarget)
                                    {
                                        //
                                        //  water all HoeDirt tiles in a location
                                        //
                                        case ContentPackFramework.ContentPacks.ISDRContentPack.ModificationTargets.Crop:
                                            switch (rule.ModificationType)
                                            {
                                                case ContentPackFramework.ContentPacks.ISDRContentPack.ProductionModificationTypes.Speed:
                                                    __instance.WaterMe();
                                                    break;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case "BuildingOutput":
                                //
                                //  Building production modifiers
                                //
                                foreach (var rule in building.ProductionModifiers[ruleId])
                                {
                                    switch (rule.ModificationType)
                                    {
                                        //
                                        //  Custom Output from a Buidling
                                        //  i.e. batteries from a Solar Shed
                                        //
                                        case ContentPackFramework.ContentPacks.ISDRContentPack.ProductionModificationTypes.Output:
                                            if (__instance.modData.ContainsKey(key_BuildingOutputCount))
                                            {
                                                if (!string.IsNullOrEmpty(rule.OutputCondition))
                                                {
                                                    //string query = rule.OutputCondition.Replace("sdr.building", __instance.GetParentLocation().Name);
                                                    if (!GameStateQuery.CheckConditions(rule.OutputCondition, __instance))
                                                    {
                                                        logger.Log($"No production for {__instance.Name}, Condition '{rule.OutputCondition}' not met.", LogLevel.Debug);
                                                        continue;
                                                    }
                                                }
                                                if (int.TryParse(__instance.modData[key_BuildingOutputCount], out int count))
                                                {
                                                    if (++count >= rule.ModificationQuantity)
                                                    {
                                                        if (__instance.objects.TryGetValue(rule.OuputChestPosition, out var outputChest))
                                                        {
                                                            //
                                                            //  fix bug in current 1.6 code that adds chest as object
                                                            //
                                                            if (outputChest is not Chest && outputChest.QualifiedItemId == "(BC)130")
                                                            {
                                                                __instance.objects.Remove(rule.OuputChestPosition);
                                                                __instance.objects.Add(rule.OuputChestPosition, new Chest(true, rule.OuputChestPosition));
                                                                __instance.objects.TryGetValue(rule.OuputChestPosition, out outputChest);
                                                                logger.Log($"Fixed 1.6 output chest bug", LogLevel.Warn);
                                                            }

                                                            //  add output to building output chest
                                                            //
                                                            if (outputChest is Chest outputDestination)
                                                            {
                                                                outputDestination.addItem(ItemRegistry.Create(rule.OutputItemId, rule.OutputQuantity));
                                                                logger.Log($"Building output for {__instance.Name} added.", LogLevel.Debug);
                                                            }
                                                            else
                                                            {
                                                                logger.Log($"Output object is not a Chest for {__instance.Name}", LogLevel.Warn);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            logger.Log($"No output chest for {__instance.Name}", LogLevel.Warn);
                                                        }
                                                        count = 0;
                                                    }
                                                    __instance.modData[key_BuildingOutputCount] = count.ToString();
                                                }
                                            }
                                            else
                                            {
                                                //
                                                //  set initial building output details
                                                //
                                                __instance.modData[key_BuildingOutputCount] = "0";
                                                //
                                                //  this info gives other mods visibility of output state
                                                //
                                                __instance.modData[key_BuildingOutputItem] = rule.OutputItemId;
                                                __instance.modData[key_BuildingOutputQuantity] = rule.OutputQuantity.ToString();
                                                __instance.modData[key_BuildingOutputTime] = rule.ModificationQuantity.ToString();
                                                logger.Log($"Starting production for {__instance.Name}", LogLevel.Debug);
                                            }
                                            break;
                                    }
                                }
                                break;
                            case "AllCrops":
                                foreach (var rule in building.ProductionModifiers[ruleId])
                                {
                                    switch (rule.ModificationTarget)
                                    {
                                        //
                                        //  Apply location crop speed ups
                                        //
                                        case ContentPackFramework.ContentPacks.ISDRContentPack.ModificationTargets.Crop:
                                            switch (rule.ModificationType)
                                            {
                                                case ContentPackFramework.ContentPacks.ISDRContentPack.ProductionModificationTypes.Speed:
                                                    var crops = __instance.terrainFeatures.Pairs.Where(p => p.Value is HoeDirt && ((HoeDirt)p.Value).crop != null);

                                                    foreach (KeyValuePair<Vector2, TerrainFeature> crop in crops)
                                                    {
                                                        Crop subject = ((HoeDirt)crop.Value).crop;
                                                        if (crop.Value.modData.TryGetValue(key_Crop, out string currentCount))
                                                        {
                                                            if (int.TryParse(currentCount, out int count))
                                                            {
                                                                count++;
                                                                logger.LogDebug("Crop.DayUpdate", $"{__instance.Name}:{crop.Key}:  Current count: {count}");
                                                                if (count >= rule.ModificationQuantity)
                                                                {
                                                                    ((HoeDirt)crop.Value).crop.newDay(1);
                                                                    count = 0;
                                                                    logger.LogDebug("Crop.DayUpdate", "    bonus day applied");
                                                                }
                                                                crop.Value.modData[key_Crop] = count.ToString();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            crop.Value.modData[key_Crop] = "0";
                                                            logger.LogDebug("Crop.DayUpdate", $"{__instance.Name}:{crop.Key}:  Current count: 0");
                                                        }
                                                    }

                                                    break;
                                            }
                                            break;
                                    }


                                }
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Harmony postafix to handle any production speed modifiers for bigCraftables
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="timeElapsed"></param>
        public static void passTimeForObjects(GameLocation __instance, int timeElapsed)
        {
            __instance.objects.Lock();
            var candadites = __instance.objects.Pairs.Where(p => p.Value.bigCraftable.Value && p.Value.heldObject.Value != null);
            foreach (KeyValuePair<Vector2, SDObject> pair in candadites)
            {
                //
                //  check machine location rules
                //
                if (customEntitiesServices.customBigCraftableService.customBigCraftableManager.BigCraftables.TryGetValue(pair.Value.ItemId, out var bigCraftable))
                {
                    if (bigCraftable.ProductionModifiers != null && bigCraftable.ProductionModifiers.TryGetValue(__instance.Name, out var rules))
                    {
                        ApplyMachineModification(rules, __instance, pair, timeElapsed);
                    }
                }
                //
                //  check for location machine rules
                //
                if (customEntitiesServices.customBuildingService.customBuildingManager.CustomBuildings.TryGetValue(__instance.Name, out var building))
                {
                    if (building.ProductionModifiers != null && building.ProductionModifiers.TryGetValue(pair.Value.ItemId, out var rules))
                    {
                        ApplyMachineModification(rules, __instance, pair, timeElapsed);
                    }
                }
            }
            __instance.objects.Unlock();
        }

        /// <summary>
        /// Apply custom Building production modifiers
        /// - machine speedups
        /// </summary>
        /// <param name="ProductionModifiers">List of Building modifiers</param>
        /// <param name="__instance">GameLocation containing Objects</param>
        /// <param name="pair">List of Locaation Objects</param>
        /// <param name="timeElapsed">Game time that has elapsed</param>
        private static void ApplyMachineModification(List<CustomBigCraftableData.ProductionModifier> ProductionModifiers, GameLocation __instance, KeyValuePair<Vector2, SDObject> pair, int timeElapsed)
        {
            foreach (var modifier in ProductionModifiers)
            {
                switch (modifier.ModificationType)
                {
                    case CustomBigCraftableData.ProductionModificationTypes.Speed:
                        int extraElapsed = (int)(timeElapsed * (modifier.ModificationQuantity - 1));
                        if (pair.Value.minutesElapsed(extraElapsed))
                        {
                            Vector2 key = pair.Key;
                            __instance.objects.Remove(key);
                        }
                        break;
                }
            }
        }
    }
}
