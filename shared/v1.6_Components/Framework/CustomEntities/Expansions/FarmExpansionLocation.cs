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
using xTile.Dimensions;
using System;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using xTile.Layers;
using SDV_Realty_Core.Framework.CustomEntities;
using Netcode;
using static StardewValley.Farm;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Integrations;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using HarmonyLib;
using SDV_Realty_Core.Framework.Objects;

namespace SDV_Realty_Core.Framework.Expansions
{
    [XmlInclude(typeof(FromagerieLocation))]
    [XmlInclude(typeof(GreenhouseLocation))]
    [XmlInclude(typeof(GreenhouseInterior))]
    [XmlInclude(typeof(LargeGreenhouseLocation))]
    [XmlInclude(typeof(LargeGreenhouseInterior))]
    public class FarmExpansionLocation : GameLocation, ICloneable
    {
        //
        //  version 1.6
        //
        private static SDVLogger logger;
        public bool forceLightning = false;
        private ExpansionDetails expansionDetails;
        private IModDataService modDataService;
        //private ExpansionManager expansionManager;
        internal IUtilitiesService utilitiesService;
        private ILocationTunerIntegrationService locationTunerIntegrationService;
        private readonly NetEvent1Field<Vector2, NetVector2> spawnCrowEvent = new NetEvent1Field<Vector2, NetVector2>();
        public readonly NetEvent1<LightningStrikeEvent> lightningStrikeEvent = new NetEvent1<LightningStrikeEvent>();
        //private Texture2D BorderTexture;
        public int CropsDestroyedByCrows = 0;
        [XmlIgnore]
        public readonly NetRef<StardewMeadowsTrain> train = new NetRef<StardewMeadowsTrain>();
        [XmlIgnore]
        public readonly NetInt trainTimer = new NetInt(0);
        public readonly NetBool hasTrainPassed = new NetBool(value: false);
        [XmlIgnore]
        public ICue? trainLoop = null;

        public FarmExpansionLocation() : base()
        {
            //BorderTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            //BorderTexture.SetData(new Color[] { Color.White });
        }
        //
        //  cannot use a game loader always appends .xnb to filename
        //
        public FarmExpansionLocation(string mapPath, string a) : base(mapPath, a)
        {
            //logger = olog;
            //this.Map = omap;
            //this.name.Value = name;
            AutoAdd = true;
            GridId = -1;
        }
        internal FarmExpansionLocation(Map omap, string mapPathValue, string locationName, SDVLogger olog, IUtilitiesService utilitiesService, IModDataService modDataService, ILocationTunerIntegrationService? locationTunerIntegrationService) : this()
        {
            logger = olog;
            Map = omap;
            mapPath.Value = mapPathValue;
            name.Value = locationName;
            AutoAdd = true;
            GridId = -1;
            this.utilitiesService = utilitiesService;
            this.modDataService = modDataService;
            this.locationTunerIntegrationService = locationTunerIntegrationService;
            modData[IModDataKeysService.FEExpansionType] = "Expansion";
            modData[IModDataKeysService.FELocationName] = Name;
            isAlwaysActive.Value = true;
        }
        internal FarmExpansionLocation(Map map, string mapPathValue, string locationName, SDVLogger olog, ExpansionDetails expansionDetails, IUtilitiesService utilitiesService, IModDataService modDataService, ILocationTunerIntegrationService locationTunerIntegrationService) : this(map, mapPathValue, locationName, olog, utilitiesService, modDataService, locationTunerIntegrationService)
        {
            this.expansionDetails = expansionDetails;
            ReloadExpansionDetails();
        }
        internal void ReloadExpansionDetails()
        {
            if (expansionDetails != null)
            {
                if (modDataService.CustomDefinitions.ContainsKey(Name))
                {
                    expansionDetails.Update(modDataService.CustomDefinitions[Name]);
                }
                if (expansionDetails.ShippingBin.HasValue)
                {
                    Vector2 binTile = new Vector2(expansionDetails.ShippingBin.Value.X, expansionDetails.ShippingBin.Value.Y);
                    if (getBuildingAt(binTile) == null)
                    {
                        Building building = Building.CreateInstanceFromId("Shipping Bin", binTile);
                        building.load();
                        buildings.Add(building);
                    }
                }
                if (expansionDetails?.AlwaysSnowing ?? false)
                    locationContextId = "prism99.advize.stardewrealty.always_snow";
                else if (expansionDetails?.AlwaysRaining ?? false)
                    locationContextId = "prism99.advize.stardewrealty.always_rain";
                else if (expansionDetails?.AlwaysSunny ?? false)
                    locationContextId = "prism99.advize.stardewrealty.always_sunny";
                else
                    locationContextId = null;
            }
        }
        //public void DrawLine(SpriteBatch b, Vector2 start, Vector2 end, Color color)
        //{
        //    float length = (end - start).Length();
        //    float rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
        //    b.Draw(BorderTexture, start, null, color, rotation, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
        //}
        //public void DrawRectangle(SpriteBatch b, Rectangle rectangle, Color color)
        //{
        //    b.Draw(BorderTexture, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 2),new Rectangle(0,0,1,1), color,0,Vector2.Zero,SpriteEffects.None,0.99f);
        //    b.Draw(BorderTexture, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, 2), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
        //    b.Draw(BorderTexture, new Rectangle(rectangle.Left, rectangle.Top, 2, rectangle.Height), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
        //    b.Draw(BorderTexture, new Rectangle(rectangle.Right, rectangle.Top, 2, rectangle.Height + 1), new Rectangle(0, 0, 1, 1), color, 0, Vector2.Zero, SpriteEffects.None, 0.99f);
        //}
        [XmlIgnoreAttribute]
        public Dictionary<string, FishAreaDetails> FishAreas;
        public bool Active { get; set; }
        public bool Loaded { get; set; }
        public string baseExpansionName = "";
        public bool BaseBuildingsAdded { get; set; }
        public long PurchasedBy { get; set; }
        public int GridId { get; set; } = -1;
        public bool AutoAdd { get; set; }
        public bool OfferLetterRead { get; set; }
        public string SeasonOverride { get; set; }
        public List<SuspensionBridge> suspensionBridges = new List<SuspensionBridge>();

