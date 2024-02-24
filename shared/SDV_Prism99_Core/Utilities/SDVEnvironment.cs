using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prism99_Core.MultiplayerUtils;
using StardewModdingAPI.Events;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Characters;
using System.Runtime.CompilerServices;
using SDV_Realty_Core.Framework.ServiceInterfaces.Game;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;

namespace Prism99_Core.Utilities
{
    internal class ObjectReferenceComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return (object)x == (object)y;
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
    internal  class SDVEnvironment
    {
        public enum GameMode
        {
            SinglePlayer,
            Multiplayer,
            SplitScreen
        }
        public enum PlayerTypes
        {
            Master,
            RemotePlayer,
            SplitScreenPlayer
        }
        public enum GameState
        {
            None,
            Started,
            GameLoaded,
            SaveLoaded
        }
        private static readonly int MaxOccupantsID = -794739;

        private static int WillyBoatFixed = 9348571;
        private static ILoggerService logger;
        private static IModHelperService helper;
        private static readonly string BlueprintBuildingType = "TractorGarage";

        public static GameMode CurrentGameMode { get; private set; }
        public static PlayerTypes PlayerType { get; private set; }
        public static Game1 RootGame { get; private set; }
        public static bool IsWindows { get; private set; }
        public static string ModsDir { get; private set; }
        public string ModDir { get; private set; }
        public  string GamePath { get; private set; }
        public static GameState CurrentState { get; set; } = GameState.None;
        public static bool IsMonoGame { get; private set; }
        public static bool IsGingerIslandOpen
        {
            get
            {
#if v16
            return Game1.MasterPlayer.eventsSeen.Contains(WillyBoatFixed.ToString());
#else
                return Game1.MasterPlayer.eventsSeen.Contains(WillyBoatFixed);
#endif
            }
        }
        public static bool QuarryUnlocked { get; set; }
        public static bool CommunityCenterOpen { get; set; }

        public static IEnumerable<Stable> GetGaragesIn(GameLocation location)
        {
#if v16
            GameLocation buildableLocation = location.IsBuildableLocation() ? location : null;
#else
            BuildableGameLocation buildableLocation = (BuildableGameLocation)((location is BuildableGameLocation) ? location : null);
#endif
            if (buildableLocation == null)
            {
                return Enumerable.Empty<Stable>();
            }
            return buildableLocation.buildings.OfType<Stable>().Where(new Func<Stable, bool>(IsGarage));
        }
        private  static IEnumerable<Horse> GetTractorsIn(GameLocation location, bool includeMounted = true)
        {
            if (!Context.IsMultiplayer || !includeMounted)
            {
                return location.characters.OfType<Horse>().Where(new Func<Horse, bool>(IsTractor));
            }
            return location.characters.OfType<Horse>().Where(new Func<Horse, bool>(IsTractor)).Concat(from player in location.farmers
                                                                                                      where IsTractor(player.mount)
                                                                                                      select player.mount)
                .Distinct(new ObjectReferenceComparer<Horse>());
        }
        public static bool ManagerIsTractor(Horse horse)
        {
            string value;
            return ((Character)horse)?.modData.TryGetValue("Pathoschild.TractorMod", out value) ?? false;
        }
        private static bool IsTractor(Horse horse)
        {
            return ManagerIsTractor(horse);
        }
        public static void TractorDayEnding()
        {
            HashSet<Guid> validStableIDs = new HashSet<Guid>(from location in GetBuildableLocations()
                                                             from garage in GetGaragesIn((GameLocation)(object)location)
                                                             select garage.HorseId);
            HashSet<GameLocation> vanillaLocations = new HashSet<GameLocation>(Game1.locations, new ObjectReferenceComparer<GameLocation>());
            foreach (GameLocation location2 in GetLocations())
            {
                bool isValidLocation = vanillaLocations.Contains(location2);
                Horse[] array = GetTractorsIn(location2).ToArray();
                foreach (Horse tractor in array)
                {
                    if (!validStableIDs.Contains(tractor.HorseId))
                    {
                        location2.characters.Remove(tractor);
                    }
                    else if (!isValidLocation)
                    {
                        Game1.warpCharacter(tractor, "Farm", new Point(0, 0));
                    }
                }
            }
        }
        private static bool IsGarage(Stable stable)
        {
            if (stable != null)
            {
                if (stable.maxOccupants.Value != MaxOccupantsID)
                {
                    return stable.buildingType.Value == BlueprintBuildingType;
                }
                return true;
            }
            return false;
        }
#if v16
        public static IEnumerable<GameLocation> GetBuildableLocations()
        {
            return GetLocations().Where(p=>p.IsBuildableLocation());
        }
#else
        public static IEnumerable<BuildableGameLocation> GetBuildableLocations()
        {
            return GetLocations().OfType<BuildableGameLocation>();
        }
#endif
        public static IEnumerable<GameLocation> GetLocations()
        {
            IEnumerable<GameLocation> source;
            if (!Context.IsMainPlayer)
            {
                source = helper.Multiplayer.GetActiveLocations();
            }
            else
            {
                IEnumerable<GameLocation> locations = Game1.locations;
                source = locations;
            }
            GameLocation[] mainLocations = source.ToArray();
            foreach (GameLocation location in mainLocations.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels))
            {
                yield return location;
#if v16
                GameLocation buildableLocation = location.IsBuildableLocation() ? location : null;
#else
                BuildableGameLocation buildableLocation = (BuildableGameLocation)(object)((location is BuildableGameLocation) ? location : null);
#endif
                if (buildableLocation == null)
                {
                    continue;
                }
                foreach (Building building in buildableLocation.buildings)
                {
                    if (building.indoors.Value != null)
                    {
                        yield return building.indoors.Value;
                    }
                }
            }
        }

