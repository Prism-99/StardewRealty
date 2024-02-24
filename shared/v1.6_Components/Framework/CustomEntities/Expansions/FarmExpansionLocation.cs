using System.Collections.Generic;
using Prism99_Core.Utilities;
using StardewValley.Buildings;
using xTile;
using System.Xml.Serialization;
using StardewRealty.SDV_Realty_Interface;
using StardewValley.BellsAndWhistles;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;
using System.Linq;
using SDV_Realty_Core.Framework.Buildings.CheeseFactory;
using SDV_Realty_Core.Framework.Buildings.Greenhouses;

namespace SDV_Realty_Core.Framework.Expansions
{
    [XmlInclude(typeof(FromagerieLocation))]
    //[XmlInclude(typeof(FromagerieBuilding))]
    //[XmlInclude(typeof(WineryBuilding))]
    //[XmlInclude(typeof(SDRGreenhouse))]
    [XmlInclude(typeof(GreenhouseLocation))]
    [XmlInclude(typeof(GreenhouseInterior))]
    [XmlInclude(typeof(LargeGreenhouseLocation))]
    [XmlInclude(typeof(LargeGreenhouseInterior))]
    //[XmlInclude(typeof(BreadFactoryBuilding))]
    //[XmlInclude(typeof(BreadFactoryLocation))]
    public class FarmExpansionLocation : GameLocation
    {
        //
        //  version 1.6
        //
        private static SDVLogger logger;
        private ExpansionManager expansionManager;
        public FarmExpansionLocation() { }
        //
        //  cannot use a game loader always appends .xnb to filename
        //
        //internal FarmExpansionLocation(string mapPath, string a, SDVLogger olog) : base(mapPath,a)
        //{
        //    logger = olog;
        //    //this.Map = omap;
        //    //this.name.Value = name;
        //    AutoAdd = true;
        //    GridId = -1;
        //}
        internal FarmExpansionLocation(Map omap, string mapPathValue, string locationName, SDVLogger olog, ExpansionManager expansionManager) : base()
        {
            logger = olog;
            Map = omap;
            mapPath.Value = mapPathValue;
            name.Value = locationName;
            AutoAdd = true;
            GridId = -1;
            this.expansionManager=expansionManager;
        }

        [XmlIgnoreAttribute]
        public Dictionary<string, FishAreaDetails> FishAreas;
        public bool Active { get; set; }
        public long PurchasedBy { get; set; }
        public int GridId { get; set; }
        public bool AutoAdd { get; set; }
        public bool OfferLetterRead { get; set; }
        public string SeasonOverride { get; set; }
        public List<SuspensionBridge> suspensionBridges = new List<SuspensionBridge>();

        public bool StockedPonds { get; set; }
        public bool CrowsEnabled { get; set; }

        public override void seasonUpdate(bool onLoad = false)
        {
            Season season = GetSeason();
            terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> pair) => pair.Value.seasonUpdate(onLoad));
            largeTerrainFeatures?.RemoveWhere((LargeTerrainFeature feature) => feature.seasonUpdate(onLoad));
            foreach (NPC character in characters)
            {
                if (!character.IsMonster)
                {
                    character.resetSeasonalDialogue();
                }
            }

            if (IsOutdoors && !onLoad)
            {
                KeyValuePair<Vector2, SDObject>[] array = objects.Pairs.ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    KeyValuePair<Vector2, SDObject> keyValuePair = array[i];
                    Vector2 key = keyValuePair.Key;
                    SDObject value = keyValuePair.Value;
                    if (value.IsSpawnedObject && !value.IsBreakableStone())
                    {
                        objects.Remove(key);
                    }
                    else if (value.QualifiedItemId == "(O)590" && doesTileHavePropertyNoNull((int)key.X, (int)key.Y, "Diggable", "Back") == "")
                    {
                        objects.Remove(key);
                    }
                }