        public bool StockedPonds { get; set; }
        public bool CrowsEnabled { get; set; }
        public bool AllowBlueGrass { get; set; }

        public override bool performAction(string fullActionString, Farmer who, Location tileLocation)
        {
            if (fullActionString.StartsWith("CentralStation") && !utilitiesService.ModHelperService.ModRegistry.IsLoaded("Pathoschild.CentralStation"))
            {
                utilitiesService.PopMessage("CentralStation is required enable transportation functionality");
                return true;
            }
            else
            {
                return base.performAction(fullActionString, who, tileLocation);
            }
        }
        public string GetSeasonOverride()
        {
            return Traverse.Create(this).Field("seasonOverride").Property("Value").GetValue()?.ToString() ?? "";
        }
        /// <summary>
        /// Overriden to add support for BlueGrass in expansions
        /// </summary>
        /// <param name="dayOfMonth"></param>
        public override void HandleGrassGrowth(int dayOfMonth)
        {

            base.HandleGrassGrowth(dayOfMonth);
            //  add bluegrass to expansions
            if (AllowBlueGrass && dayOfMonth == 1 && HasMapPropertyWithValue("SpawnRandomGrassOnNewYear") && Game1.IsSpring && Game1.stats.DaysPlayed > 1)
            {
                for (int i = 0; i < 15; i++)
                {
                    int xCoord = Game1.random.Next(map.DisplayWidth / 64);
                    int yCoord = Game1.random.Next(map.DisplayHeight / 64);
                    Vector2 location2 = new Vector2(xCoord, yCoord);
                    objects.TryGetValue(location2, out var o2);
                    if (o2 == null && doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null && !IsNoSpawnTile(location2) && isTileLocationOpen(new Location(xCoord, yCoord)) && !IsTileOccupiedBy(location2) && !isWaterTile(xCoord, yCoord))
                    {
                        if (Game1.random.NextDouble() < 0.2)
                        {
                            terrainFeatures.Add(location2, new Grass(7, 4));
                        }
                    }
                }
            }
        }
        //public override void seasonUpdate(bool onLoad = false)
        //{
        //    Season season = GetSeason();
        //    terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> pair) => pair.Value.seasonUpdate(onLoad));
        //    largeTerrainFeatures?.RemoveWhere((LargeTerrainFeature feature) => feature.seasonUpdate(onLoad));
        //    foreach (NPC character in characters)
        //    {
        //        if (!character.IsMonster)
        //        {
        //            character.resetSeasonalDialogue();
        //        }
        //    }

