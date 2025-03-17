using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.Integrations;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using CustomMenuFramework.Menus;
using System;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Utilities;
using StardewValley.BellsAndWhistles;


namespace SDV_Realty_Core.Framework.ServiceProviders.ModMechanics
{
    internal class WeatherManagerService : IWeatherManagerService
    {
        private static IModDataService modDataService;
        private static ILoggerService loggerService;
        private static ILocationTunerIntegrationService locationTunerIntegrationService;
        private static bool onlyFireWhenCurrent = true;
        private static bool expansionLightningEnabled = true;
        public override Type[] InitArgs => new Type[]
        {
            typeof(IUtilitiesService),
            typeof(IExpansionManager),typeof(IModDataService),
            typeof(IInputService),typeof(ILocationTunerIntegrationService)

        };

        public override object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == ServiceType)
                return this;

            return null;
        }

        internal override void Initialize(ILoggerService logger, object[] args)
        {
            this.logger = logger;
            // add static reference
            loggerService = logger;
            IUtilitiesService utilitiesService = (IUtilitiesService)args[0];
            IExpansionManager expansionManager = (IExpansionManager)args[1];
            modDataService = (IModDataService)args[2];
            IInputService inputService = (IInputService)args[3];
            locationTunerIntegrationService = (ILocationTunerIntegrationService)args[4];

#if DEBUG
            // add location critter and lightning triggering menu
            inputService.AddKeyBind(new KeybindList(new Keybind(SButton.L, SButton.LeftControl)), HandleLightningKey);
#endif
            WeatherManager = new Weather.WeatherManager(logger, utilitiesService, expansionManager, modDataService);

            expansionLightningEnabled = modDataService.Config.EnableExpansionLightning;

            utilitiesService.PatchingService.patches.AddPatch(false, 1000, typeof(Utility), "performLightningUpdate",
              new Type[] { typeof(int) }, GetType(), nameof(AddLightning),
             "Add lightning strikes to Farm Expansions.", "Lightning");

            utilitiesService.PatchingService.ApplyPatches("Lightning");
        }
        /// <summary>
        /// Popup critter and lightning trigger enu
        /// </summary>
        /// <param name="exp">Farm Expansion to trigger in</param>
        private void PopInsertChoice(FarmExpansionLocation exp)
        {
            GenericPickListMenu pickProperty = new GenericPickListMenu();
            List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>( "smalll","Small Lightning"),
                new KeyValuePair<string, string>( "bigl","Big Lightning"),
                new KeyValuePair<string, string>( "bunnies","Add a Rabbit"),
                new KeyValuePair<string, string>( "birdies","Add a Bird"),
                new KeyValuePair<string, string>( "butterflies","Add Butterflies"),
                new KeyValuePair<string, string>( "crows","Add a Crow"),
                new KeyValuePair<string, string>( "frogs","Add a Frog"),
                new KeyValuePair<string, string>( "wpecker","Add a Woodpecker"),
                new KeyValuePair<string, string>( "owl","Add an Owl"),
                new KeyValuePair<string, string>( "opossum","Add an Opossum"),
                new KeyValuePair<string, string>( "groundmon","Add a Ground Monster"),
                new KeyValuePair<string, string>( "flyingmon","Add a Flying Monster")
            };

            pickProperty.ShowPagedResponses("Spawn", options, delegate (string value)
            {
                switch (value)
                {
                    case "flyingmon":
                        exp.spawnFlyingMonstersOffScreen();
                        break;
                    case "groundmon":
                        exp.spawnGroundMonsterOffScreen();
                        break;
                    case "bigl":
                        exp.forceLightning = true;
                        DelayedAction.functionAfterDelay(() =>
                        DoBigLightning(exp.Name, Game1.timeOfDay), 500
                            );
                        break;
                    case "smalll":
                        exp.forceLightning = true;
                        DelayedAction.functionAfterDelay(() =>
                       DoLittleLightning(exp.Name), 500
                           );
                        break;
                    case "bunnies":
                        Vector2 pos = Game1.player.Tile;
                        pos.X--;

                        exp.critters.Add(new Rabbit(exp, pos, false));
                        exp.addBunnies(1);
                        break;
                    case "birdies":
                        exp.addBirdies(1);
                        break;
                    case "butterflies":
                        exp.addButterflies(1);
                        break;
                    case "crows":
                        exp.addCrows();
                        break;
                    case "frogs":
                        Vector2 pos1 = Game1.player.Tile;
                        pos1.X--;
                        exp.critters.Add(new Frog(pos1, false, false));
                        exp.addFrog();
                        break;
                    case "wpecker":
                        exp.addWoodpecker(1);
                        break;
                    case "owl":
                        exp.addOwl();
                        break;
                    case "opossum":
                        exp.addOpossums(1);
                        break;
                }
            }, auto_select_single_choice: true);

        }
        /// <summary>
        /// Handle hotkey to popup critter and lightning trigger menu
        /// </summary>
        /// <param name="key">Prefiltered keybind</param>
        private void HandleLightningKey(KeybindList key)
        {
            if (Game1.currentLocation is FarmExpansionLocation exp)
            {
                PopInsertChoice(exp);
            }
        }
        /// <summary>
        /// Called by patch of Utility.performLightningUpdate
        /// to add lightning strikes to Farm Expansions
        /// </summary>
        public static void AddLightning()
        {
            if (!expansionLightningEnabled)
                return;

            var expansions = modDataService.farmExpansions.Where(p => p.Value.Active).Select(p => p.Key).ToList();
            foreach (var exp in expansions)
            {
                try
                {
#if DEBUG
                    loggerService.Log($"Doing lightning for {exp}", LogLevel.Debug);
#endif
                    DoLocationLightning(exp, Game1.timeOfDay);
                }
                catch (Exception ex)
                {
                    loggerService.LogError(ex);
                }
            }
        }
        /// <summary>
        /// Create a big lightning event in an Expansion
        /// </summary>
        /// <param name="locationName">Location to trigger in</param>
        /// <param name="time_of_day">Current game time of day</param>
        public static void DoBigLightning(string locationName, int time_of_day)
        {
            FarmExpansionLocation farm = modDataService.farmExpansions[locationName];
            Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, time_of_day);
            Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
            lightningEvent.bigFlash = true;
            //GameLocation farm = Game1.getFarm();
            bool doVisual = !onlyFireWhenCurrent || Game1.currentLocation.Name == locationName;
            loggerService.Log($"   DoBigLightning for {locationName}", LogLevel.Debug);
            loggerService.Log($"   doVisual: {doVisual}", LogLevel.Debug);
            List<Vector2> lightningRods = new List<Vector2>();
            foreach (KeyValuePair<Vector2, SDObject> v2 in farm.objects.Pairs)
            {
                if (v2.Value.QualifiedItemId == "(BC)9")
                {
                    lightningRods.Add(v2.Key);
                }
            }
            if (lightningRods.Count > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 v = random.ChooseFrom(lightningRods);
                    if (farm.objects[v].heldObject.Value == null)
                    {
                        farm.objects[v].heldObject.Value = ItemRegistry.Create<SDObject>("(O)787");
                        farm.objects[v].minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                        farm.objects[v].shakeTimer = 1000;
                        if (doVisual)
                        {
                            lightningEvent.createBolt = true;
                            lightningEvent.boltPosition = v * 64f + new Vector2(32f, 0f);
                            farm.lightningStrikeEvent.Fire(lightningEvent);
                        }
                        return;
                    }
                }
            }
            if (random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() - Game1.player.team.AverageLuckLevel() / 100.0)
            {
                try
                {
                    if (Utility.TryGetRandom(farm.terrainFeatures, out var tile, out var feature))
                    {
                        if (feature is FruitTree fruitTree)
                        {
#if DEBUG
                            loggerService.Log($"   Fruit tree hit. ({tile})", LogLevel.Debug);
#endif
                            fruitTree.struckByLightningCountdown.Value = 4;
                            fruitTree.shake(tile, doEvenIfStillShaking: true);
                            if (doVisual)
                            {
                                lightningEvent.createBolt = true;
                                lightningEvent.boltPosition = tile * 64f + new Vector2(32f, -128f);
                            }
                        }
                        else
                        {
                            Crop crop = (feature as HoeDirt)?.crop;
                            bool num = crop != null && !crop.dead.Value;
                            if (feature.performToolAction(null, 50, tile))
                            {
#if DEBUG
                                loggerService.Log($"   Crop destroyed. ({tile})",LogLevel.Debug);
#endif
                                farm.terrainFeatures.Remove(tile);
                                if (doVisual)
                                {
                                    lightningEvent.destroyedTerrainFeature = true;
                                    lightningEvent.createBolt = true;
                                    lightningEvent.boltPosition = tile * 64f + new Vector2(32f, -128f);
                                }
                            }
                            if (num && crop.dead.Value)
                            {
#if DEBUG
                                loggerService.Log($"   Crop dead. ({tile})", LogLevel.Debug);
#endif
                                if (doVisual)
                                {
                                    lightningEvent.createBolt = true;
                                    lightningEvent.boltPosition = tile * 64f + new Vector2(32f, 0f);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            if (doVisual)
            {
#if DEBUG
                loggerService.Log($"   Doing visual. ({lightningEvent.boltPosition})", LogLevel.Debug);
#endif
                farm.lightningStrikeEvent.Fire(lightningEvent);
            }
        }
        /// <summary>
        /// Create a little lightning strike in an Expansion
        /// </summary>
        /// <param name="locationName">Location to trigger in</param>
        public static void DoLittleLightning(string locationName)
        {
            bool doVisual = !onlyFireWhenCurrent || Game1.currentLocation.Name == locationName;
            loggerService.Log($"   DoLittleLightning for {locationName}", LogLevel.Debug);
            loggerService.Log($"   doVisual: {doVisual}", LogLevel.Debug);

            if (doVisual)
            {
                FarmExpansionLocation farm = modDataService.farmExpansions[locationName];

                Farm.LightningStrikeEvent lightningEvent2 = new Farm.LightningStrikeEvent();
                lightningEvent2.smallFlash = true;
                farm.lightningStrikeEvent.Fire(lightningEvent2);
            }
        }
        /// <summary>
        /// Spawn lightning for a location
        /// </summary>
        /// <param name="locationName"></param>
        /// <param name="time_of_day"></param>
        private static void DoLocationLightning(string locationName, int time_of_day)
        {
            FarmExpansionLocation farm = modDataService.farmExpansions[locationName];

            if (locationTunerIntegrationService.NoLightning(locationName, out bool value))
            {
                if (value)
                {
#if DEBUG
                    loggerService.Log("   Disabled by LocationTuner", LogLevel.Debug);
#endif
                    return;
                }
            }
            if (farm.GetSeason() != Season.Winter)
            {
                Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, time_of_day);
                if (random.NextDouble() < 0.125 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() / 100.0)
                {
                    DoBigLightning(locationName, time_of_day);
                }
                else if (random.NextDouble() < 0.1)
                {
                    DoLittleLightning(locationName);
                }
            }
#if DEBUG
            else
            {
                loggerService.Log("    Skipped, season is Winter", LogLevel.Debug);
            }
#endif
        }
    }
}