                numberOfSpawnedObjectsOnMap = 0;
            }

            switch (season)
            {
                case Season.Spring:
                    waterColor.Value = new Color(120, 200, 255) * 0.5f;
                    break;
                case Season.Summer:
                    waterColor.Value = new Color(60, 240, 255) * 0.5f;
                    break;
                case Season.Fall:
                    waterColor.Value = new Color(255, 130, 200) * 0.5f;
                    break;
                case Season.Winter:
                    waterColor.Value = new Color(130, 80, 255) * 0.5f;
                    break;
            }

            if (!onLoad && season == Season.Spring && Game1.stats.DaysPlayed > 1)
            {
                loadWeeds();
            }
        }
       
        //public new void reloadMap()
        //{
        //    base.reloadMap();
        //}
        private string GetLocContextId()
        {
            if (locationContextId == null)
            {
                if (map == null)
                {
                    reloadMap();
                }
                if (map != null && map.Properties.TryGetValue("LocationContext", out var contextId))
                {
                    if (Game1.locationContextData.ContainsKey(contextId))
                    {
                        locationContextId = contextId;
                    }
                    else
                    {
                        logger.Log($"Could not find context: {contextId}", StardewModdingAPI.LogLevel.Debug);
                        //IGameLogger log = Game1.log;
                        //DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
                        //defaultInterpolatedStringHandler.AppendLiteral("Location ");
                        //defaultInterpolatedStringHandler.AppendFormatted(NameOrUniqueName);
                        //defaultInterpolatedStringHandler.AppendLiteral(" has invalid LocationContext map property '");
                        //defaultInterpolatedStringHandler.AppendFormatted(contextId);
                        //defaultInterpolatedStringHandler.AppendLiteral("', ignoring value.");
                        //log.Error(defaultInterpolatedStringHandler.ToStringAndClear());
                    }
                }
                if (locationContextId == null)
                {
                    locationContextId = GetParentLocation()?.GetLocationContextId() ?? "Default";
                }
            }
            return locationContextId;
        }
        public override bool IsLocationSpecificPlacementRestriction(Vector2 tileLocation)
        {
            foreach (SuspensionBridge suspensionBridge in suspensionBridges)
            {
                if (suspensionBridge.CheckPlacementPrevention(tileLocation))
                {
                    return true;
                }
            }
            return base.IsLocationSpecificPlacementRestriction(tileLocation);
        }
        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
            foreach (SuspensionBridge suspensionBridge in suspensionBridges)
            {
                suspensionBridge.Update(time);
            }            
        }
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            foreach (SuspensionBridge suspensionBridge in suspensionBridges)
            {
                suspensionBridge.Draw(b);
            }
        }
        //public override string GetLocationContextId()
        //{
        //    string baseId = GetLocContextId();// base.GetLocationContextId();

        //    return baseId;
        //}
        public void FixAnimals()
        {
            foreach (Building building in buildings)
            {
                FixBuildingAnimals(building);
            }
        }