        //    if (IsOutdoors && !onLoad)
        //    {
        //        KeyValuePair<Vector2, SDObject>[] array = objects.Pairs.ToArray();
        //        for (int i = 0; i < array.Length; i++)
        //        {
        //            KeyValuePair<Vector2, SDObject> keyValuePair = array[i];
        //            Vector2 key = keyValuePair.Key;
        //            SDObject value = keyValuePair.Value;
        //            if (value.IsSpawnedObject && !value.IsBreakableStone())
        //            {
        //                objects.Remove(key);
        //            }
        //            else if (value.QualifiedItemId == "(O)590" && doesTileHavePropertyNoNull((int)key.X, (int)key.Y, "Diggable", "Back") == "")
        //            {
        //                objects.Remove(key);
        //            }
        //        }

        //        numberOfSpawnedObjectsOnMap = 0;
        //    }

        //    switch (season)
        //    {
        //        case Season.Spring:
        //            waterColor.Value = new Color(120, 200, 255) * 0.5f;
        //            break;
        //        case Season.Summer:
        //            waterColor.Value = new Color(60, 240, 255) * 0.5f;
        //            break;
        //        case Season.Fall:
        //            waterColor.Value = new Color(255, 130, 200) * 0.5f;
        //            break;
        //        case Season.Winter:
        //            waterColor.Value = new Color(130, 80, 255) * 0.5f;
        //            break;
        //    }

        //    if (!onLoad && season == Season.Spring && Game1.stats.DaysPlayed > 1)
        //    {
        //        loadWeeds();
        //    }
        //}

