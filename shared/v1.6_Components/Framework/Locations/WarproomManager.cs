using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.Objects;
using SDV_Realty_Core.ContentPackFramework.Utilities;
using StardewModHelpers;
using xTile;
using xTile.Tiles;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Utilities;
using SDV_Realty_Core.Framework.AssetUtils;
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using StardewModdingAPI.Enums;

namespace SDV_Realty_Core.Framework.Locations
{
    /// <summary>
    /// version 1.6
    /// </summary>
    internal class WarproomManager
    {
        private ILoggerService logger;
        private SDRContentManager ContentManager = null;
        private Vector2 vWarpRoomSignLocation = new Vector2(6, 3);
        public const string WarpRoomLoacationName = "sdr_warproom";
        public GameLocation WarpRoom = null;
        public readonly PerScreen<bool> WarpRoomAdded = new PerScreen<bool>();
        public static bool WarpRoomInUse = false;
        public static long PlayerInWarpRoom;
        public static readonly Vector2 WarpRoomEntrancePoint = new Vector2(14, 11);
        public readonly Vector2 WarpRoomExitPoint = new Vector2(WarpRoomEntrancePoint.X, WarpRoomEntrancePoint.Y + 1);
        public Vector2 lastPlayerPoint = Vector2.Zero;
        private FEConfig Config;
        private Rectangle lastWarpCheck = Rectangle.Empty;
        private static bool warpRoomBusyPopped = false;
        private IUtilitiesService _utilitiesService;
        private IMultiplayerService multiplayerService;
        private static IExpansionManager _expansionManager;
        private IContentPackService contentPackService;

        public static Dictionary<string, Tuple<string, EntranceDetails>> CaveEntrances = new Dictionary<string, Tuple<string, EntranceDetails>>();