#if false
        public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
        {
            if (locationName != null && locationName != Name && (!(locationName == "UndergroundMine") || !(this is MineShaft)))
            {
                GameLocation location = Game1.getLocationFromName(locationName);
                if (location != null && location != this)
                {
                    return location.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
                }
            }
            if (bobberTile != Vector2.Zero && who.currentLocation?.NameOrUniqueName == NameOrUniqueName)
            {
                foreach (Building building in buildings)
                {
                    FishPond pond = building as FishPond;
                    if (pond != null && pond.isTileFishable(bobberTile))
                    {
                        return pond.CatchFish();
                    }
                }
            }
            bool isTutorialCatch = who.fishCaught.Length == 0;
            return GetFishFromLocationData(Name, bobberTile, waterDepth, who, isTutorialCatch, isInherited: false, this) ?? ItemRegistry.Create("(O)168");
        }
        public new static Item GetFishFromLocationData(string locationName, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, GameLocation location = null)
        {
            return GetFishFromLocationData(locationName, bobberTile, waterDepth, player, isTutorialCatch, isInherited, location, null);
        }
        internal static Item GetFishFromLocationData(string locationName, Vector2 bobberTile, int waterDepth, Farmer player, bool isTutorialCatch, bool isInherited, GameLocation location, ItemQueryContext itemQueryContext)
        {
            if (location == null)
            {
                location = Game1.getLocationFromName(locationName);
            }
            Dictionary<string, LocationData> dictionary = Game1.content.Load<Dictionary<string, LocationData>>("Data\\Locations");
            LocationData locationData = ((location != null) ? location.GetData() : GetData(locationName));
            Dictionary<string, string> allFishData = Game1.content.Load<Dictionary<string, string>>("Data\\Fish");
            Season season = Game1.GetSeasonForLocation(location);
            if (location == null || !location.TryGetFishAreaForTile(bobberTile, out var fishAreaId, out var _))
            {
                fishAreaId = null;
            }
            bool usingMagicBait = false;
            bool hasCuriosityLure = false;
            FishingRod rod = player?.CurrentTool as FishingRod;
            if (rod != null)
            {
                usingMagicBait = rod.HasMagicBait();
                hasCuriosityLure = rod.HasCuriosityLure();
            }
            Point playerTile = player.TilePoint;
            if (itemQueryContext == null)
            {
                itemQueryContext = new ItemQueryContext(location, null, Game1.random);
            }
            IEnumerable<SpawnFishData> possibleFish = dictionary["Default"].Fish;
            if (locationData != null && locationData.Fish?.Count > 0)
            {
                possibleFish = possibleFish.Concat(locationData.Fish);
            }
            possibleFish = from p in possibleFish
                           orderby p.Precedence, Game1.random.Next()
                           select p;
            HashSet<string> ignoreQueryKeys = (usingMagicBait ? GameStateQuery.MagicBaitIgnoreQueryKeys : null);
            foreach (SpawnFishData spawn in possibleFish)
            {
                if ((isInherited && !spawn.CanBeInherited) || (spawn.FishAreaId != null && fishAreaId != spawn.FishAreaId) || (spawn.Season.HasValue && !usingMagicBait && spawn.Season != season))
                {
                    continue;
                }
                Microsoft.Xna.Framework.Rectangle? playerPosition = spawn.PlayerPosition;
                if (playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains(playerTile.X, playerTile.Y))
                {
                    continue;
                }
                playerPosition = spawn.BobberPosition;
                if ((playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains((int)bobberTile.X, (int)bobberTile.Y)) || player.FishingLevel < spawn.MinFishingLevel || waterDepth < spawn.MinDistanceFromShore || (spawn.MaxDistanceFromShore > -1 && waterDepth > spawn.MaxDistanceFromShore) || (spawn.RequireMagicBait && !usingMagicBait))
                {
                    continue;
                }
                float chance = spawn.GetChance(hasCuriosityLure, player.DailyLuck, (float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location));
                if (!Game1.random.NextBool(chance) || (spawn.Condition != null && !GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, null, ignoreQueryKeys)))
                {
                    continue;
                }
                Item item = ItemQueryResolver.TryResolveRandomItem(spawn, itemQueryContext, avoidRepeat: false, null, (string query) => query.Replace("BOBBER_X", ((int)bobberTile.X).ToString()).Replace("BOBBER_Y", ((int)bobberTile.Y).ToString()).Replace("WATER_DEPTH", waterDepth.ToString()), null, delegate (string query, string error)
                {
                    //IGameLogger log2 = Game1.log;
                    //DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(54, 4);
                    //defaultInterpolatedStringHandler2.AppendLiteral("Location '");
                    //defaultInterpolatedStringHandler2.AppendFormatted(location.NameOrUniqueName);
                    //defaultInterpolatedStringHandler2.AppendLiteral("' failed parsing item query '");
                    //defaultInterpolatedStringHandler2.AppendFormatted(query);
                    //defaultInterpolatedStringHandler2.AppendLiteral("' for fish '");
                    //defaultInterpolatedStringHandler2.AppendFormatted(spawn.Id);
                    //defaultInterpolatedStringHandler2.AppendLiteral("': ");
                    //defaultInterpolatedStringHandler2.AppendFormatted(error);
                    //log2.Error(defaultInterpolatedStringHandler2.ToStringAndClear());
                });
                if (item == null)
                {
                    continue;
                }
                SDObject obj = item as SDObject;
                if (obj == null)
                {
                    //IGameLogger log = Game1.log;
                    //DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 2);
                    //defaultInterpolatedStringHandler.AppendLiteral("Skipped fish ID '");
                    //defaultInterpolatedStringHandler.AppendFormatted(item.QualifiedItemId);
                    //defaultInterpolatedStringHandler.AppendLiteral("' produced by the ");
                    //defaultInterpolatedStringHandler.AppendFormatted(spawn.Id);
                    //defaultInterpolatedStringHandler.AppendLiteral(" fish spawn rule: must return an object-type item.");
                    //log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(spawn.SetFlagOnCatch))
                {
                    item.SetFlagOnPickup = spawn.SetFlagOnCatch;
                }
                if (spawn.IsBossFish)
                {
                    item.SetTempData("IsBossFish", value: true);
                }
                SDObject fish = obj;
                if ((spawn.CatchLimit > -1 && player.fishCaught.TryGetValue(fish.QualifiedItemId, out var values) && values[0] >= spawn.CatchLimit) || !CheckGenericFishRequirements(fish, allFishData, location, player, spawn, waterDepth, usingMagicBait, hasCuriosityLure, isTutorialCatch))
                {
                    continue;
                }
                return fish;
            }
            if (!isTutorialCatch)
            {
                return null;
            }
            return ItemRegistry.Create("(O)145");
        }
        internal static bool CheckGenericFishRequirements(Item fish, Dictionary<string, string> allFishData, GameLocation location, Farmer player, SpawnFishData spawn, int waterDepth, bool usingMagicBait, bool hasCuriosityLure, bool isTutorialCatch)
        {
            if (!fish.HasTypeObject() || !allFishData.TryGetValue(fish.ItemId, out var rawSpecificFishData))
            {
                return !isTutorialCatch;
            }
            string[] specificFishData = rawSpecificFishData.Split('/');
            if (ArgUtility.Get(specificFishData, 1) == "trap")
            {
                return !isTutorialCatch;
            }
            bool isTrainingRod = player?.CurrentTool?.QualifiedItemId == "(T)TrainingRod";
            if (isTrainingRod)
            {
                if (!ArgUtility.TryGetInt(specificFishData, 1, out var difficulty, out var error7))
                {
                    return LogFormatError(error7);
                }
                if (difficulty >= 50)
                {
                    return false;
                }
            }
            if (isTutorialCatch)
            {
                if (!ArgUtility.TryGetOptionalBool(specificFishData, 14, out var isTutorialFish, out var error6))
                {
                    return LogFormatError(error6);
                }
                if (!isTutorialFish)
                {
                    return false;
                }
            }
            if (!spawn.IgnoreFishDataRequirements)
            {
                if (!usingMagicBait)
                {
                    if (!ArgUtility.TryGet(specificFishData, 5, out var rawTimeSpans, out var error5))
                    {
                        return LogFormatError(error5);
                    }
                    string[] timeSpans = ArgUtility.SplitBySpace(rawTimeSpans);
                    bool found = false;
                    for (int i = 0; i < timeSpans.Length; i += 2)
                    {
                        if (!ArgUtility.TryGetInt(timeSpans, i, out var startTime, out error5) || !ArgUtility.TryGetInt(timeSpans, i + 1, out var endTime, out error5))
                        {
                            return LogFormatError("invalid time spans '" + rawTimeSpans + "': " + error5);
                        }
                        if (Game1.timeOfDay >= startTime && Game1.timeOfDay < endTime)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        return false;
                    }
                }
                if (!usingMagicBait)
                {
                    if (!ArgUtility.TryGet(specificFishData, 7, out var weather, out var error4))
                    {
                        return LogFormatError(error4);
                    }
                    if (!(weather == "rainy"))
                    {
                        if (weather == "sunny" && location.IsRainingHere())
                        {
                            return false;
                        }
                    }
                    else if (!location.IsRainingHere())
                    {
                        return false;
                    }
                }
                if (!ArgUtility.TryGetInt(specificFishData, 12, out var minFishingLevel, out var error3))
                {
                    return LogFormatError(error3);
                }
                if (player.FishingLevel < minFishingLevel)
                {
                    return false;
                }
                if (!ArgUtility.TryGetInt(specificFishData, 9, out var maxDepth, out var error2) || !ArgUtility.TryGetFloat(specificFishData, 10, out var chance, out error2) || !ArgUtility.TryGetFloat(specificFishData, 11, out var depthMultiplier, out error2))
                {
                    return LogFormatError(error2);
                }
                float dropOffAmount = depthMultiplier * chance;
                chance -= (float)Math.Max(0, maxDepth - waterDepth) * dropOffAmount;
                chance += (float)player.FishingLevel / 50f;
                if (isTrainingRod)
                {
                    chance *= 1.1f;
                }
                chance = Math.Min(chance, 0.9f);
                if ((double)chance < 0.25 && hasCuriosityLure)
                {
                    if (spawn.CuriosityLureBuff > -1f)
                    {
                        chance += spawn.CuriosityLureBuff;
                    }
                    else
                    {
                        float max = 0.25f;
                        float min = 0.08f;
                        chance = (max - min) / max * chance + (max - min) / 2f;
                    }
                }
                if (spawn.ApplyDailyLuck)
                {
                    chance += (float)player.DailyLuck;
                }
                List<QuantityModifier> chanceModifiers = spawn.ChanceModifiers;
                if (chanceModifiers != null && chanceModifiers.Count > 0)
                {
                    chance = Utility.ApplyQuantityModifiers(chance, spawn.ChanceModifiers, spawn.ChanceModifierMode, location);
                }
                if (!Game1.random.NextBool(chance))
                {
                    return false;
                }
            }
            return true;
            bool LogFormatError(string error)
            {
                //Game1.log.Warn("Skipped fish '" + fish.ItemId + "' due to invalid requirements in Data/Fish: " + error);
                return false;
            }
        }
