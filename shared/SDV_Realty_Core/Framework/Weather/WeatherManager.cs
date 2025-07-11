using Microsoft.Xna.Framework.Graphics;
using Prism99_Core.Utilities;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using StardewValley.Network;
using xTile.Dimensions;


namespace SDV_Realty_Core.Framework.Weather
{
    internal class WeatherManager
    {
        public struct CurrentWeather
        {
            public string WeatherId;
            public int WeatherPhase;
            public int PhaseCountDown;
            public string PreviousWeather;
            public int IntParam1;
            public int IntParam2;
            public int IntParam3;
            public Point Location;
            public Size Size;
            public GameLocation GameLocation;
        }
        internal struct WeatherPhase
        {
            public string Weather;
            public int Duration;
        }
        private ILoggerService logger;
        private static Dictionary<string, CurrentWeather> locationWeather;
        private Dictionary<string, WeatherCycle> weatherCycles;
        private static Dictionary<string, IWeatherCondition> weatherConditions;
        private Dictionary<int, string> MapGrid;
        private Random randomWeather = new Random();
        private IExpansionManager _expansionManager;
        private static IModDataService _modDataService;
        private readonly string contextId_always_sunny = "prism99.advize.stardewrealty.always_sunny";
        private readonly string contextId_always_raining = "prism99.advize.stardewrealty.always_rain";
        private readonly string contextId_always_snowing = "prism99.advize.stardewrealty.always_snow";
        public WeatherManager(ILoggerService olog, IUtilitiesService utilitiesService,  IExpansionManager expansionManager, IModDataService modDataService)
        {
            logger = olog;
            MapGrid = modDataService.MapGrid;
            locationWeather = new Dictionary<string, CurrentWeather>();
            weatherConditions = new Dictionary<string, IWeatherCondition> { };
            _expansionManager = expansionManager;
            _modDataService = modDataService;

            utilitiesService.GameEventsService.AddSubscription(typeof(TimeChangedEventArgs).Name, HandleTimeChanged);
            utilitiesService.GameEventsService.AddSubscription(new OneSecondUpdateTickedEventArgs(), HandleOneSecondUpdateTicked);
            utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), HandleDayStarted);
            utilitiesService.GameEventsService.AddSubscription(new UpdateTickedEventArgs(), HandleTickEvent);
            //
            //  add weather types
            //
            AddWeatherCondition(new LightRain());
            AddWeatherCondition(new Drizzle());
            AddWeatherCondition(new fog(utilitiesService.ModHelperService.modHelper.ModContent));
            //
            //  add weather cycles
            //
            weatherCycles = new Dictionary<string, WeatherCycle>
            {
                {"prism99.advize.stardewrealty.foggy" , new WeatherCycle
                {
                    Phases=new List<WeatherPhase>
                    {
                        new WeatherPhase{Weather="prism99.advize.stardewrealty.fog",Duration=600}
                    },
                    Seasons=new List<Season>{Season.Spring,Season.Spring,Season.Fall}

                }
                },

                {"prism99.advize.stardewrealty.rainstorms",new WeatherCycle
                    {
                        Phases=new List<WeatherPhase>
                        {
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain",Duration=40 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 50 },
                            new WeatherPhase { Weather = contextId_always_raining, Duration = 40 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 30 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain", Duration = 20 },
                            new WeatherPhase { Weather = contextId_always_sunny, Duration = 40 },
                            new WeatherPhase { Weather = contextId_always_snowing, Duration = 20 },
                            new WeatherPhase { Weather = contextId_always_sunny, Duration = 40 }
                        },
                        Seasons=new List<Season>{Season.Spring,Season.Spring,Season.Fall}
                    }
                },
                {"prism99.advize.stardewrealty.morning_rain",new WeatherCycle
                    {
                        Phases=new List<WeatherPhase>
                        {
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain",Duration=60 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 60 },
                            new WeatherPhase { Weather = contextId_always_raining, Duration = 180 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 30 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain", Duration = 30 },
                            new WeatherPhase { Weather = contextId_always_sunny, Duration = 720 },
                        },
                        Seasons=new List<Season>{Season.Spring,Season.Spring,Season.Fall}
                    }
                },
                {"prism99.advize.stardewrealty.afternoon_rain",new WeatherCycle
                     {
                        Phases=new List<WeatherPhase>
                        {
                            new WeatherPhase { Weather = contextId_always_sunny, Duration = 600 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain",Duration=60 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 60 },
                            new WeatherPhase { Weather = contextId_always_raining, Duration = 180 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 30 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain", Duration = 30 },
                        },
                        Seasons=new List<Season>{Season.Spring,Season.Spring,Season.Fall}
                     }
                }
            };

            //
            //  weather patches
            //
            utilitiesService.PatchingService.patches.AddPatch(true, typeof(Game1), "drawWeather",
                new Type[] { typeof(GameTime), typeof(RenderTarget2D) },
                typeof(WeatherManager), nameof(drawWeather_Prefix),
                "Add custom weather patterns to expansions.",
                "Game1");

            utilitiesService.PatchingService.patches.AddPatch(true, typeof(Game1), "updateWeather",
                new Type[] { typeof(GameTime) },
                typeof(WeatherManager), nameof(updateWeather_Prefix),
                "Add custom weather patterns to expansions.",
                "Game1");

            utilitiesService.PatchingService.patches.AddPatch(false, typeof(GameLocation), "GetWeather",
                new Type[] { },
                typeof(WeatherManager), nameof(GetWeather),
                "Set Weather value for location with custom weather.",
                "Game1");
        }


        #region "Event Handlers"
        private void HandleTickEvent(EventArgs e)
        {
            if (Game1.IsMasterGame && _modDataService.Config.UseCustomWeather)
            {
                foreach (string key in locationWeather.Keys)
                {
                    CurrentWeather cw = locationWeather[key];
                    string weatherId = weatherCycles[cw.WeatherId].Phases[0].Weather;
                    if (weatherConditions.TryGetValue(LocationContexts.Require(weatherId).WeatherConditions[0].Weather, out var handler))
                    {
                        handler.UpdateTicked(ref cw);
                        locationWeather[key] = cw;
                    }
                }
            }
        }
        private void HandleDayStarted(EventArgs e)
        {
            DayStartedEventArgs dayStartedEventArgs = e as DayStartedEventArgs;

            //
            //  set weather for all expansions
            //
            if (Game1.IsMasterGame && _modDataService.Config.UseCustomWeather)
            {
                locationWeather.Clear();

                foreach (string expansionName in _modDataService.MapGrid.Values)
                {
                    ExpansionPack location = _modDataService.validContents[expansionName];
                    //
                    //  check for location that has variable weather
                    //
                    if (!location.AlwaysSunny && !location.AlwaysRaining && !location.AlwaysSnowing)
                    {
                        Season expansionSeason = GetSeasonFromString(_expansionManager.expansionManager.GetExpansionSeasonalOverride(expansionName));
                        //
                        //  get list of weather cycles for the expansion season
                        //
                        List<KeyValuePair<string, WeatherCycle>> cycleList = weatherCycles.Where(p => p.Value.Seasons.Contains(expansionSeason)).ToList();
                        //
                        //  pick random number to see if custom weather is selected
                        //
                        int weatherType = randomWeather.Next(0, cycleList.Count * 2);

                        if (weatherType < cycleList.Count)
                        {
                            //
                            //  custom weather chosen
                            //
                            //string weatherCondition = "prism99.advize.stardewrealty.foggy";
                            string weatherCondition = cycleList.ToArray()[weatherType].Key;
                            logger.Log($"Setting expansion: '{expansionName}' to weather '{weatherCondition}'", LogLevel.Debug);
                            locationWeather.Add(expansionName, new CurrentWeather { WeatherId = weatherCondition, WeatherPhase = 0, PhaseCountDown = -1, GameLocation = Game1.getLocationFromName(expansionName) });
                        }
                        else
                        {
                            //
                            //  set location to game current weather
                            //
                            if (string.IsNullOrEmpty(_modDataService.validContents[expansionName].LocationContextId))
                            {
                                Game1.getLocationFromName(expansionName).locationContextId = "Default";
                                logger.Log($"Setting expansion: '{expansionName}' to weather default weather", LogLevel.Debug);
                            }
                            else
                            {
                                Game1.getLocationFromName(expansionName).locationContextId = _modDataService.validContents[expansionName].LocationContextId;
                            }
                        }
                    }
                }
            }
            if (_modDataService.Config.ApplyWeatherFixes)
            {
                //
                //  add game weather fixes
                //
                //  1. Stop snow in spring, summer and fall
                //  2. Stop rain in winter
                //
                LocationWeather oWeather = Game1.netWorldState.Value.GetWeatherForLocation(Game1.getFarm().GetLocationContextId());
                //
                //  check for snow in non=Winter locations
                //
                IEnumerable<string> nonWinter = _modDataService.MapGrid.Values.Where(p => !string.IsNullOrEmpty(_modDataService.expDetails[p].SeasonOverride) && _modDataService.expDetails[p].SeasonOverride != "Winter");

                foreach (string expansionName in nonWinter)
                {
                    if (oWeather.IsSnowing)
                    {
                        // add always raining context
                        Game1.getLocationFromName(expansionName).locationContextId = contextId_always_raining;
                    }
                    else
                    {
                        // remove always sunny context
                        Game1.getLocationFromName(expansionName).locationContextId = "Default";
                    }

                }
                //
                //  check for rain in winter locations
                //
                IEnumerable<string> winter = _modDataService.MapGrid.Values.Where(p => !string.IsNullOrEmpty(_modDataService.validContents[p].SeasonOverride) && _modDataService.validContents[p].SeasonOverride == "Winter");
                foreach (string expansionName in winter)
                {
                    if (oWeather.IsRaining)
                    {
                        // add always sunny context
                        Game1.getLocationFromName(expansionName).locationContextId = contextId_always_snowing;
                    }
                    else
                    {
                        // remove always sunny context
                        Game1.getLocationFromName(expansionName).locationContextId = "Default";
                    }
                }
            }
        }
        private void HandleOneSecondUpdateTicked(EventArgs e)
        {
            if (Game1.IsMasterGame && _modDataService.Config.UseCustomWeather)
            {
                foreach (string key in locationWeather.Keys)
                {
                    CurrentWeather cw = locationWeather[key];
                    string weatherId = weatherCycles[cw.WeatherId].Phases[0].Weather;
                    if (weatherConditions.TryGetValue(LocationContexts.Require(weatherId).WeatherConditions[0].Weather, out var handler))
                    {
                        handler.OneSecondUpdateTicked((OneSecondUpdateTickedEventArgs)e, ref cw);
                        locationWeather[key] = cw;
                    }
                }
            }
        }
        private void HandleTimeChanged(EventArgs e)
        {
            if (Game1.IsMasterGame && _modDataService.Config.UseCustomWeather)
            {
                //
                //  check for location weather changes
                //
                foreach (string key in locationWeather.Keys)
                {
                    if (Game1.getLocationFromName(key) != null)
                    {
                        string weatherId = null;
                        CurrentWeather cw = locationWeather[key];
                        if (cw.PhaseCountDown == -1)
                        {
                            cw.WeatherPhase = 0;
                            cw.PhaseCountDown = weatherCycles[cw.WeatherId].Phases[0].Duration;
                            cw.PreviousWeather = Game1.getLocationFromName(key).GetLocationContext().WeatherConditions[0].Weather;
                            weatherId = weatherCycles[cw.WeatherId].Phases[0].Weather;
                            if (weatherConditions.TryGetValue(LocationContexts.Require(weatherId).WeatherConditions[0].Weather, out var handler))
                            {
                                handler.StartCycle(ref cw);
                            }
                        }
                        else
                        {
                            cw.PhaseCountDown -= 10;
                            if (cw.PhaseCountDown <= 0)
                            {
                                cw.PreviousWeather = cw.WeatherId;
                                cw.WeatherPhase++;
                                if (cw.WeatherPhase > weatherCycles[cw.WeatherId].Phases.Count - 1)
                                    cw.WeatherPhase = 0;

                                cw.PhaseCountDown = weatherCycles[cw.WeatherId].Phases[cw.WeatherPhase].Duration;
                                weatherId = weatherCycles[cw.WeatherId].Phases[cw.WeatherPhase].Weather;
                                if (weatherConditions.TryGetValue(LocationContexts.Require(weatherId).WeatherConditions[0].Weather, out var handler))
                                {
                                    handler.StartCycle(ref cw);
                                }
                            }
                        }
                        locationWeather[key] = cw;

                        if (weatherId != null)
                        {
                            GameLocation wl = Game1.getLocationFromName(key);
                            if (wl != null)
                            {
                                logger.Log($"Setting {key} weather to {weatherId}", LogLevel.Debug);
                                wl.locationContextId = weatherId;
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #region "Private Methods"
        /// <summary>
        /// Override GameLocation.GetWeather to set details of custom
        /// weather condition
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="__instance"></param>
        private static void GetWeather(ref LocationWeather __result, GameLocation __instance)
        {
            if (_modDataService.Config.UseCustomWeather)
            {
                if (!string.IsNullOrEmpty(__result.Weather) && weatherConditions.ContainsKey(__result.Weather))
                {
                    __result.IsRaining = weatherConditions[__result.Weather].IsRaining();
                    __result.IsSnowing = weatherConditions[__result.Weather].IsSnowing();
                    //
                    //  set last so not to screwup setting of previous values
                    //
                    __result.Weather = weatherConditions[__result.Weather].DisplayName();
                }
            }
        }
        private Season GetSeasonFromString(string season)
        {
            if (season == null)
                return Season.Spring;

            if (SDVUtilities.TryParseEnum(season, out Season parsedSeason))
                return parsedSeason;

            return Season.Spring;
        }
        /// <summary>
        /// Add custom weather definitions
        /// </summary>
        /// <param name="cond">New weather defintion to add</param>
        private void AddWeatherCondition(IWeatherCondition cond)
        {
            if (!weatherConditions.ContainsKey(cond.Name()))
            {
                weatherConditions.Add(cond.Name(), cond);
            }
        }
        #endregion

        #region "Harmony Patches"
        public static bool drawWeather_Prefix(GameTime time, RenderTarget2D target_screen, Game1 __instance)
        {
            if (!_modDataService.Config.UseCustomWeather)
                return true;

            if (Game1.currentLocation != null && Game1.currentLocation.GetLocationContext() != null)
            {
                if (Game1.currentLocation.GetLocationContext().WeatherConditions.Count > 0)
                {
                    string wc = Game1.currentLocation.GetLocationContext().WeatherConditions[0].Weather;
                    if (!weatherConditions.TryGetValue(wc, out var handler))
                    {
                        return true;
                    }
                    if (locationWeather.ContainsKey(Game1.currentLocation.Name))
                    {
                        CurrentWeather cw = locationWeather[Game1.currentLocation.Name];
                        handler.Draw(ref cw);
                        locationWeather[Game1.currentLocation.Name] = cw;
                        return false;
                    }
                }
            }

            return true;
        }
        public static bool updateWeather_Prefix(GameTime time, Game1 __instance)
        {
            if (!_modDataService.Config.UseCustomWeather)
                return true;

            if (Game1.currentLocation != null && Game1.currentLocation.GetLocationContext() != null)
            {
                if (Game1.currentLocation.GetLocationContext().WeatherConditions.Count > 0)
                {
                    string wc = Game1.currentLocation.GetLocationContext().WeatherConditions[0].Weather;
                    if (!weatherConditions.TryGetValue(wc, out var handler))
                    {
                        return true;
                    }
                    if (locationWeather.ContainsKey(Game1.currentLocation.Name))
                    {
                        CurrentWeather cw = locationWeather[Game1.currentLocation.Name];
                        handler.Update(time, ref cw);
                        locationWeather[Game1.currentLocation.Name] = cw;
                        return false;
                    }
                }
                return true;
            }

            return true;
        }
        #endregion
    }
}
