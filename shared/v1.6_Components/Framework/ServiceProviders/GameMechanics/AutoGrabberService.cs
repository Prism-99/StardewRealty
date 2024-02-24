using Prism99_Core.Abstractions;
using Prism99_Core.Abstractions.Objects;
using Prism99_Core.Abstractions.TerrainFeatures;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.GameMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Events;
using StardewValley.Buildings;
using StardewValley.GameData.Crops;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDV_Realty_Core.Framework.ServiceProviders.GameMechanics
{
    internal class AutoGrabberService : IAutoGrabberService
    {
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),typeof(IModDataService)
        };
        private FEConfig config;
         private IModDataService modDataService;
        private List<string> quality = new List<string> { "basic", "silver", "gold", "3", "iridium" };
        private readonly int FARMING = 0;
        private readonly int FORAGING = 2;

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;

            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            modDataService = (IModDataService)args[1];

            config = utilitiesService.ConfigService.config;

            utilitiesService.GameEventsService.AddSubscription(typeof(ObjectListChangedEventArgs).Name, LocationEvents_ObjectsChanged);
            utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), DayStarted);


        }
        private bool IsGrabbableCoop(SDObject obj)
        {
            if (obj.bigCraftable.Value)
            {
                return obj.Name.Contains("Slime Ball");
            }
            if (obj.Name.Contains("Egg") || obj.Name.Contains("Wool") || obj.Name.Contains("Foot") || obj.Name.Contains("Feather"))
            {
                return true;
            }
            return false;
        }
        private void gainExperience(int skill, int xp)
        {
            Game1.player.gainExperience(skill, xp);
        }
        private void AutograbBuildings()
        {

            SDObject grabber = null;
            List<Vector2> grabbables = new List<Vector2>();
            Dictionary<string, int> itemsAdded = new Dictionary<string, int>();

            List<GameLocation> blocations = Game1.locations.Where(p => p.IsBuildableLocation()).Select(q => q as GameLocation).ToList();

#if DEBUG
            logger.Log($"Found {blocations.Count} buildable locations.", LogLevel.Debug);
#endif

            foreach (GameLocation bloc in blocations)
            {
                if ((modDataService.farmExpansions.ContainsKey(bloc.NameOrUniqueName) && config.EnableDeluxeAutoGrabberOnExpansions) || (config.EnableDeluxeAutoGrabberOnHomeFarm && bloc is Farm))
                {
                    foreach (Building building in bloc.buildings)
                    {
                        grabber = null;
                        grabbables.Clear();
                        itemsAdded.Clear();
                        if (building.buildingType.Contains("Coop") || building.buildingType.Contains("Slime"))
                        {
                            logger.Log($"Searching {building.buildingType} at <{building.tileX},{building.tileY}> for auto-grabber", LogLevel.Debug);
                            GameLocation location = building.indoors.Value;
                            if (location != null)
                            {
                                var pairs = location.Objects.Pairs;
                                var enumerator2 = pairs.GetEnumerator();
                                try
                                {
                                    while (enumerator2.MoveNext())
                                    {
                                        KeyValuePair<Vector2, SDObject> pair2 = enumerator2.Current;
                                        if (pair2.Value.Name.Contains("Grabber"))
                                        {
#if DEBUG
                                            logger.Log($"  Grabber found  at {building.GetIndoorsName()}: {pair2.Key}", LogLevel.Debug);
#endif
                                            grabber = pair2.Value;
                                        }
                                        if (pair2.Value != null && IsGrabbableCoop(pair2.Value))
                                        {
#if DEBUG
                                            logger.Log($"    Found grabbable item at {pair2.Key}: {pair2.Value.Name}", LogLevel.Debug);
#endif
                                            grabbables.Add(pair2.Key);
                                        }
                                    }
                                }
                                finally
                                {
                                    enumerator2.Dispose();
                                }
                                if (grabber == null)
                                {
                                    logger.Log("  No grabber found", LogLevel.Info);
                                    continue;
                                }
                                bool full = false;
                                foreach (Vector2 tile in grabbables)
                                {
                                    if ((grabber.heldObject.Value as prism_Chest).items.Count >= 36)
                                    {
                                        logger.Log("  Grabber is full", LogLevel.Info);
                                        full = true;
                                        break;
                                    }
                                    if (location.objects[tile].Name.Contains("Slime Ball"))
                                    {
                                        Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame + (int)tile.X * 77 + (int)tile.Y * 777 + 2);
                                        (grabber.heldObject.Value as Chest).addItem(new SDObject("(O)766", random.Next(10, 21)));
                                        int i = 0;
                                        while (random.NextDouble() < 0.33)
                                        {
                                            i++;
                                        }
                                        if (i > 0)
                                        {
                                            (grabber.heldObject.Value as Chest).addItem(new SDObject("(O)557", i));
                                        }
                                    }
                                    else if ((grabber.heldObject.Value as Chest).addItem(location.Objects[tile]) != null)
                                    {
                                        continue;
                                    }
                                    string name = location.Objects[tile].Name;
                                    if (!itemsAdded.ContainsKey(name))
                                    {
                                        itemsAdded.Add(name, 1);
                                    }
                                    else
                                    {
                                        itemsAdded[name]++;
                                    }
                                    location.Objects.Remove(tile);
                                    if (config.DoGainExperience)
                                    {
                                        gainExperience(FARMING, 5);
                                    }
                                }
                                if (full)
                                {
                                    continue;
                                }
                                foreach (KeyValuePair<string, int> pair in itemsAdded)
                                {
                                    string plural = "";
                                    if (pair.Value != 1)
                                    {
                                        plural = "s";
                                    }
#if DEBUG
                                    logger.Log($"  Added {pair.Value} {pair.Key}{plural}", LogLevel.Debug);
#endif
                                }
                            }
                        }
                        if (grabber != null && ((grabber.heldObject.Value as prism_Chest)?.items.Count ?? 0) > 0)
                        {
                            grabber.showNextIndex.Value = true;
                        }
                    }
                }
            }
        }
        private List<Item> GetFruit(FruitTree fruitTree, Vector2 tile, GameLocation location)
        {
            List<Item> fruitsReturned = new List<Item> { };
            prism_FruitTree pFruit = new prism_FruitTree(fruitTree);
            int quality = 0;
            if (fruitTree == null)
            {
                return null;
            }
#if DEBUG
            logger.Log($"Found fruit in {location.Name} ({tile})", LogLevel.Debug);
#endif
            if (!config.DoHarvestFruitTrees)
            {
                return null;
            }
            if (fruitTree.fruit.Count > 0)
            {
                foreach (var tfruit in fruitTree.fruit)
                {
                    fruitsReturned.Add(tfruit);
                }
                fruitTree.fruit.Clear();
            }

            //if (fruitTree.growthStage.Value >= 4 && pFruit.fruitsOnTree > 0)
            //{
            //    if (fruitTree.daysUntilMature.Value <= -112)
            //    {
            //        quality = 1;
            //    }
            //    if (fruitTree.daysUntilMature.Value <= -224)
            //    {
            //        quality = 2;
            //    }
            //    if (fruitTree.daysUntilMature.Value <= -336)
            //    {
            //        quality = 4;
            //    }
            //    if (fruitTree.struckByLightningCountdown.Value > 0)
            //    {
            //        quality = 0;
            //    }
            //    fruit = new SDObject(pFruit.indexOfFruit.ToString(), pFruit.fruitsOnTree, isRecipe: false, -1, quality);
            //    pFruit.fruitsOnTree = 0;
            //    pFruit.fruit.Clear();
            //    return fruit;
            //}
            return fruitsReturned;
        }
        private void AutograbCrops()
        {
            if (!config.DoHarvestCrops && !config.DoHarvestTruffles)
            {
                return;
            }
            int range = config.GrabberRange;
            logger.Log($"Auto grabbing crops within {range} tiles", LogLevel.Debug);

            HoeDirt dirt = default(HoeDirt);
            IndoorPot pot = default(IndoorPot);
            FruitTree fruitTree = default(FruitTree);
            List<GameLocation> searchLocs = Game1.locations.ToList();

            List<GameLocation> buildings = Game1.locations.Where(p => p.IsBuildableLocation()).ToList();

            foreach (GameLocation bgl in buildings)
            {
                searchLocs.AddRange(bgl.buildings.Where(p => p.indoors.Value != null).Select(p => p.indoors.Value).ToList());
            }

            foreach (GameLocation location in searchLocs)
            {
                var pairs = location.Objects.Pairs;
                var enumerator2 = pairs.GetEnumerator();
                try
                {
                    while (enumerator2.MoveNext())
                    {
                        SDObject grabber;
                        KeyValuePair<Vector2, SDObject> pair = enumerator2.Current;
                        if (pair.Value.Name.Contains("Grabber"))
                        {
#if DEBUG
                            logger.Log($"     Found grabber, checking for Chest", LogLevel.Debug);
#endif
                            if (pair.Value.heldObject.Value == null)
                            {
                                //if(location.objects.TryGetValue(pair.Key, out SDObject emptChest))
                                //{
                                grabber = pair.Value;
                                grabber.heldObject.Value = new Chest();
                                //grabber = emptChest;
                                //}

                                //SDObject emptChest = location.getObjectAt((int)pair.Key.X, (int)pair.Key.Y);
                            }
                            else
                            {
                                grabber = pair.Value;
                            }
                        }
                        else
                        {
                            continue;
                        }
                        logger.Log($"Found grabber {location.Name}.{pair.Key.X}X{pair.Key.Y}", LogLevel.Debug);

                        if (IsChestFull(grabber))
                        {
                            continue;
                        }
                        Chest grabberChest = grabber.heldObject.Value as Chest;
                        bool full = false;
                        logger.Log($"      Grabber not null or full: {full}", LogLevel.Debug);
                        for (int x = (int)pair.Key.X - range; (float)x < pair.Key.X + (float)range + 1f; x++)
                        {
                            for (int y = (int)pair.Key.Y - range; (float)y < pair.Key.Y + (float)range + 1f; y++)
                            {
                                if (full)
                                {
                                    break;
                                }
                                Vector2 tile = new Vector2(x, y);
                                int num;
                                if (location.terrainFeatures.ContainsKey(tile))
                                {
                                    dirt = location.terrainFeatures[tile] as HoeDirt;
                                    num = ((dirt != null) ? 1 : 0);
                                }
                                else
                                {
                                    num = 0;
                                }
                                if (num != 0)
                                {
                                    SDObject harvest2 = GetHarvest(dirt, tile, location);
                                    if (harvest2 != null)
                                    {
                                        grabberChest.addItem(harvest2);
                                        if (config.DoGainExperience)
                                        {
                                            gainExperience(FORAGING, 3);
                                        }
                                    }
                                }
                                else
                                {
                                    int num2;
                                    if (location.Objects.ContainsKey(tile))
                                    {
                                        pot = location.Objects[tile] as IndoorPot;
                                        num2 = ((pot != null) ? 1 : 0);
                                    }
                                    else
                                    {
                                        num2 = 0;
                                    }
                                    if (num2 != 0)
                                    {
                                        SDObject harvest = GetHarvest(pot.hoeDirt.Value, tile, location);
                                        if (harvest != null)
                                        {
                                            grabberChest.addItem(harvest);
                                            if (config.DoGainExperience)
                                            {
                                                gainExperience(FORAGING, 3);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int num3;
                                        if (location.terrainFeatures.ContainsKey(tile))
                                        {
                                            fruitTree = location.terrainFeatures[tile] as FruitTree;
                                            num3 = ((fruitTree != null) ? 1 : 0);
                                        }
                                        else
                                        {
                                            num3 = 0;
                                        }
                                        if (num3 != 0)
                                        {
                                            List<Item> fruits = GetFruit(fruitTree, tile, location);

                                            foreach (var treeFruit in fruits)
                                            {
                                                grabberChest.addItem(treeFruit);
                                                if (config.DoGainExperience)
                                                {
                                                    gainExperience(FORAGING, 3);
                                                }
                                            }

                                            //if (fruit != null)
                                            //{
                                            //    grabberChest.addItem(fruit);
                                            //    if (config.DoGainExperience)
                                            //    {
                                            //        gainExperience(FORAGING, 3);
                                            //    }
                                            //}
                                        }
                                        else if (config.DoHarvestTruffles && location.Objects.ContainsKey(tile))
                                        {
                                            //
                                            //  look for truffles
                                            //
                                            SDObject truffle = location.Objects[tile];
                                            if (truffle != null && truffle.ParentSheetIndex == 430)
                                            {
                                                AddTruffleToChest(new KeyValuePair<Vector2, SDObject>(tile, truffle), grabberChest);
                                                location.Objects.Remove(tile);
                                            }

                                        }
                                    }
                                }
                                full = IsChestFull(grabber);
                            }
                        }

                        if (((Chest)grabber.heldObject.Value).Items.Count > 0)
                        {
                            grabber.showNextIndex.Value = true;
                        }
                    }
                }
                finally
                {
                    enumerator2.Dispose();
                }
            }
        }
        private bool IsChestFull(SDObject grabber)
        {
            if (grabber == null || grabber.heldObject.Value == null)
            {
                return true;
            }
            prism_Chest pChest = new prism_Chest(grabber.heldObject.Value as Chest);
            return pChest.items.Count >= 36;
        }
        private SDObject GetHarvest(HoeDirt dirt, Vector2 tile, GameLocation location)
        {
            //Crop crop = dirt.crop;
            int stack = 0;
            if (dirt.crop == null)
            {
                return null;
            }

            prism_Crop crop = new prism_Crop(dirt.crop);
            prism_HoeDirt fedirt = new prism_HoeDirt(dirt);

            if (!config.DoHarvestFlowers)
            {

                switch (crop.IndexOfHarvest)
                {
                    case "421":
                        return null;
                    case "593":
                        return null;
                    case "595":
                        return null;
                    case "591":
                        return null;
                    case "597":
                        return null;
                    case "376":
                        return null;
                }
            }
            //if (crop.currentPhase.Value >= crop.phaseDays.Count - 1 && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0))
            if (dirt.readyForHarvest())
            {
                int num1 = 1;
                int num2 = 0;
                int num3 = 0;
                if (crop.IndexOfHarvest == "0")
                {
                    return null;
                }
                Random random = new Random((int)tile.X * 7 + (int)tile.Y * 11 + (int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame);
                switch (fedirt.Fertilizer)
                {
                    case "368":
                        num3 = 1;
                        break;
                    case "369":
                        num3 = 2;
                        break;
                }
                double num4 = 0.2 * ((double)Game1.player.FarmingLevel / 10.0) + 0.2 * (double)num3 * (((double)Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
                double num5 = Math.Min(0.75, num4 * 2.0);
                if (random.NextDouble() < num4)
                {
                    num2 = 2;
                }
                else if (random.NextDouble() < num5)
                {
                    num2 = 1;
                }
                if (crop.minHarvest > 1 || crop.maxHarvest > 1)
                {
                    num1 = ((crop.maxHarvestIncreasePerFarmingLevel <= 0) ? 1 : random.Next(crop.minHarvest, (int)Math.Min(crop.minHarvest + 1, crop.maxHarvest + 1 + Game1.player.FarmingLevel / crop.maxHarvestIncreasePerFarmingLevel)));
                }
                if (crop.chanceForExtraCrops > 0.0)
                {
                    while (random.NextDouble() < Math.Min(0.9, crop.chanceForExtraCrops))
                    {
                        num1++;
                    }
                }
                if (crop.harvestMethod == HarvestMethod.Scythe)
                {
                    for (int i = 0; i < num1; i++)
                    {
                        stack++;
                    }
                    if (crop.RegrowAfterHarvest != -1)
                    {
                        dirt.crop.currentPhase.Value = crop.RegrowAfterHarvest;
                        dirt.crop.dayOfCurrentPhase.Value = 0;
                        dirt.crop.fullyGrown.Value = false;
                    }
                    else
                    {
                        dirt.destroyCrop(true);
                    }

                    return new SDObject(crop.indexOfHarvest, stack, isRecipe: false, -1, num2);
                }
                SDObject harvest;
                if (!crop.programColored.Value)
                {
                    harvest = new SDObject(crop.indexOfHarvest, 1, isRecipe: false, -1, num2);
                }
                else
                {
                    ColoredObject coloredObject = new ColoredObject(crop.indexOfHarvest, 1, crop.tintColor.Value);
                    coloredObject.Quality = num2;
                    harvest = coloredObject;
                }
                if (crop.RegrowAfterHarvest != -1)
                {
                    dirt.crop.currentPhase.Value = crop.RegrowAfterHarvest;
                    dirt.crop.dayOfCurrentPhase.Value = 0;
                    dirt.crop.fullyGrown.Value = false;

                    //crop.dayOfCurrentPhase.Value = crop.RegrowAfterHarvest;
                    //crop.fullyGrown.Value = false;
                }
                else
                {
                    dirt.crop = null;
                }
                return harvest;
            }
            return null;
        }
        private bool IsGrabbableWorld(SDObject obj)
        {
            if (obj.bigCraftable.Value)
            {
                return false;
            }
            switch (obj.ParentSheetIndex)
            {
                case 16:
                case 18:
                case 20:
                case 22:
                case 78:
                case 88:
                case 90:
                case 257:
                case 259:
                case 281:
                case 283:
                case 296:
                case 372:
                case 392:
                case 393:
                case 394:
                case 396:
                case 397:
                case 398:
                case 399:
                case 402:
                case 404:
                case 406:
                case 408:
                case 410:
                case 412:
                case 414:
                case 416:
                case 418:
                case 420:
                case 430:
                case 718:
                case 719:
                case 723:
                    return true;
                default:
                    //return false;
#if v16
                    SDObject grab = new SDObject(obj.ParentSheetIndex.ToString(), 1);
#else
                    SDObject grab = new SDObject(obj.ParentSheetIndex, 1);
#endif
                    if (grab is null) { return false; }
                    return grab.Category == -23;
            }
        }
        private void AutograbWorld()
        {
            if (!config.DoGlobalForage)
            {
                return;
            }
            List<Vector2> grabbables = new List<Vector2>();
            Dictionary<string, int> itemsAdded = new Dictionary<string, int>();
            Random random = new Random();
            if (string.IsNullOrEmpty(config.GlobalForageMap))
                return;
            GameLocation foragerMap = Game1.getLocationFromName(config.GlobalForageMap);
            if (foragerMap == null)
            {
                logger.LogOnce($"Invalid GlobalForageMap: {config.GlobalForageMap}", LogLevel.Error);
                return;
            }
            foragerMap.Objects.TryGetValue(new Vector2(config.GlobalForageTileX, config.GlobalForageTileY), out var grabber);
            if (grabber == null || !grabber.Name.Contains("Grabber"))
            {
                logger.Log($"No auto-grabber at {config.GlobalForageMap}: <{config.GlobalForageTileX}, {config.GlobalForageTileY}>", LogLevel.Error);
                return;
            }
            prism_Chest pChest = new prism_Chest(grabber.heldObject.Value as Chest);

            foreach (GameLocation location in Game1.locations)
            {
                grabbables.Clear();
                itemsAdded.Clear();

                IEnumerable<KeyValuePair<Vector2, SDObject>> pairs = location.Objects.Pairs;
                var enumerator2 = pairs.GetEnumerator();
                try
                {
                    while (enumerator2.MoveNext())
                    {
                        KeyValuePair<Vector2, SDObject> pair2 = enumerator2.Current;
#if v16
                        if (!pair2.Value.bigCraftable.Value && (IsGrabbableWorld(pair2.Value) || pair2.Value.isForage()))
#else
                        if (!pair2.Value.bigCraftable.Value && (IsGrabbableWorld(pair2.Value) || pair2.Value.isForage(null)))
#endif
                        {
                            grabbables.Add(pair2.Key);
                        }
                    }
                }
                finally
                {
                    enumerator2.Dispose();
                }
                if (location.Name.Equals("Forest"))
                {
                    foreach (TerrainFeature feature in location.terrainFeatures.Values)
                    {
                        if (pChest.items.Count >= 36)
                        {
                            logger.Log("Global grabber full", LogLevel.Info);
                            return;
                        }
                        if (feature as HoeDirt is null || ((HoeDirt)feature).crop is null)
                        {
                            continue;
                        }
                        prism_HoeDirt dirt = new prism_HoeDirt(feature as HoeDirt);
                        prism_Crop pCrop = new prism_Crop(dirt.instance.crop);

                        if (!pCrop.instance.forageCrop.Value || pCrop.whichForageCrop != "1")
                        {
                            continue;
                        }
                        SDObject onion = new SDObject("(O)399", 1);
                        if (Game1.player.professions.Contains(16))
                        {
                            onion.Quality = 4;
                        }
                        else if (random.NextDouble() < (double)Game1.player.ForagingLevel / 30.0)
                        {
                            onion.Quality = 2;
                        }
                        else if (random.NextDouble() < (double)Game1.player.ForagingLevel / 15.0)
                        {
                            onion.Quality = 1;
                        }
                        if (Game1.player.professions.Contains(13))
                        {
                            while (random.NextDouble() < 0.2)
                            {
                                onion.Stack++;
                            }
                        }
                        (grabber.heldObject.Value as Chest).addItem(onion);
                        if (!itemsAdded.ContainsKey("Spring Onion"))
                        {
                            itemsAdded.Add("Spring Onion", 1);
                        }
                        else
                        {
                            itemsAdded["Spring Onion"]++;
                        }
                        dirt.crop = null;
                        if (config.DoGainExperience)
                        {
                            gainExperience(FORAGING, 3);
                        }
                    }
                }
                foreach (LargeTerrainFeature feature2 in location.largeTerrainFeatures)
                {
                    string berryType;
                    int berryIndex;
                    if (Game1.currentSeason == "spring")
                    {
                        berryType = "Salmon Berry";
                        berryIndex = 296;
                    }
                    else
                    {
                        if (!(Game1.currentSeason == "fall"))
                        {
                            break;
                        }
                        berryType = "Blackberry";
                        berryIndex = 410;
                    }
                    if (pChest.items.Count >= 36)
                    {
                        logger.Log("Global grabber full", LogLevel.Info);
                        return;
                    }
                    Bush bush = feature2 as Bush;
                    bool inBloom;

                    inBloom = bush.inBloom();
                    if (bush != null && inBloom && bush.tileSheetOffset.Value == 1)
                    {
                        SDObject berry = new SDObject(berryIndex.ToString(), 1 + Game1.player.FarmingLevel / 4);
                        if (Game1.player.professions.Contains(16))
                        {
                            berry.Quality = 4;
                        }
                        bush.tileSheetOffset.Value = 0;
                        bush.setUpSourceRect();
                        Item item2 = (grabber.heldObject.Value as Chest).addItem(berry);
                        if (!itemsAdded.ContainsKey(berryType))
                        {
                            itemsAdded.Add(berryType, 1);
                        }
                        else
                        {
                            itemsAdded[berryType]++;
                        }
                    }
                }
                foreach (Vector2 tile in grabbables)
                {
                    if (pChest.items.Count >= 36)
                    {
                        logger.Log("Global grabber full", LogLevel.Info);
                        return;
                    }
                    SDObject obj2 = location.Objects[tile];
                    if (Game1.player.professions.Contains(16))
                    {
                        obj2.Quality = 4;
                    }
                    else if (random.NextDouble() < (double)Game1.player.ForagingLevel / 30.0)
                    {
                        obj2.Quality = 2;
                    }
                    else if (random.NextDouble() < (double)Game1.player.ForagingLevel / 15.0)
                    {
                        obj2.Quality = 1;
                    }
                    if (Game1.player.professions.Contains(13))
                    {
                        while (random.NextDouble() < 0.2)
                        {
                            obj2.Stack++;
                        }
                    }
                    Item item = (grabber.heldObject.Value as Chest).addItem(obj2);
                    string name2 = location.Objects[tile].Name;
                    if (!itemsAdded.ContainsKey(name2))
                    {
                        itemsAdded.Add(name2, 1);
                    }
                    else
                    {
                        itemsAdded[name2]++;
                    }
                    location.Objects.Remove(tile);
                    if (config.DoGainExperience)
                    {
                        gainExperience(FORAGING, 7);
                    }
                }
                if (config.DoHarvestFarmCave && location is FarmCave)
                {
                    var values = location.Objects.Values;
                    var enumerator6 = values.GetEnumerator();
                    try
                    {
                        while (enumerator6.MoveNext())
                        {
                            SDObject obj = enumerator6.Current;
                            if (pChest.items.Count >= 36)
                            {
                                logger.Log("Global grabber full", LogLevel.Info);
                                return;
                            }
                            if (obj.bigCraftable.Value && obj.ParentSheetIndex == 128 && obj.heldObject.Value != null)
                            {
                                (grabber.heldObject.Value as Chest).addItem(obj.heldObject.Value);
                                string name = grabber.heldObject.Value.Name;
                                if (!itemsAdded.ContainsKey(name))
                                {
                                    itemsAdded.Add(name, 1);
                                }
                                else
                                {
                                    itemsAdded[name]++;
                                }
                                obj.heldObject.Value = null;
                            }
                        }
                    }
                    finally
                    {
                        ((IDisposable)(enumerator6)).Dispose();
                    }
                }
                foreach (KeyValuePair<string, int> pair in itemsAdded)
                {
                    string plural = "";
                    if (pair.Value != 1)
                    {
                        plural = "s";
                    }
                    logger.Log($"  {location} - found {pair.Value} {pair.Key}{plural}", LogLevel.Debug);
                }
                if (pChest.items.Count > 0)
                {
                    grabber.showNextIndex.Value = true;
                }
            }
        }
        private void AddTruffleToChest(KeyValuePair<Vector2, SDObject> pair, SDObject grabber)
        {
            AddTruffleToChest(pair, grabber.heldObject.Value as Chest);
        }
        private void AddTruffleToChest(KeyValuePair<Vector2, SDObject> pair, Chest grabber)
        {
            Random random = new Random();

            SDObject obj = pair.Value;
            if (obj.Stack == 0)
            {
                obj.Stack = 1;
            }

            if (Game1.player.professions.Contains(16))
            {
                obj.Quality = 4;
            }
            else if (random.NextDouble() < (double)Game1.player.ForagingLevel / 30.0)
            {
                obj.Quality = 2;
            }
            else if (random.NextDouble() < (double)Game1.player.ForagingLevel / 15.0)
            {
                obj.Quality = 1;
            }
            if (Game1.player.professions.Contains(13))
            {
                while (random.NextDouble() < 0.2)
                {
                    obj.Stack++;
                }
            }
            logger.Log($"Grabbing truffle: {obj.Stack}x{quality[obj.Quality]}", LogLevel.Debug);
            grabber.addItem(obj);
            if (config.DoGainExperience)
            {
                gainExperience(FORAGING, 7);
            }

        }
        private void DayStarted(EventArgs e)
        {
            //
            //  only run if Main player
            //
            if (config.EnableDeluxeAutoGrabberOnExpansions || config.EnableDeluxeAutoGrabberOnHomeFarm)
            {
                if (Context.IsMainPlayer)
                {
                    logger.Log($"SDR.Autograbber DayStarted", LogLevel.Debug);

                    if (config.EnableDeluxeAutoGrabberOnHomeFarm || config.EnableDeluxeAutoGrabberOnExpansions)
                    {
                        try
                        {
                            AutograbBuildings();
                            AutograbCrops();
                            AutograbWorld();
                        }
                        catch (Exception ex) { logger.LogError("AutoGrabber.DayStarted", ex); }
                    }
                }
            }
        }

        private void LocationEvents_ObjectsChanged(EventArgs ep)
        {
            ObjectListChangedEventArgs e = (ObjectListChangedEventArgs)ep;
            if (!config.DoHarvestTruffles || string.IsNullOrEmpty(config.GlobalForageMap))
            {
                return;
            }
            GameLocation foragerMap = Game1.getLocationFromName(config.GlobalForageMap);
            if (foragerMap == null)
            {
                logger.LogOnce($"Invalid GlobalForageMap name: {config.GlobalForageMap}", LogLevel.Error);
                return;
            }
            foragerMap.Objects.TryGetValue(new Vector2(config.GlobalForageTileX, config.GlobalForageTileY), out var grabber);
            if (grabber == null || !grabber.Name.Contains("Grabber"))
            {
                logger.LogOnce($"Object is not a valid grabber ({config.GlobalForageTileX},{config.GlobalForageTileY})", LogLevel.Error);
                return;
            }
            if (grabber.heldObject.Value == null)
            {
                logger.LogOnce($"Grabber heldObject is null", LogLevel.Error);
                return;
            }
            Random random = new Random();
            foreach (KeyValuePair<Vector2, SDObject> pair in e.Added)
            {
                if (pair.Value.ParentSheetIndex != 430 || pair.Value.bigCraftable.Value)
                {
                    continue;
                }
#if v16
#else
                if (IsChestFull(grabber))
                {
                    return;
                }
#endif
                AddTruffleToChest(pair, grabber);
                e.Location.Objects.Remove(pair.Key);
            }
            prism_Chest pChest = new prism_Chest(grabber.heldObject.Value as Chest);
            if (pChest.items.Count > 0)
            {
                grabber.showNextIndex.Value = true;
            }
        }

    }
}