#endif
        public void FixBuildingAnimals(Building building)
        {

            if (building.indoors?.Value == null || building.indoors.Value is not AnimalHouse)
            {
                return;
            }

            foreach (long item in (building.indoors.Value as AnimalHouse).animalsThatLiveHere)
            {
                FarmAnimal animal = getAnimal(item);
                if (animal != null)
                {
                    animal.home = building;

                    animal.reload(building);
                    animal.setTileLocation(new Vector2(building.tileX.Value, building.tileY.Value));
                    if (!animal.IsHome)
                    {
                        animal.warpHome();
                    }

                }
            }

        }
        public FarmAnimal getAnimal(long id)
        {
            if (animals.ContainsKey(id))
            {
                return animals[id];
            }

            foreach (Building building in buildings)
            {
                if (building.indoors.Value is AnimalHouse && (building?.indoors.Value as AnimalHouse).animals.ContainsKey(id))
                {
                    return (building?.indoors.Value as AnimalHouse).animals[id];
                }
            }

            return null;
        }
        //private void AddGreenhouseFromSave(GreenhouseLocation greenhouse, Building building, string locationName)
        //{
        //    //
        //    //  Re-Add a GreenhouseLocation from save data
        //    //
        //    logger.Log($"Adding {building.buildingType.Value} [{locationName}] ({building.tileX}, {building.tileY})", LogLevel.Debug);

        //    if (building.indoors?.Value != null)
        //    {
        //        //greenhouse.objects.ReplaceWith(building.indoors.Value.objects);
        //        //greenhouse.uniqueName.Value = building.indoors.Value.uniqueName.Value;
        //        //greenhouse.terrainFeatures.ReplaceWith(building.indoors.Value.terrainFeatures);
        //        //greenhouse.largeTerrainFeatures.ReplaceWith(building.indoors.Value.largeTerrainFeatures);

        //        //greenhouse.modData = building.indoors.Value.modData;


        //        greenhouse.IsGreenhouse = true;
        //        building.indoors.Value = greenhouse;
        //        //
        //        //  re-add interior to GameLocations
        //        //
        //        //FEFramework.AddGameLocation(building.indoors.Value, "AddGreenhouseFromSave");
        //        ExpansionManager.FixBuildingWarps(building, locationName);

        //        SetupCustomBuilding(building, false);
        //    }
        //    if (!building.modData.ContainsKey(FEModDataKeys.FEGreenhouse))
        //        building.modData.Add(FEModDataKeys.FEGreenhouse, "Y");

        //}
        //private static void SetupCustomBuilding(Building building, bool reloadBaseBuilding)
        //{

        //    try
        //    {
        //        if (CustomBuildingManager.buildings.ContainsKey(building.buildingType.Value))
        //        {
        //            if (building.indoors?.Value != null)
        //            {
        //                foreach (KeyValuePair<Vector2, SDObject> pair2 in building.indoors.Value.objects.Pairs)
        //                {
        //                    pair2.Value.initializeLightSource(pair2.Key);
        //                    pair2.Value.reloadSprite();
        //                }
        //                BuildingData bluePrint = Game1.buildingData[building.buildingType.Value];

        //                if (bluePrint != null)
        //                {
        //                    building.humanDoor.X = bluePrint.HumanDoor.X;
        //                    building.humanDoor.Y = bluePrint.HumanDoor.Y;


        //                    //building.additionalPlacementTiles.Clear();                           
        //                    //building.additionalPlacementTiles.AddRange(bluePrint.additionalPlacementTiles);
        //                }
        //            }
        //        }
        //        else if (reloadBaseBuilding)
        //        {
        //            building.load();
        //        }
        //    }
        //    catch { }
        //}

        //private void AddBreadFactoryFromSave(BreadFactoryLocation breadf, Building building, string locationName)
        //{
        //    logger?.Log($"Adding {building.buildingType.Value} [{locationName}] ({building.tileX}, {building.tileY})", LogLevel.Debug);

        //    if (building.indoors?.Value != null)
        //    {
        //        //greenhouse.objects.ReplaceWith(building.indoors.Value.objects);
        //        //greenhouse.uniqueName.Value = building.indoors.Value.uniqueName.Value;

        //        breadf.modData.Clear();
        //        breadf.modData.AddMyRange(building.indoors.Value.modData);

        //        breadf.IsGreenhouse = false;
        //        building.indoors.Value = breadf;
        //        //FEFramework.AddGameLocation(building.indoors.Value, "AddLargeGreenhouseFromSave");
        //        ExpansionManager.FixBuildingWarps(building, locationName);

        //        SetupCustomBuilding(building, false);
        //    }

        //}
        //private void AddLargeGreenhouseFromSave(LargeGreenhouseLocation greenhouse, Building building, string locationName)
        //{
        //    //
        //    //  Re-Add a LargeGreenhouseLocation from save data
        //    //
        //    logger?.Log($"Adding {building.buildingType.Value} [{locationName}] ({building.tileX}, {building.tileY})", LogLevel.Debug);

        //    if (building.indoors?.Value != null)
        //    {
        //        //greenhouse.objects.ReplaceWith(building.indoors.Value.objects);
        //        //greenhouse.uniqueName.Value = building.indoors.Value.uniqueName.Value;

        //        greenhouse.modData.Clear();
        //        greenhouse.modData.AddMyRange(building.indoors.Value.modData);

        //        greenhouse.IsGreenhouse = true;
        //        building.indoors.Value = greenhouse;
        //        //FEFramework.AddGameLocation(building.indoors.Value, "AddLargeGreenhouseFromSave");
        //        ExpansionManager.FixBuildingWarps(building, locationName);

        //        SetupCustomBuilding(building, false);
        //    }
        //    if (!building.modData.ContainsKey(FEModDataKeys.FELargeGreenhouse))
        //        building.modData.Add(FEModDataKeys.FELargeGreenhouse, "Y");

        //}

        //private void AddWineryFromSave(Cellar winery, Building building, string locationName)
        //{
        //    //
        //    //  Re-Add a Winery from save data
        //    //
        //    logger?.Log($"Adding {building.buildingType.Value} [{locationName}] ({building.tileX}, {building.tileY})", LogLevel.Debug);

        //    if (building.indoors?.Value != null)
        //    {
        //        //winery.objects.ReplaceWith(building.indoors.Value.objects);
        //        winery.uniqueName.Value = building.indoors.Value.uniqueName.Value;

        //        winery.modData.Clear();
        //        winery.modData.AddMyRange(building.indoors.Value.modData);

        //        building.indoors.Value = winery;
        //        //  loading winery in single player cause double location loading
        //        //AddGameLocation(building.indoors.Value);
        //        ExpansionManager.FixBuildingWarps(building, locationName);
        //        SetupCustomBuilding(building, false);

        //        if (!building.modData.ContainsKey(FEModDataKeys.FEWinery))
        //            building.modData.Add(FEModDataKeys.FEWinery, "Y");
        //    }
        //}
        //private void AddFromagerieFromSave(FromagerieLocation fromagerie, Building building, string locationName)
        //{
        //    //
        //    //  Re-Add a FromagerieLocation from save data
        //    //
        //    logger.Log($"Adding {building.buildingType.Value} [{locationName}] ({building.tileX}, {building.tileY})", LogLevel.Debug);

        //    if (building.indoors?.Value != null)
        //    {
        //        //fromagerie.objects.ReplaceWith(building.indoors.Value.objects);
        //        fromagerie.uniqueName.Value = building.indoors.Value.uniqueName.Value;

        //        fromagerie.modData.Clear();
        //        fromagerie.modData.AddMyRange(building.indoors.Value.modData);

        //        fromagerie.cheeseVatList = ((FromagerieLocation)building.indoors.Value).cheeseVatList;
        //        fromagerie.SetMachineState(0, fromagerie.cheeseVatList[0].Running);
        //        fromagerie.SetMachineState(1, fromagerie.cheeseVatList[1].Running);
        //        fromagerie.SetMachineState(2, fromagerie.cheeseVatList[2].Running);

        //        building.indoors.Value = fromagerie;
        //        //FEFramework.AddGameLocation(building.indoors.Value, "AddFromagerieFromSave");
        //        ExpansionManager.FixBuildingWarps(building, locationName);

        //        SetupCustomBuilding(building, false);
        //    }
        //    if (!building.modData.ContainsKey(FEModDataKeys.FEFromagerie))
        //        building.modData.Add(FEModDataKeys.FEFromagerie, "Y");

        //}

        public void LoadBuildings(bool addToBaseFarm)
        {
            foreach (Building building in buildings)
            {
                //if load is not called, building interior map
                //is not loaded
                building.load();
                expansionManager.FixBuildingWarps(building, Name);
                //
                // 2024-01-10 should not be needed
                //
                //FixBuildingAnimals(building);

                if (building.indoors.Value != null)
                {
                    //
                    //  2024-01-10 fix to move seasonOverride to animal house indoors
                    //
                    //if (!string.IsNullOrEmpty(SeasonOverride))
                    //{
                    //    if (!building.indoors.Value.map.Properties.ContainsKey("SeasonOverride"))
                    //    {
                    //        building.indoors.Value.map.Properties.Add("SeasonOverride", SeasonOverride);
                    //    }
                    //}
                }
            }
            return;
            //logger?.Log($"     Loading ExpansionBuildings for {Name}, {buildings.Count()} buildings.", LogLevel.Debug);
            //foreach (Building building in buildings)
            //{

            //    Vector2 tile = new Vector2((float)building.tileX.Value, (float)building.tileY.Value);

            //    if (building.indoors?.Value is Shed)
            //    {
            //        building.load();
            //        (building.indoors.Value as Shed).furniture.ReplaceWith((getBuildingAt(tile).indoors.Value as Shed).furniture);
            //        (building.indoors.Value as Shed).wallPaper.Set((getBuildingAt(tile).indoors.Value as Shed).wallPaper);
            //        (building.indoors.Value as Shed).floor.Set((getBuildingAt(tile).indoors.Value as Shed).floor);
            //        ExpansionManager.FixBuildingWarps(building, Name);
            //    }
            //    else if (building.indoors?.Value is SlimeHutch hutch)
            //    {
            //        building.load();
            //        for (int i = hutch.characters.Count - 1; i >= 0; i--)
            //        {
            //            if (!hutch.characters[i].DefaultPosition.Equals(Vector2.Zero))
            //                hutch.characters[i].position.Value = hutch.characters[i].DefaultPosition;

            //            hutch.characters[i].currentLocation = hutch;

            //            if (i < hutch.characters.Count)
            //                hutch.characters[i].reloadSprite();
            //        }
            //        ExpansionManager.FixBuildingWarps(building, Name);
            //    }
            //    else if (building is JunimoHut jh)
            //    {
            //        jh.load();
            //        if ((int)Traverse.Create(jh).Field("junimoSendOutTimer").GetValue() == 0)
            //        {
            //            Traverse.Create(jh).Field("junimoSendOutTimer").SetValue(1000);
            //        }
            //        jh.shouldSendOutJunimos.Value = true;
            //    }
            //    else if (building.buildingType.Value == "Winery")
            //    {
            //        building.buildingType.Value = "fe.winery";
            //        AddWineryFromSave((Cellar)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, Name), building, Name);
            //    }
            //    else if (building.buildingType.Value == "fe.winery")
            //    {
            //        AddWineryFromSave((Cellar)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, Name), building, Name);
            //    }
            //    else if (building.buildingType.Value == "Fromagerie")
            //    {
            //        building.buildingType.Value = "fe.fromagerie";
            //        AddFromagerieFromSave((FromagerieLocation)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, Name), building, Name);
            //    }
            //    else if (building.buildingType.Value == "fe.fromagerie")
            //    {
            //        AddFromagerieFromSave((FromagerieLocation)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, Name), building, Name);
            //    }
            //    else if (building.buildingType.Value == "fe.greenhouse")
            //    {
            //        AddGreenhouseFromSave((GreenhouseLocation)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, Name), building, Name);
            //    }
            //    else if (building.buildingType.Value == "fe.largegreenhouse")
            //    {
            //        AddLargeGreenhouseFromSave((LargeGreenhouseLocation)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, Name), building, Name);
            //    }
            //    else if (building.buildingType.Value == "fe.breadfactory")
            //    {
            //        AddBreadFactoryFromSave((BreadFactoryLocation)CustomBuildingManager.buildings[building.buildingType.Value].GetIndoorLocation(building, Name), building, Name);
            //    }
            //    else
            //    {
            //        building.load();
            //        ExpansionManager.FixBuildingWarps(building, Name);
            //    }
            //    //
            //    //  if the buidling is on the Base farm, re-add it
            //    //  to the building list
            //    //
            //    if (addToBaseFarm)
            //    {
            //        Game1.getFarm().buildings.Add(building);
            //    }

            //    prism_Building pBuilding = new prism_Building(building);

            //    logger?.Log($"Checking building {pBuilding.NameOfIndoors ?? building.buildingType.Value}", LogLevel.Debug);
            //    logger?.Log($"Building Type {building.buildingType.Value ?? "Unknown"}", LogLevel.Debug);

            //    if (!building.modData.ContainsKey(FEModDataKeys.FELocationName))
            //    {
            //        building.modData.Add(FEModDataKeys.FELocationName, Name);
            //    }

            //    //logger?.Log($"Loading Building: {building.nameOfIndoorsWithoutUnique}", LogLevel.Debug);

            //    //SetupCustomBuilding(building, true);

            //    if (building.indoors?.Value != null && building.indoors.Value is AnimalHouse oHouse)
            //    {
            //        if (!oHouse.modData.ContainsKey(FEModDataKeys.FELocationName))
            //        {
            //            oHouse.modData.Add(FEModDataKeys.FELocationName, Name);
            //        }
            //        //
            //        // do animal check
            //        //

            //        logger?.Log($"animalsthatlivehere count={oHouse.animalsThatLiveHere.Count}", LogLevel.Debug);

            //        if (building.indoors?.Value != null && building.indoors.Value is AnimalHouse oAnimalHouse)
            //        {
            //            List<long> delIds = new List<long>();
            //            foreach (long iD in oAnimalHouse.animalsThatLiveHere)
            //            {
            //                if (!oAnimalHouse.animals.ContainsKey(iD))
            //                {
            //                    if (animals != null && animals.ContainsKey(iD))
            //                    {
            //                        oAnimalHouse.animals.Add(iD, animals[iD]);
            //                        animals.Remove(iD);
            //                    }
            //                }
            //                if (oAnimalHouse.animals.ContainsKey(iD))
            //                {
            //                    try
            //                    {
            //                        var p = oAnimalHouse.animals[iD].GetBoundingBox();
            //                    }
            //                    catch
            //                    {
            //                        logger.Log($"SM.LoadexpansionBuildings Removing invalid Animal {iD}:{oAnimalHouse.animals[iD].type.Value}", LogLevel.Error);
            //                        delIds.Add(iD);
            //                    }
            //                }
            //                else
            //                {
            //                    delIds.Add(iD);
            //                }

            //            }
            //            foreach (long iD in delIds)
            //            {
            //                logger.Log($"Removing Animal {iD}:", LogLevel.Debug);
            //                oAnimalHouse.animalsThatLiveHere.Remove(iD);
            //                building.currentOccupants.Value--;
            //            }
            //        }

            //        logger?.Log($"checking AnimalHouse {oHouse.Name}", LogLevel.Debug);
            //        logger?.Log($"animal count: {oHouse.animals.Count()}", LogLevel.Debug);
            //    }

            //    if (!addToBaseFarm)
            //    {
            //        FixAnimals();
            //    }
            //}
        }

    }
}