        public WarproomManager(ILoggerService errorLogger, SDRContentManager cManager, IUtilitiesService utilitiesService, IMultiplayerService multiplayerService, IExpansionManager expansionManager, IContentPackService contentPackService)
        {
            logger = errorLogger;
            this.multiplayerService = multiplayerService;
            ContentManager = cManager;
            _utilitiesService = utilitiesService;
            _expansionManager = expansionManager;
            this.contentPackService = contentPackService;
            utilitiesService.GameEventsService.AddSubscription("WarpedEventArgs", Player_Warped);
            utilitiesService.GameEventsService.AddSubscription(new UpdateTickingEventArgs(), GameLoop_UpdateTicking);
            utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), GameLoop_SaveLoaded);
            utilitiesService.GameEventsService.AddSubscription(new SavedEventArgs(), GameLoop_SaveLoaded);
            utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), LoadStageChanged);
            utilitiesService.GameEventsService.AddSubscription(new ReturnedToTitleEventArgs(), ReturnToTitle);
            //helper.Events.Player.Warped += Player_Warped;
            //helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            //helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Config = utilitiesService.ConfigService.config;
            //
            // warp  room patch
            //
            utilitiesService.PatchingService.patches.AddPatch(true, typeof(GameLocation), "isCollidingWithWarp",
                new Type[] { typeof(Rectangle), typeof(Character) }, typeof(WarproomManager), nameof(isCollidingWithWarp_Prefix),
                "Check warproom availability.",
                "Warps");
        }

        private void GameLoop_SaveLoaded(EventArgs e)
        {
            AddHomeWarpRoomLocations();
            AddWarpRoom();
        }
        internal Warp GetPlayerHomeWarp()
        {

            GameLocation glHome = Game1.getLocationFromName(Game1.player.homeLocation.Value);

            var oWarps = glHome.warps.Where(p => p.TargetName == "Farm");

            if (oWarps.Any())
            {
                Warp homeWarp = oWarps.First();
                return new Warp
                {
                    TargetName = Game1.player.homeLocation.Value,
                    TargetX = homeWarp.X,
                    TargetY = homeWarp.Y
                };

            }

            return null;
        }
        internal void AddCaveEntrance(string location, Tuple<string, EntranceDetails> position)
        {
            CaveEntrances.Add(location, position);
        }
        private void ReturnToTitle(EventArgs eventArgs)
        {
            CaveEntrances.Clear();
        }
        private void LoadStageChanged(EventArgs ep)
        {
            LoadStageChangedEventArgs e = (LoadStageChangedEventArgs)ep;
            if (e.NewStage == LoadStage.SaveAddedLocations || e.NewStage == LoadStage.CreatedInitialLocations)
            {
                // need to find better event or change AddCaveEntrance call time
                //CaveEntrances.Clear();
            }
        }
        internal void AddHomeWarpRoomLocations()
        {
            //
            //  add warp home
            //
            Warp oHomeWarp = GetPlayerHomeWarp();

            if (oHomeWarp != null && !CaveEntrances.ContainsKey(oHomeWarp.TargetName))
                CaveEntrances.Add(oHomeWarp.TargetName, Tuple.Create(oHomeWarp.TargetName, new EntranceDetails { WarpIn = new EntranceWarp { X = oHomeWarp.X, Y = oHomeWarp.Y }, WarpOut = new EntranceWarp { X = oHomeWarp.X, Y = oHomeWarp.Y } }));

        }

        public void AddWarpRoom()
        {

            logger?.Log("Adding Warproom", LogLevel.Debug);
            logger?.Log($"  Game Location loaded: {Game1.getLocationFromName(WarpRoomLoacationName) != null}", LogLevel.Debug);
            logger?.Log($"  Warproom cached: {WarpRoom != null}", LogLevel.Debug);
            logger?.Log($"  Master game: {Game1.IsMasterGame}", LogLevel.Debug);
            logger?.Log($"  Split screen: {Context.IsSplitScreen}", LogLevel.Debug);
            //
            //  added isplitscreen for mplayer support
            //
            if (!Game1.locations.Where(p => p.Name == WarpRoomLoacationName).Any())// && (Game1.IsMasterGame || Context.IsSplitScreen))
            {
                //
                //  add warproom map
                //
                if (WarpRoom != null)
                {
                    _expansionManager.expansionManager.AddGameLocation(WarpRoom, "AddWarpRoom");
                }
                else
                {
                    Map warpmap = _utilitiesService.MapLoaderService.LoadMap( Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "Maps", WarpRoomLoacationName, "warproom.tmx").Replace(_utilitiesService.GameEnvironment.GamePath, ""), "realtywarproom", false, false);
                    string assetPath = SDVPathUtilities.AssertAndNormalizeAssetName(Path.Combine("SDR", WarpRoomLoacationName, "warproom.tmx"));

                    //
                    //  this is all loaded by AddWarproomDefinition
                    //
                    //ContentManager.ExternalReferences.Add(assetPath, warpmap);
                    if (ContentManager.ExternalReferences.ContainsKey(assetPath))
                    {
                        ContentManager.ExternalReferences.Remove(assetPath);
                    }
                    ContentManager.ExternalReferences.Add(assetPath, warpmap);
                    ////
                    ////  add warproom tilesheet sources
                    ////
                    foreach (TileSheet oLayer in warpmap.TileSheets)
                    {
                        oLayer.ImageSource = oLayer.ImageSource.Replace("\\", "/");
                        if (!string.IsNullOrEmpty(oLayer.ImageSource) && oLayer.ImageSource.StartsWith($"SDR{FEConstants.AssetDelimiter}{WarpRoomLoacationName}"))
                        {
                            string sFilename = Path.GetFileName(oLayer.ImageSource);
                            string sFullname = Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "Maps", WarpRoomLoacationName, sFilename);

                            if (!ContentManager.ExternalReferences.ContainsKey(oLayer.ImageSource))
                            {
                                ContentManager.ExternalReferences.Add(oLayer.ImageSource, new StardewBitmap(sFullname).Texture());
                            }
                        }
                    }

                    WarpRoomAdded.SetValueForScreen(Context.ScreenId, true);
                    //
                    //  still need to re-add warproom
                    //  LocationsData setting only loads the warproom at the
                    //  start of the game
                    //
                    GameLocation glWarpRoom = new GameLocation(assetPath, WarpRoomLoacationName);
                    glWarpRoom.modData.Add(FEModDataKeys.FEExpansionDisplayName, "Warp Room");
                    _expansionManager.expansionManager.AddGameLocation(glWarpRoom, "AddWarpRoom");
                    AddWarpRoomCustomizations();
                    WarpRoom = glWarpRoom;
                }
            }

        }
        public void AddWarpRoomCustomizations(GameLocation warpRoom = null)
        {
            GameLocation glWarpRoom = warpRoom;
            if (glWarpRoom == null)
                glWarpRoom = Game1.getLocationFromName(WarpRoomLoacationName);

            if (glWarpRoom != null)
            {

                glWarpRoom.mapPath.Set($"SDR\\{WarpRoomLoacationName}\\warproom.tmx");
                glWarpRoom.uniqueName.Value = WarpRoomLoacationName;
                glWarpRoom.name.Value = WarpRoomLoacationName;


                //glWarpRoom.reloadMap();
                //glWarpRoom.loadObjects();

                glWarpRoom.objects.Add(vWarpRoomSignLocation, new WarpSign(vWarpRoomSignLocation, CaveEntrances ) { Name = "warpsign", Text = "Warp Room" });
                glWarpRoom.IsOutdoors = false;
                //FEFramework.ParseMapProperties(glWarpRoom.Map, glWarpRoom);
                glWarpRoom.setTileProperty(13, 1, "Buildings", "Action", "GoBuilder");
                glWarpRoom.setTileProperty(13, 0, "Buildings", "Action", "GoBuilder");
                glWarpRoom.setTileProperty(9, 1, "Buildings", "Action", "GoAnimals");
                glWarpRoom.setTileProperty(9, 0, "Buildings", "Action", "GoAnimals");
            }

        }
        public void AddWarpRoomDefinition()
        {

            logger.Log("Adding Warp Room definition", LogLevel.Debug);
            //
            //  added isplitscreen for mplayer support
            //
            if (Game1.getLocationFromName(WarpRoomLoacationName) == null && (Game1.IsMasterGame || Context.IsSplitScreen))
            {
                //
                //  add warproom map
                //
                Map warpmap = _utilitiesService.MapLoaderService.LoadMap( Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "Maps", WarpRoomLoacationName, "warproom.tmx"), "realtywarproom", false, false, false);
                string assetPath = SDVPathUtilities.AssertAndNormalizeAssetName(Path.Combine("SDR", WarpRoomLoacationName, "warproom.tmx"));

                if (ContentManager.ExternalReferences.ContainsKey(WarpRoomLoacationName))
                {
                    ContentManager.ExternalReferences.Remove(WarpRoomLoacationName);
                }
                //ContentManager.ExternalReferences.Add(WarpRoomLoacationName, warpmap);
                ContentManager.ExternalReferences.Add(assetPath, warpmap);

                //
                //  add warproom tilesheet sources
                //
                foreach (TileSheet oLayer in warpmap.TileSheets)
                {
                    oLayer.ImageSource = oLayer.ImageSource.Replace("/", "\\");
                    if (!string.IsNullOrEmpty(oLayer.ImageSource) && oLayer.ImageSource.StartsWith($"SDR{FEConstants.AssetDelimiter}{WarpRoomLoacationName}"))
                    {
                        string sFilename = Path.GetFileName(oLayer.ImageSource);
                        string sFullname = Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "Maps", WarpRoomLoacationName, sFilename);

                        ContentManager.ExternalReferences.Add($"{oLayer.ImageSource}", new StardewBitmap(sFullname).Texture());
                    }
                }
                //AddWarpRoomCustomizations();

                WarpRoomAdded.SetValueForScreen(Context.ScreenId, true);
            }
        }
        public void RemoveWarpRoom()
        {
            if (Game1.getLocationFromName(WarpRoomLoacationName) != null)
            {
                logger.Log($"   Warproom removed.", LogLevel.Debug);
                if (WarpRoom == null)
                {
                    WarpRoom = Game1.getLocationFromName(WarpRoomLoacationName);
                }
                Game1.locations.Remove(Game1.getLocationFromName(WarpRoomLoacationName));
                Game1._locationLookup.Remove(WarpRoomLoacationName);
                SetWarpRoomStatus(false);
            }
        }
        private void Player_Warped(EventArgs eParam)
        {
            WarpedEventArgs e = (WarpedEventArgs)eParam;
            if (Context.IsMultiplayer && WarpRoomInUse && e.NewLocation.Name != WarpRoomLoacationName && e.Player.UniqueMultiplayerID == PlayerInWarpRoom)
            {
                logger.Log($"Warproom unlocked", LogLevel.Debug);
                SetWarpRoomStatus(false);
            }
            else if (e.NewLocation.Name == WarpRoomLoacationName)
            {
                if (contentPackService.contentPackLoader.ValidContents.Count == 0)
                {
                    Game1.addHUDMessage(new HUDMessage(I18n.NeedPacks())
                    {
                        noIcon = true,
                        timeLeft = 3000
                    });
                    return;
                }
            }

            if (e.IsLocalPlayer)
            {

                if (e.NewLocation.Name == WarpRoomLoacationName && e.OldLocation.Name != WarpRoomLoacationName)
                {
                    //  Player warped into the Warproom
                    //
                    //  update the in/out warp to point to the location
                    //  the player came from
                    //
                    if (Context.IsMultiplayer)
                        SetWarpRoomStatus(true);

                    GameLocation warpRoom = Game1.getLocationFromName(WarpRoomLoacationName);
                    WarpSign oSign = (WarpSign)warpRoom.objects[vWarpRoomSignLocation];

                    string oldLocation = e.OldLocation.Name;
                    Vector2 oldPoint = lastPlayerPoint;
                    if (contentPackService.contentPackLoader.ValidContents.ContainsKey(oldLocation))
                    {
                        oldLocation = contentPackService.contentPackLoader.ValidContents[e.OldLocation.Name].DisplayName;
                        ExpansionPack oPack = contentPackService.contentPackLoader.ValidContents[e.OldLocation.Name];
                        oldPoint = new Vector2(oPack.CaveEntrance.WarpIn.X, oPack.CaveEntrance.WarpIn.Y);
                    }
                    //oSign.Text = oldLocation;
                    //
                    //  remove previous warp
                    //
                    Vector2 vEntrance = WarpRoomEntrancePoint;
                    Vector2 vExit = new Vector2(WarpRoomExitPoint.X, WarpRoomExitPoint.Y);
                    var oWarps = warpRoom.warps.Where(p => (p.X == vExit.X && p.Y == vExit.Y) || (p.X == vEntrance.X && p.Y == vEntrance.Y)).ToList();

                    if (oWarps.Any())
                    {
                        foreach (var p in oWarps)
                        {
                            warpRoom.warps.Remove(p);
                        }
                    }

                    warpRoom.warps.Add(new Warp((int)vExit.X, (int)vExit.Y, e.OldLocation.Name, (int)oldPoint.X, (int)oldPoint.Y, false));
                }
            }
        }
        public static bool isCollidingWithWarp_Prefix(Rectangle position, Character character, GameLocation __instance, ref Warp __result)
        {
            //
            //  intercept warps to warproom to check if the room is free
            //  if not, block the warp and pop a message
            //
            if (_expansionManager.expansionManager.farmExpansions.ContainsKey(__instance.Name))
            {
                var targetwarp = __instance.warps.Where(p => p.TargetName == WarpRoomLoacationName && (p.X == (int)Math.Floor((double)position.Left / 64.0) || p.X == (int)Math.Floor((double)position.Right / 64.0)) && (p.Y == (int)Math.Floor((double)position.Top / 64.0) || p.Y == (int)Math.Floor((double)position.Bottom / 64.0)));

                if (targetwarp.Any())
                {
                    if (WarpRoomInUse)
                    {
                        // warp busy
                        if (!warpRoomBusyPopped)
                        {
                            PopWarpRoomBusy();
                            warpRoomBusyPopped = true;
                        }
                        __result = null;
                        return false;
                    }

                    return true;
                }
                else
                {
                    warpRoomBusyPopped = false;
                    return true;
                }
            }
            else
            {
                //
                //  not an expansion pass to original code
                //
                return true;
            }
        }
        private static void PopWarpRoomBusy()
        {
            Game1.addHUDMessage(new HUDMessage(I18n.WarpInUse()) { noIcon = true, timeLeft = 2000 });
        }
        private void SetWarpRoomStatus(bool inUse)
        {
            WarpRoomInUse = inUse;
            if (inUse)
                PlayerInWarpRoom = Game1.player.UniqueMultiplayerID;
            else
                PlayerInWarpRoom = -1;

            if (Context.IsMultiplayer)
                multiplayerService.Multiplayer.SendWarpRooomStatus(inUse, PlayerInWarpRoom);
        }
        private void GameLoop_UpdateTicking(EventArgs e)
        {
            //
            //  Warp the Player to the WarpRoom
            //  update player last position before warping
            //
            if (Game1.hasLoadedGame && Config.WarpRoomKey.JustPressed())
            {
                // ignore if player is already in the warproom

                if (Game1.player.currentLocation != null && Game1.player.currentLocation.Name == WarpRoomLoacationName)
                    return;

                if (Context.IsMultiplayer && WarpRoomInUse)
                {
                    PopWarpRoomBusy();
                }
                else
                {
#if v16
                    lastPlayerPoint = Game1.player.Tile;
#else
                    lastPlayerPoint = Game1.player.getTileLocation();
#endif
                    if (Context.IsMultiplayer)
                        SetWarpRoomStatus(true);

                    Game1.warpFarmer(WarpRoomLoacationName, (int)WarpRoomEntrancePoint.X, (int)WarpRoomEntrancePoint.Y, 1);
                }
            }
        }
    }
}