        //private string GetLocContextId()
        //{
        //    if (locationContextId == null)
        //    {
        //        if (map == null)
        //        {
        //            reloadMap();
        //        }
        //        if (map != null && map.Properties.TryGetValue("LocationContext", out var contextId))
        //        {
        //            if (Game1.locationContextData.ContainsKey(contextId))
        //            {
        //                locationContextId = contextId;
        //            }
        //            else
        //            {
        //                logger.Log($"Could not find context: {contextId}", StardewModdingAPI.LogLevel.Debug);
        //            }
        //        }
        //        if (locationContextId == null)
        //        {
        //            locationContextId = GetParentLocation()?.GetLocationContextId() ?? "Default";
        //        }
        //    }
        //    return locationContextId;
        //}
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
        public override void performTenMinuteUpdate(int timeOfDay)
        {
            base.performTenMinuteUpdate(timeOfDay);
            if (Game1.IsMasterGame)
            {
#if DEBUG
                bool monstersEnabled = modDataService?.Config.enableNightMonsters ?? false;
#else
             bool monstersEnabled = modDataService?.Config.enableNightMonsters??false && Game1.spawnMonstersAtNight;
#endif

                if (monstersEnabled && timeOfDay >= 1900 && !(Game1.random.NextDouble() > 0.25 - Game1.player.team.AverageDailyLuck() / 2.0))
                {
                    if (Game1.random.NextDouble() < 0.25)
                    {
                        if (Game1.currentLocation.Name == Name)
                        {
                            spawnFlyingMonstersOffScreen();
                        }
                    }
                    else
                    {
                        spawnGroundMonsterOffScreen();
                    }
                }
            }
        }
        public void spawnGroundMonsterOffScreen()
        {
#if DEBUG
            logger.Log($"spawnGroundMonsterOffScreen for {Name}", LogLevel.Debug);
#endif        
            for (int i = 0; i < 15; i++)
            {
                Vector2 spawnLocation = getRandomTile();
                if (Utility.isOnScreen(Utility.Vector2ToPoint(spawnLocation), 64, this))
                {
                    spawnLocation.X -= Game1.viewport.Width / 64;
                }
                if (!CanItemBePlacedHere(spawnLocation))
                {
                    continue;
                }
                int combatLevel = Game1.player.CombatLevel;
                bool success;
                if (combatLevel >= 8 && Game1.random.NextDouble() < 0.15)
                {
                    characters.Add(new ShadowBrute(spawnLocation * 64f)
                    {
                        focusedOnFarmers = true,
                        wildernessFarmMonster = true
                    });
                    success = true;
                }
                else if (Game1.random.NextDouble() < ((Game1.whichFarm == 4) ? 0.66 : 0.33))
                {
                    characters.Add(new RockGolem(spawnLocation * 64f, combatLevel)
                    {
                        wildernessFarmMonster = true
                    });
                    success = true;
                }
                else
                {
                    int virtualMineLevel = 1;
                    if (combatLevel >= 10)
                    {
                        virtualMineLevel = 140;
                    }
                    else if (combatLevel >= 8)
                    {
                        virtualMineLevel = 100;
                    }
                    else if (combatLevel >= 4)
                    {
                        virtualMineLevel = 41;
                    }
                    characters.Add(new GreenSlime(spawnLocation * 64f, virtualMineLevel)
                    {
                        wildernessFarmMonster = true
                    });
                    success = true;
                }
                if (!success || Game1.currentLocation.Name != Name)
                {
                    break;
                }
                {
                    foreach (KeyValuePair<Vector2, SDObject> v in objects.Pairs)
                    {
                        if (v.Value?.QualifiedItemId == "(BC)83")
                        {
                            v.Value.shakeTimer = 1000;
                            v.Value.showNextIndex.Value = true;
                            Game1.currentLightSources.Add(new LightSource(v.Value.GenerateLightSourceId(v.Value.TileLocation), 4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
                        }
                    }
                    break;
                }
            }
        }


        public void spawnFlyingMonstersOffScreen()
        {
#if DEBUG
            logger.Log($"spawnFlyingMonstersOffScreen for {Name}", LogLevel.Debug);
#endif
            Vector2 spawnLocation = Vector2.Zero;
            switch (Game1.random.Next(4))
            {
                case 0:
                    spawnLocation.X = Game1.random.Next(map.Layers[0].LayerWidth);
                    break;
                case 3:
                    spawnLocation.Y = Game1.random.Next(map.Layers[0].LayerHeight);
                    break;
                case 1:
                    spawnLocation.X = map.Layers[0].LayerWidth - 1;
                    spawnLocation.Y = Game1.random.Next(map.Layers[0].LayerHeight);
                    break;
                case 2:
                    spawnLocation.Y = map.Layers[0].LayerHeight - 1;
                    spawnLocation.X = Game1.random.Next(map.Layers[0].LayerWidth);
                    break;
            }
            if (Utility.isOnScreen(spawnLocation * 64f, 64))
            {
                spawnLocation.X -= Game1.viewport.Width;
            }
            int combatLevel = Game1.player.CombatLevel;
            bool success;
            if (combatLevel >= 10 && Game1.random.NextDouble() < 0.01 && Game1.player.Items.ContainsId("(W)4"))
            {
                characters.Add(new Bat(spawnLocation * 64f, 9999)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = true
                });
                success = true;
            }
            else if (combatLevel >= 10 && Game1.random.NextDouble() < 0.25)
            {
                characters.Add(new Bat(spawnLocation * 64f, 172)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = true
                });
                success = true;
            }
            else if (combatLevel >= 10 && Game1.random.NextDouble() < 0.25)
            {
                characters.Add(new Serpent(spawnLocation * 64f)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = true
                });
                success = true;
            }
            else if (combatLevel >= 8 && Game1.random.NextBool())
            {
                characters.Add(new Bat(spawnLocation * 64f, 81)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = true
                });
                success = true;
            }
            else if (combatLevel >= 5 && Game1.random.NextBool())
            {
                characters.Add(new Bat(spawnLocation * 64f, 41)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = true
                });
                success = true;
            }
            else
            {
                characters.Add(new Bat(spawnLocation * 64f, 1)
                {
                    focusedOnFarmers = true,
                    wildernessFarmMonster = true
                });
                success = true;
            }
            if (!success || Game1.currentLocation.Name != Name)
            {
                return;
            }
            foreach (KeyValuePair<Vector2, SDObject> v in objects.Pairs)
            {
                if (v.Value?.QualifiedItemId == "(BC)83")
                {
                    v.Value.shakeTimer = 1000;
                    v.Value.showNextIndex.Value = true;
                    Game1.currentLightSources.Add(new LightSource(v.Value.GenerateLightSourceId(v.Value.TileLocation), 4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
                }
            }
        }

        /// <summary>
        /// Override to support adding crows
        /// </summary>
        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(spawnCrowEvent, "spawnCrowEvent").AddField(lightningStrikeEvent, "lightningStrikeEvent");
            NetFields.AddField(train);

            lightningStrikeEvent.onEvent += doLightningStrike;
            spawnCrowEvent.onEvent += doSpawnCrow;
        }

        public new bool IsLightningHere()
        {
            if (forceLightning || !Game1.IsMasterGame)
            {
                forceLightning = false;
                return true;
            }
            //
            //  block lightning when location season is Winter
            if (GetSeason() == Season.Winter)
                return false;
            else
                return GetWeather().IsLightning;
        }
        private void doLightningStrike(LightningStrikeEvent lightning)
        {
            //
            //  
            if (Game1.IsMasterGame)
            {
                if (locationTunerIntegrationService.NoLightning(Name, out bool value))
                {
                    if (value) return;
                }
            }
            if (lightning.smallFlash)
            {
                if (Game1.currentLocation.IsOutdoors && !Game1.newDay && IsLightningHere())
                {
                    Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
                    if (Game1.random.NextBool())
                    {
                        DelayedAction.screenFlashAfterDelay((float)(0.3 + Game1.random.NextDouble()), Game1.random.Next(500, 1000));
                    }
                    DelayedAction.playSoundAfterDelay("thunder_small", Game1.random.Next(500, 1500));
                }
            }
            else if (lightning.bigFlash && Game1.currentLocation.IsOutdoors && IsLightningHere() && !Game1.newDay)
            {
                Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
                Game1.playSound("thunder");
            }
            if (lightning.createBolt)
            {
                if (lightning.destroyedTerrainFeature)
                {
                    temporarySprites.Add(new TemporaryAnimatedSprite(362, 75f, 6, 1, lightning.boltPosition, flicker: false, flipped: false));
                }
                Utility.drawLightningBolt(lightning.boltPosition, this);
            }
        }

        private void doSpawnCrow(Vector2 v)
        {
            if (critters == null && isOutdoors.Value)
            {
                critters = new List<Critter>();
            }
            critters.Add(new Crow((int)v.X, (int)v.Y));
        }
        public void addCrows()
        {
            if (locationTunerIntegrationService.NoCrows(Name, out bool value))
            {
                if (value) return;
            }
            int numCrops = 0;
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in terrainFeatures.Pairs)
            {
                if (pair.Value is HoeDirt { crop: not null })
                {
                    numCrops++;
                }
            }
            List<Vector2> scarecrowPositions = new List<Vector2>();
            foreach (KeyValuePair<Vector2, SDObject> v in objects.Pairs)
            {
                if (v.Value.IsScarecrow())
                {
                    scarecrowPositions.Add(v.Key);
                }
            }
            int potentialCrows = Math.Min(modDataService.Config.MaxNumberOfCrows, numCrops / 16);
            for (int i = 0; i < potentialCrows; i++)
            {
                if (!(Game1.random.NextDouble() < 0.3))
                {
                    continue;
                }
                for (int attempts = 0; attempts < 10; attempts++)
                {
                    if (!Utility.TryGetRandom(terrainFeatures, out var tile, out var feature) || !(feature is HoeDirt dirt))
                    {
                        continue;
                    }
                    Crop crop = dirt.crop;
                    if (crop == null || crop.currentPhase.Value <= 1)
                    {
                        continue;
                    }
                    bool scarecrow = false;
                    foreach (Vector2 s in scarecrowPositions)
                    {
                        int radius = objects[s].GetRadiusForScarecrow();
                        if (Vector2.Distance(s, tile) < (float)radius)
                        {
                            scarecrow = true;
                            objects[s].SpecialVariable++;
                            break;
                        }
                    }
                    if (!scarecrow)
                    {
                        CropsDestroyedByCrows++;
#if DEBUG                        
                        Game1.addHUDMessage(new HUDMessage($"Crop destroyed by Crow ({CropsDestroyedByCrows})") { noIcon = true, timeLeft = 1500 });
#endif
                        dirt.destroyCrop(showAnimation: false);
                        spawnCrowEvent.Fire(tile);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Override base event to update location's SuspensionBridges
        /// </summary>
        /// <param name="time"></param>
        public override void UpdateWhenCurrentLocation(GameTime time)
        {
            base.UpdateWhenCurrentLocation(time);
            foreach (SuspensionBridge suspensionBridge in suspensionBridges)
            {
                suspensionBridge.Update(time);
            }
        } /// <summary>
          /// Override base event to draw location's SuspensionBridges
          /// </summary>
          /// <param name="time"></param>
        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            foreach (SuspensionBridge suspensionBridge in suspensionBridges)
            {
                suspensionBridge.Draw(b);
            }
        }

        public void FixAnimals()
        {
            foreach (Building building in buildings)
            {
                FixBuildingAnimals(building);
            }
        }

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
        public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
        {
            spawnCrowEvent.Poll();
            lightningStrikeEvent.Poll();
            base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
        }
        public override void DayUpdate(int dayOfMonth)
        {
            //
            //  added override to stop crazy spawning of debris
            //
            CustomProductionManager.DayUpdate(this, dayOfMonth);

            if (modDataService?.validContents.TryGetValue(Name, out var content) ?? false)
            {
                if (content.CrowsEnabled)
                {
                    addCrows();
                }
            }
            isMusicTownMusic = null;
            netAudio.StopPlaying("fuse");
            SelectRandomMiniJukeboxTrack();
            critters?.Clear();
            for (int i = characters.Count - 1; i >= 0; i--)
            {
                NPC npc = characters[i];
                if (npc is JunimoHarvester)
                {
                    characters.RemoveAt(i);
                }
                else if (npc is Monster { wildernessFarmMonster: not false })
                {
                    characters.RemoveAt(i);
                }
            }
            FarmAnimal[] array = animals.Values.ToArray();
            for (int k = 0; k < array.Length; k++)
            {
                array[k].dayUpdate(this);
            }
            for (int j = debris.Count - 1; j >= 0; j--)
            {
                Debris d2 = debris[j];
                if (d2.isEssentialItem() && Game1.IsMasterGame)
                {
                    if (d2.item?.QualifiedItemId == "(O)73")
                    {
                        d2.collect(Game1.player);
                    }
                    else
                    {
                        Item item = d2.item;
                        d2.item = null;
                        Game1.player.team.returnedDonations.Add(item);
                        Game1.player.team.newLostAndFoundItems.Value = true;
                    }
                    debris.RemoveAt(j);
                }
            }
            updateMap();
            temporarySprites.Clear();
            KeyValuePair<Vector2, TerrainFeature>[] map_features = terrainFeatures.Pairs.ToArray();
            KeyValuePair<Vector2, TerrainFeature>[] array2 = map_features;
            for (int k = 0; k < array2.Length; k++)
            {
                KeyValuePair<Vector2, TerrainFeature> pair5 = array2[k];
                if (!isTileOnMap(pair5.Key))
                {
                    terrainFeatures.Remove(pair5.Key);
                }
                else
                {
                    pair5.Value.dayUpdate();
                }
            }
            array2 = map_features;
            foreach (KeyValuePair<Vector2, TerrainFeature> pair4 in array2)
            {
                if (pair4.Value is HoeDirt hoe_dirt)
                {
                    hoe_dirt.updateNeighbors();
                }
            }
            if (largeTerrainFeatures != null)
            {
                LargeTerrainFeature[] array3 = largeTerrainFeatures.ToArray();
                for (int k = 0; k < array3.Length; k++)
                {
                    array3[k].dayUpdate();
                }
            }
            objects.Lock();
            foreach (KeyValuePair<Vector2, SDObject> pair3 in objects.Pairs)
            {
                pair3.Value.DayUpdate();
                if (pair3.Value.destroyOvernight)
                {
                    pair3.Value.performRemoveAction();
                    objects.Remove(pair3.Key);
                }
            }
            objects.Unlock();
            RespawnStumpsFromMapProperty();
            //if (!(this is FarmHouse))
            //{
            debris.RemoveWhere((Debris d) => d.item == null && d.itemId.Value == null);
            //}
            if (map != null && (isOutdoors.Value || map.Properties.ContainsKey("ForceSpawnForageables")) && !map.Properties.ContainsKey("skipWeedGrowth"))
            {
                //if (Game1.dayOfMonth % 7 == 0 && !(this is Farm))
                //{
                //    Rectangle ignoreRectangle = new Rectangle(0, 0, 0, 0);
                //    //if (this is IslandWest)
                //    //{
                //    //    ignoreRectangle = new Microsoft.Xna.Framework.Rectangle(31, 3, 77, 70);
                //    //}
                //    KeyValuePair<Vector2, SDObject>[] array4 = objects.Pairs.ToArray();
                //    for (int k = 0; k < array4.Length; k++)
                //    {
                //        KeyValuePair<Vector2, SDObject> pair2 = array4[k];
                //        if ((bool)pair2.Value.isSpawnedObject && !ignoreRectangle.Contains(Utility.Vector2ToPoint(pair2.Key)))
                //        {
                //            objects.Remove(pair2.Key);
                //        }
                //    }
                //    numberOfSpawnedObjectsOnMap = 0;
                //    spawnObjects();
                //    spawnObjects();
                //}
                spawnObjects();
                if (Game1.dayOfMonth == 1)
                {
                    spawnObjects();
                }
                if (Game1.stats.DaysPlayed < 4)
                {
                    spawnObjects();
                }
                Layer pathsLayer = map.GetLayer("Paths");
                if (pathsLayer != null )
                {
                    for (int x = 0; x < map.Layers[0].LayerWidth; x++)
                    {
                        for (int y = 0; y < map.Layers[0].LayerHeight; y++)
                        {
                            if (!TryGetTreeIdForTile(pathsLayer.Tiles[x, y], out var treeId, out var _, out var growthStageOnRegrow, out var isFruitTree) || !Game1.random.NextBool())
                            {
                                continue;
                            }
                            Vector2 tile = new Vector2(x, y);
                            if (GetFurnitureAt(tile) == null && !terrainFeatures.ContainsKey(tile) && !objects.ContainsKey(tile) && getBuildingAt(tile) == null)
                            {
                                if (isFruitTree)
                                {
                                    terrainFeatures.Add(tile, new FruitTree(treeId, growthStageOnRegrow ?? 2));
                                }
                                else
                                {
                                    terrainFeatures.Add(tile, new Tree(treeId, growthStageOnRegrow ?? 2));
                                }
                            }
                        }
                    }
                }
            }
            terrainFeatures.RemoveWhere((KeyValuePair<Vector2, TerrainFeature> pair) => pair.Value is HoeDirt hoeDirt && (hoeDirt.crop == null || (bool)hoeDirt.crop.forageCrop.Value) && (!objects.TryGetValue(pair.Key, out var value) || value == null || !value.IsSpawnedObject || !value.isForage()) && Game1.random.NextBool(GetDirtDecayChance(pair.Key)));
            lightLevel.Value = 0f;
            foreach (Furniture item2 in furniture)
            {
                item2.minutesElapsed(Utility.CalculateMinutesUntilMorning(Game1.timeOfDay));
                item2.DayUpdate();
            }
            addLightGlows();
            //if (!(this is Farm))
            //{
            HandleGrassGrowth(dayOfMonth);
            //}
            foreach (Building building2 in buildings)
            {
                building2.dayUpdate(dayOfMonth);
            }
            foreach (string builder in new List<string>(Game1.netWorldState.Value.Builders.Keys))
            {
                BuilderData builderData = Game1.netWorldState.Value.Builders[builder];
                if (builderData.buildingLocation.Value == NameOrUniqueName)
                {
                    Building building = getBuildingAt(Utility.PointToVector2(builderData.buildingTile.Value));
                    if (building == null || (building.daysUntilUpgrade.Value == 0 && building.daysOfConstructionLeft.Value == 0))
                    {
                        Game1.netWorldState.Value.Builders.Remove(builder);
                    }
                    else
                    {
                        Game1.netWorldState.Value.MarkUnderConstruction(builder, building);
                    }
                }
            }
        }

        public void LoadBuildings(bool addToBaseFarm)
        {
            foreach (Building building in buildings)
            {
                //if load is not called, building interior map
                //is not loaded
                building.load();
                utilitiesService.FixBuildingWarps(building, Name);
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
        }

        public object Clone()
        {
            FarmExpansionLocation location = new FarmExpansionLocation(map, mapPath.Value, name.Value, logger, utilitiesService, modDataService, locationTunerIntegrationService)
            {
                baseExpansionName = name.Value
            };

            return location;
        }
    }
}