        public  void Initialize(IModHelperService ohelper, ILoggerService olog)
        {
            logger = olog;
            RootGame = Game1.game1;
            helper = ohelper;

            PlayerType = Game1.IsMasterGame ? PlayerTypes.Master : Context.IsSplitScreen?PlayerTypes.SplitScreenPlayer: PlayerTypes.RemotePlayer;
            CurrentGameMode = Game1.IsMasterGame ? Context.IsSplitScreen ? GameMode.SplitScreen : GameMode.Multiplayer : GameMode.SinglePlayer;
            OperatingSystem os_info = Environment.OSVersion;
            IsWindows = os_info.Platform.ToString().Contains("Win");
            if (IsWindows)
            {
                string[] arVer = Game1.version.Split('.');
                int ver = Convert.ToInt32(arVer[0]);
                int major = Convert.ToInt32(arVer[1]);
                int minor = Convert.ToInt32(arVer[1]);

                if (ver > 1) IsMonoGame = true;
                if (major > 5) IsMonoGame = true;
                if (major == 5 && minor > 4) IsMonoGame = true;

                IsMonoGame = false;
            }
            else
            {
                IsMonoGame = true;
            }

            GamePath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

            ModsDir = Path.Combine(GamePath, "Mods");

            ModDir = helper.DirectoryPath;

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                bool isModPath = false;

                foreach (string arg in Environment.GetCommandLineArgs())
                {
                    if (isModPath)
                    {
                        ModsDir = Path.Combine(GamePath, arg);
                        break;
                    }
                    isModPath = arg == "--mods-path";
                }
            }
            //helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            //helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            P99Core_MultiPlayer.Add_PeerConnectedCallback(Handle_PeerConnected);
            DumpGameMode();
        }
        private static void Handle_PeerConnected(PeerConnectedEventArgs e)
        {
            if (Context.IsSplitScreen)
            {
                CurrentGameMode = GameMode.SplitScreen;
                PlayerType = Context.IsMainPlayer ? PlayerTypes.Master : PlayerTypes.SplitScreenPlayer;
            }
            else
            {
                CurrentGameMode = GameMode.Multiplayer;
                PlayerType = Context.IsMainPlayer ? PlayerTypes.Master : PlayerTypes.RemotePlayer;
            }
            DumpGameMode();
        }
        private static void DumpGameMode()
        {
            logger.Log($"SDEnv: GameMode: {CurrentGameMode}", LogLevel.Debug);
            logger.Log($"SDEnv: PlayerType: {PlayerType}", LogLevel.Debug);
            logger.Log($"SDEnv: GameState: {CurrentState}", LogLevel.Debug);
        }
        private static void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            CurrentState = GameState.GameLoaded;
            DumpGameMode();
        }

        private static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            CurrentState = GameState.SaveLoaded;
            DumpGameMode();
        }
    }
}
