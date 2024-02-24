using Microsoft.Xna.Framework.Graphics;
using Prism99_Core.Utilities;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces;


namespace SDV_Realty_Core.Framework.Weather
{
    internal  class WeatherManager
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
        }
        internal struct WeatherPhase
        {
            public string Weather;
            public int Duration;
        }
        private  ILoggerService logger;
        private static  Dictionary<string, CurrentWeather> locationWeather;
        private  Dictionary<string, WeatherCycle> weatherCycles;
        private static  Dictionary<string, IWeatherCondition> weatherConditions;
        private  Dictionary<int, string> MapGrid;
        private  FEConfig config;
        private  Random randomWeather = new Random();
        private IExpansionManager _expansionManager;
        private IModDataService _modDataService;
        public WeatherManager(ILoggerService olog, IUtilitiesService utilitiesService, Dictionary<int, string> mapGrid, IExpansionManager expansionManager,IModDataService modDataService)
        {
            config =utilitiesService.ConfigService.config;
            logger = olog;
            MapGrid = mapGrid;
            locationWeather = new Dictionary<string, CurrentWeather>();
            weatherConditions = new Dictionary<string, IWeatherCondition> { };
            _expansionManager = expansionManager;
            _modDataService = modDataService;

            utilitiesService.GameEventsService.AddSubscription(typeof( TimeChangedEventArgs).Name, GameLoop_TimeChanged);
            utilitiesService.GameEventsService.AddSubscription(new OneSecondUpdateTickedEventArgs(), GameLoop_OneSecondUpdateTicked);
            utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), GameLoop_DayStarted);

            //helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            //helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            //helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;


            //
            //  add weather types
            //
            AddWeatherCondition(new LightRain());
            AddWeatherCondition(new Drizzle());
            //
            //  add weather cycles
            //
            weatherCycles = new Dictionary<string, WeatherCycle>
            {
                {"prism99.advize.stardewrealty.rainstorms",new WeatherCycle
                    {
                        Phases=new List<WeatherPhase>
                        {
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain",Duration=40 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 50 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.always_rain", Duration = 40 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 30 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain", Duration = 20 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.always_sunny", Duration = 40 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.always_snow", Duration = 20 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.always_sunny", Duration = 40 }
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
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.always_rain", Duration = 180 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 30 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain", Duration = 30 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.always_sunny", Duration = 720 },
                        },
                        Seasons=new List<Season>{Season.Spring,Season.Spring,Season.Fall}
                    }
                },
                {"prism99.advize.stardewrealty.afternoon_rain",new WeatherCycle
                     {
                        Phases=new List<WeatherPhase>
                        {
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.always_sunny", Duration = 600 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.light_rain",Duration=60 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.drizzle", Duration = 60 },
                            new WeatherPhase { Weather = "prism99.advize.stardewrealty.always_rain", Duration = 180 },
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
        }

        private  Season GetSeasonFromString(string season)
        {
            if (season == null)
                return Season.Spring;

            if (SDVUtilities.TryParseEnum<Season>(season, out Season parsedSeason))
                return parsedSeason;

            return Season.Spring;
        }
        private  void GameLoop_DayStarted(EventArgs e)
        {
            //
            //  set weather for all expansions
            //
            if (config.UseCustomWeather)
            {
                locationWeather.Clear();

                foreach (string expansionName in _modDataService.MapGrid.Values)
                {
                    var location = _modDataService.validContents[expansionName];
                    //
                    //  check for location that has variable weather
                    //
                    if (!location.AlwaysSunny && !location.AlwaysRaining && !location.AlwaysSnowing)
                    {
                        Season expansionSeason = GetSeasonFromString(_expansionManager.expansionManager.GetExpansionSeasonalOverride(expansionName));
                        //
                        //  get list of weather cycles for the expansion season
                        //
                        var cycleList = weatherCycles.Where(p => p.Value.Seasons.Contains(expansionSeason)).ToList();
                        //
                        //  pick random number to see if custom weather is selected
                        //
                        int weatherType = randomWeather.Next(0, cycleList.Count * 2);

                        if (weatherType < cycleList.Count)
                        {
                            //
                            //  custom weather chosen
                            //
                            string weatherCondtion = cycleList.ToArray()[weatherType].Key;
                            logger.Log($"Setting expansion: '{expansionName}' to weather '{weatherCondtion}'", LogLevel.Debug);
                            locationWeather.Add(expansionName, new CurrentWeather { WeatherId = weatherCondtion, WeatherPhase = 0, PhaseCountDown = -1 });
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
        }

        private  void GameLoop_OneSecondUpdateTicked(EventArgs e)
        {
            if (config.UseCustomWeather)
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
        /// <summary>
        /// Add custom weather definitions
        /// </summary>
        /// <param name="cond">New weather defintion to add</param>
        private  void AddWeatherCondition(IWeatherCondition cond)
        {
            if (!weatherConditions.ContainsKey(cond.Name()))
            {
                weatherConditions.Add(cond.Name(), cond);
            }
        }
        private  void GameLoop_TimeChanged(EventArgs e)
        {
            if (config.UseCustomWeather)
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
        public static bool drawWeather_Prefix(GameTime time, RenderTarget2D target_screen, Game1 __instance)
        {
            if (Game1.currentLocation != null && Game1.currentLocation.GetLocationContext() != null)
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

                return true;
            }

            return true;
        }
        public static bool updateWeather_Prefix(GameTime time, Game1 __instance)
        {
            if (Game1.currentLocation != null && Game1.currentLocation.GetLocationContext() != null)
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

                return true;
            }

            return true;
        }

    }
}
