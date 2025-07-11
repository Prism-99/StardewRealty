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
using SDV_Realty_Core.ContentPackFramework.ContentPacks.ExpansionPacks;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.ServiceInterfaces;
using SDV_Realty_Core.Framework.ServiceInterfaces.Utilities;
using SDV_Realty_Core.Framework.ServiceInterfaces.Events;
using static SDV_Realty_Core.Framework.Objects.SDRMultiplayer;
using SDV_Realty_Core.Framework.ServiceInterfaces.FileManagement;
using StardewValley.Buildings;
using Netcode;
using SDV_Realty_Core.Framework.Expansions;

namespace SDV_Realty_Core.Framework.Locations
{
    /// <summary>
    /// version 1.6
    /// </summary>
    internal class WarproomManager
    {
        private enum WarpRoomStatus
        {
            Free,
            Busy
        }

        private ILoggerService logger;
        private IContentManagerService ContentManager = null;
        private IAutoMapperService autoMapperService;
#if warpV2
        public NetInt ints = new NetInt(0);
        private WarpSign? warpSign = null;
        private Vector2 meadowsShippingBinPoint = Vector2.Zero;
        private Vector2 vWarpRoomSignLocation = new Vector2(4, 25);
        public const string StardewMeadowsLoacationName = "prism99.sdr.stardewmeadows";
        public static Point EasternEntrance = new Point(88, 19);
        public const string RealtyOfficeLocationName = "prism99.sdr.realtyoffice";
        public const string GalleryLocationName = "prism99.sdr.gallery";
        public const string FarmServicesLocationName = "prism99.sdr.farmservices";
        public static FarmExpansionLocation StardewMeadows = null;
        public GameLocation RealtyOfficeInterior = null;
        public GameLocation ArtGalleryInterior = null;
        public GameLocation FarmServicesInterior = null;
        private string StardewMeadowsMapName = "stardewmeadows.tmx";
        private string RealtyOfficeInteriorMapName = "realestateinterior.tmx";
        private string FarmServicesInteriorMapName = "farmservicesinterior.tmx";
        private string GalleryInteriorMapName = "sdr.gallery.tmx";
        public static string StardewMeadowsMapAssetPath = string.Empty;
        public const string StardewMeadowsDisplayName = "Stardew Meadows";
        public readonly PerScreen<bool> WarpRoomAdded = new PerScreen<bool>();
        public static readonly Point StardewMeadowsSoutherEntrancePoint = new Point(14, 60);
        private Dictionary<long, MapWarp> PlayerReturnPoints = new();
        private List<string> MeadowsDefaultLocations = new List<string> { GalleryLocationName, StardewMeadowsLoacationName, RealtyOfficeLocationName };
#else
       private Vector2 vWarpRoomSignLocation = new Vector2(6, 3);
        public const string WarpRoomLoacationName = "sdr_warproom";
        public GameLocation WarpRoom = null;
        private string warpRoomMapName = "warproom.tmx";
        private string displayName = "Warp Room";
        public readonly PerScreen<bool> WarpRoomAdded = new PerScreen<bool>();
        public static bool WarpRoomInUse = false;
        public static long PlayerInWarpRoom;
       public static readonly Vector2 WarpRoomEntrancePoint = new Vector2(14, 11);
#endif
        public Vector2 lastPlayerPoint = Vector2.Zero;
        public string lastPlayerLocation = string.Empty;
        public readonly Vector2 WarpRoomExitPoint = new Vector2(StardewMeadowsSoutherEntrancePoint.X, StardewMeadowsSoutherEntrancePoint.Y + 1);
        private Rectangle lastWarpCheck = Rectangle.Empty;
        private static bool warpRoomBusyPopped = false;
        private IUtilitiesService utilitiesService;
        private IMultiplayerService multiplayerService;
        private static IExpansionManager expansionManager;
        private IModDataService modDataService;
        private const string warproom_status = "warp_status";
        private Action<Vector2, string, int> WarpToTheMeadows;
        public static Dictionary<string, Tuple<string, EntranceDetails>> CaveEntrances = new();

        public WarproomManager(ILoggerService errorLogger, Action<Vector2, string, int> WarpToTheMeadows, IAutoMapperService autoMapperService, IInputService inputService, IModDataService modDataService, IContentManagerService cManager, IUtilitiesService utilitiesService, IMultiplayerService multiplayerService, IExpansionManager expansionManager, ISaveManagerService saveManagerService)
        {
            logger = errorLogger;
            this.multiplayerService = multiplayerService;
            this.WarpToTheMeadows = WarpToTheMeadows;
            this.modDataService = modDataService;
            this.autoMapperService = autoMapperService;
            ContentManager = cManager;
            this.utilitiesService = utilitiesService;
            WarproomManager.expansionManager = expansionManager;

            StardewMeadowsMapAssetPath = SDVPathUtilities.AssertAndNormalizeAssetName(Path.Combine("SDR", "Expansion", StardewMeadowsLoacationName, utilitiesService.RemoveMapExtensions(StardewMeadowsMapName)));
            utilitiesService.GameEventsService.AddSubscription(IGameEventsService.EventTypes.ContentLoaded, HandleContentLoaded);
            utilitiesService.CustomEventsService.AddModEventSubscription(ICustomEventsService.ModEvents.LandSold, HandleLandSold);
            utilitiesService.GameEventsService.AddSubscription(new ReturnedToTitleEventArgs(), ReturnToTitle);
            //utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), HandlePreSaveLoad);
            //utilitiesService.GameEventsService.AddSubscription(new DayStartedEventArgs(), HandleGameSaved);
            //Config = utilitiesService.ConfigService.config;
            utilitiesService.GameEventsService.AddSubscription(new GameLaunchedEventArgs(), HandleGameLaunched, 1);
            multiplayerService.Multiplayer.AddMessageReceivedSubscription(HandleClientMessageRecevied);
            GameLocation.RegisterTouchAction(ModKeyStrings.TAction_WarpBack, HandleWarpBackTouchAction);
            GameLocation.RegisterTouchAction(ModKeyStrings.TAction_WarpRing, HandleWarpRingTouchAction);
            GameLocation.RegisterTouchAction(ModKeyStrings.TAction_ToMeadows, HandleToMeadowsTouchAction);
            inputService.AddKeyBind(modDataService.Config.WarpRoomKey, WarpKeyPressed);

            saveManagerService.AddPreSaveHook(HandlePreSave, 1);
            saveManagerService.AddPreLoadHook(HandlePreLoaded);
            saveManagerService.AddLoadedHook(AddWarpRoomCustomizations);
            saveManagerService.AddPostSaveHook(AddWarpRoomCustomizations);
        }
        private void HandleToMeadowsTouchAction(GameLocation location, string[] args, Farmer who, Vector2 pos)
        {
            PlayerReturnPoints[Game1.player.UniqueMultiplayerID] = new MapWarp
            {
                ToMap = location.Name,
                ToX = (int)pos.X,
                ToY = (int)pos.Y

            };
            WarpToTheMeadows(Game1.player.Tile, Game1.player.currentLocation.Name, 0);
        }
        private void HandleWarpRingTouchAction(GameLocation location, string[] args, Farmer who, Vector2 pos)
        {
            if (who.facingDirection.Value == 0)
            {
                if (warpSign?.CurrentWarp == null)
                {
                    MapWarp warpBack = utilitiesService.GetPlayerHomeWarp();
                    Game1.warpFarmer(warpBack.ToMap, warpBack.ToX, warpBack.ToY, 1);
                }
                else
                {
                    Game1.warpFarmer(warpSign.CurrentWarp.TargetName, warpSign.CurrentWarp.TargetX, warpSign.CurrentWarp.TargetY, 2);
                }
            }
        }

        private void WarpKeyPressed(KeybindList key)
        {
            WarpKeyPressed();
        }
        private void AddLocationData()
        {
            //StardewMeadowsMapAssetPath = SDVPathUtilities.AssertAndNormalizeAssetName(Path.Combine("SDR", "Expansion", StardewMeadowsLoacationName, utilitiesService.RemoveMapExtensions(StardewMeadowsMapName)));
            // add map and tilesheet data to ExternalReferences
            if (utilitiesService.TryAddGameLocation(StardewMeadowsLoacationName, StardewMeadowsMapName, StardewMeadowsDisplayName, ContentManager, expansionManager, null, out GameLocation gamelocation, out Map map, IUtilitiesService.LocationType.Vanilla, false))
            {
                //
                //  add stations
                //
                var mapData = autoMapperService.ParseMap(map);
                if (mapData.TrainStationIn.HasValue)
                {
                    // add pre vanilla train station open price
                    modDataService.Stations.Add(new StationDetails
                    {
                        DisplayName = $"{StardewMeadowsDisplayName} - {I18n.TrainStation()}",
                        LocationName = StardewMeadowsLoacationName,
                        InPoint = mapData.TrainStationIn.Value,
                        FacingDirection = Game1.down,
                        Type = StationDetails.StationType.Train,
                        Network = "Train",
                        Condition = "DAYS_PLAYED 0 30, LOCATION_NAME Here Pathoschild.CentralStation_CentralStation",
                        Cost = 20,
                        Key = $"{StardewMeadowsLoacationName}.train.c1"
                    });
                    // add post vanilla train station open price
                    modDataService.Stations.Add(new StationDetails
                    {
                        DisplayName = $"{StardewMeadowsDisplayName} - {I18n.TrainStation()}",
                        LocationName = StardewMeadowsLoacationName,
                        InPoint = mapData.TrainStationIn.Value,
                        FacingDirection = Game1.down,
                        Type = StationDetails.StationType.Train,
                        Network = "Train",
                        Condition = "DAYS_PLAYED 31, LOCATION_NAME Here Pathoschild.CentralStation_CentralStation",
                        Cost = 0,
                        Key = $"{StardewMeadowsLoacationName}.train.c2"
                    });
                }
                if (mapData.BoatDockIn.HasValue)
                {
                    //  add pre train price of 20
                    modDataService.Stations.Add(new StationDetails
                    {
                        DisplayName = $"{StardewMeadowsDisplayName} - {I18n.BoatDock()}",
                        LocationName = StardewMeadowsLoacationName,
                        InPoint = mapData.BoatDockIn.Value,
                        FacingDirection = Game1.down,
                        Type = StationDetails.StationType.Boat,
                        Network = "Boat",
                        Condition = "DAYS_PLAYED 0 30, LOCATION_NAME Here Pathoschild.CentralStation_CentralStation",
                        Cost = 20,
                        Key = $"{StardewMeadowsLoacationName}.boat.c1"
                    });
                    // add post train free ride
                    modDataService.Stations.Add(new StationDetails
                    {
                        DisplayName = $"{StardewMeadowsDisplayName} - {I18n.BoatDock()}",
                        LocationName = StardewMeadowsLoacationName,
                        InPoint = mapData.BoatDockIn.Value,
                        FacingDirection = Game1.down,
                        Type = StationDetails.StationType.Boat,
                        Network = "Boat",
                        Condition = "DAYS_PLAYED 31, LOCATION_NAME Here Pathoschild.CentralStation_CentralStation",
                        Cost = 0,
                        Key = $"{StardewMeadowsLoacationName}.boat.c2"
                    });
                }
                if (mapData.BusIn.HasValue)
                {
                    modDataService.Stations.Add(new StationDetails
                    {
                        DisplayName = $"{StardewMeadowsDisplayName} - {I18n.BusStop()}",
                        LocationName = StardewMeadowsLoacationName,
                        InPoint = mapData.BusIn.Value,
                        FacingDirection = Game1.down,
                        Type = StationDetails.StationType.Bus,
                        Network = "Bus",
                        Condition = "LOCATION_NAME Here Pathoschild.CentralStation_CentralStation"
                    });
                }
                if (mapData.ShippingBinLocation.HasValue)
                {
                    meadowsShippingBinPoint = mapData.ShippingBinLocation.Value;
                }
            }
        }
        private void HandleLandSold(object[] args)
        {
#if DEBUG
            logger.Log($"Removing cave entrance for '{args[0]}'", LogLevel.Debug);
#endif
            if (args.Length == 1)
                CaveEntrances.Remove(args[0].ToString());
        }
        //private void HandleGameSaved(EventArgs e)
        //{
        //    AddWarpRoomCustomizations();

        //}
        private void PatchInWarpRing()
        {
            string patchPath = Path.Combine(utilitiesService.GameEnvironment.ModPath, "data", "Maps", StardewMeadowsLoacationName, "ring_patch.tmx").Replace(utilitiesService.GameEnvironment.GamePath, "");
            Map warpRing = utilitiesService.MapLoaderService.LoadMap(patchPath, "", false);
            GameLocation glMeadows = Game1.getLocationFromName(StardewMeadowsLoacationName);

            int width = warpRing.GetLayer("Back").LayerWidth;
            int height = warpRing.GetLayer("Back").LayerHeight;
            //glMeadows.ApplyMapOverride(warpRing, "sdr.warproom", new Rectangle(3, 25, width, height), new Rectangle(0, 0, width, height));

            utilitiesService.MapUtilities.OverlayMap(glMeadows, warpRing, new Point(3, 25));
        }
        /// <summary>
        /// Load required pre-load elements
        /// </summary>
        private void HandlePreLoaded()
        {
            LoadArtGallery();
            AddRealEstateOffice();
            AddFarmServices();
            AddHomeWarpRoomLocations();
            //AddStardewMeadows();
            StardewMeadows = (FarmExpansionLocation)Game1.getLocationFromName(StardewMeadowsLoacationName);

            if (Context.IsMainPlayer)
                AddWarpRoomCustomizations();
        }
        /// <summary>
        /// Load Art Gallery location before the save is loaded
        /// Art Gallery is stored in the game save
        /// </summary>
        /// <param name="e"></param>
        //private void HandlePreSaveLoad(EventArgs e)
        //{
        //    LoadStageChangedEventArgs stage = (LoadStageChangedEventArgs)e;
        //    if (stage.NewStage == LoadStage.CreatedInitialLocations || stage.NewStage == LoadStage.SaveAddedLocations)
        //    {
        //        PreSaveLoaded();
        //    }
        //}

        private void LoadArtGallery()
        {
            if (ArtGalleryInterior != null)
            {
                expansionManager.expansionManager.AddGameLocation(ArtGalleryInterior, "AddWarpRoom");
            }
            else
            {
                if (utilitiesService.TryAddGameLocation(GalleryLocationName, GalleryInteriorMapName, "Art Gallery", ContentManager, expansionManager, null, out ArtGalleryInterior, out _, IUtilitiesService.LocationType.Decoratable))
                {
                    //WarpRoomAdded.SetValueForScreen(Context.ScreenId, true);
                    //AddWarpRoomCustomizations();
                }
            }
        }
        private bool HandleClientMessageRecevied(bool masterPlayer, ModMessageReceivedEventArgs e)
        {
            bool handled = false;
            switch (e.Type)
            {
                case warproom_status:
                    //
                    //  update current usage of the warproom
                    //
                    FEMessage warp = e.ReadAs<FEMessage>();
#if !warpV2
                        switch ((WarpRoomStatus)warp.Code)
                        {
                            case WarpRoomStatus.Busy:
                                // warp room in use
                                WarpRoomInUse = true;
                                PlayerInWarpRoom = warp.Player;
                                break;
                            case WarpRoomStatus.Free:
                                //  warp room free
                                WarpRoomInUse = false;
                                PlayerInWarpRoom = -1;
                                break;
                        }
#endif
                    break;
            }
            return handled;
        }
        public void SendWarpRooomStatus(bool inUse, long userId)
        {
            FEMessage fEMessage = new FEMessage()
            {
                Player = userId,
                Code = (int)(inUse ? WarpRoomStatus.Busy : WarpRoomStatus.Free)
            };
            multiplayerService.Multiplayer.SendMessageToClient(fEMessage, warproom_status, -1);

        }
        private void HandleContentLoaded()
        {
            //
            //  only add warproom if any expansion loaded
            //
            if (utilitiesService.HaveExpansions)
            {
                utilitiesService.GameEventsService.AddSubscription("WarpedEventArgs", HandlePlayerWarped);
                //utilitiesService.GameEventsService.AddSubscription(new UpdateTickingEventArgs(), GameLoop_UpdateTicking);
                //utilitiesService.GameEventsService.AddSubscription(new SaveLoadedEventArgs(), GameLoop_SaveLoaded);
                //utilitiesService.GameEventsService.AddSubscription(new SavedEventArgs(), GameLoop_SaveLoaded);
                //utilitiesService.GameEventsService.AddSubscription(new LoadStageChangedEventArgs(0, 0), LoadStageChanged);
                //utilitiesService.GameEventsService.AddSubscription(IGameEventsService.EventTypes.PreSave, HandlePreSave);
                utilitiesService.CustomEventsService.AddCustomSubscription("ClientDisconnected", ClientDisconnected);
#if !warpV2
                //
                // warp room patch
                //
                _utilitiesService.PatchingService.patches.AddPatch(true, typeof(GameLocation), "isCollidingWithWarp",
                    new Type[] { typeof(Rectangle), typeof(Character) }, typeof(WarproomManager), nameof(isCollidingWithWarp_Prefix),
                    "Check warproom availability.",
                    "Warps");

                _utilitiesService.PatchingService.ApplyPatches("Warps");
#endif
            }
        }
        /// <summary>
        /// Handle PreSave event and remove WarpRoom from game
        /// </summary>
        private void HandlePreSave()
        {

            GameLocation glWarpRoom = Game1.getLocationFromName(StardewMeadowsLoacationName);

            glWarpRoom.objects.Remove(vWarpRoomSignLocation);

            RemoveWarpRoom();

        }
        private void ClientDisconnected(object[] args)
        {
            //
            //  unlock warproom if disconnected player had it locked
            //
#if !warpV2
            if (WarpRoomInUse && PlayerInWarpRoom == (long)args[0])
            {
                SetWarpRoomStatus(false);
            }
#endif
        }
        //        private void GameLoop_SaveLoaded(EventArgs e)
        //        {
        //#if !warpV2
        //            AddHomeWarpRoomLocations();
        //            AddWarpRoom();
        //#endif
        //        }       
        internal void AddCaveEntrance(string location, Tuple<string, EntranceDetails> position)
        {
            CaveEntrances.Add(location, position);
        }
        private void HandleGameLaunched(EventArgs eventArgs)
        {
            AddLocationData();
        }
        private void ReturnToTitle(EventArgs eventArgs)
        {
            CaveEntrances.Clear();
            StardewMeadows = null;
        }
        //private void LoadStageChanged(EventArgs ep)
        //{
        //    LoadStageChangedEventArgs e = (LoadStageChangedEventArgs)ep;
        //    if (e.NewStage == LoadStage.SaveAddedLocations || e.NewStage == LoadStage.CreatedInitialLocations)
        //    {
        //        // need to find better event or change AddCaveEntrance call time
        //        //CaveEntrances.Clear();
        //    }
        //}
        internal void AddHomeWarpRoomLocations()
        {
            //
            //  add warp home
            //
            MapWarp oHomeWarp = utilitiesService.GetPlayerHomeWarp();

            if (oHomeWarp != null && !CaveEntrances.ContainsKey(oHomeWarp.ToMap))
                CaveEntrances.Add(oHomeWarp.ToMap, Tuple.Create(oHomeWarp.ToMap, new EntranceDetails { WarpIn = new EntranceWarp { X = oHomeWarp.ToX, Y = oHomeWarp.ToY }, WarpOut = new EntranceWarp { X = oHomeWarp.FromX, Y = oHomeWarp.FromY } }));

        }
        private void AddRealEstateOffice()
        {
            utilitiesService.TryAddGameLocation(RealtyOfficeLocationName, RealtyOfficeInteriorMapName, "Realty Office", ContentManager, expansionManager, null, out RealtyOfficeInterior, out _);
            //RealtyOfficeInterior.parentLocationName.Value = StardewMeadowsLoacationName;
        }
        private void AddFarmServices()
        {
            utilitiesService.TryAddGameLocation(FarmServicesLocationName, FarmServicesInteriorMapName, "Realty Office", ContentManager, expansionManager, null, out FarmServicesInterior, out _);
        }
        public void AddStardewMeadows()
        {

#if DEBUG
            logger.LogDebug("Adding Stardew Meadows");
            logger.LogDebug($"  Game Location loaded: {Game1.getLocationFromName(StardewMeadowsLoacationName) != null}");
            logger.LogDebug($"  StardewMeadows()\r\n        {{ cached: {StardewMeadows != null}");
            logger.LogDebug($"  Master game: {Game1.IsMasterGame}");
            logger.LogDebug($"  Split screen: {Context.IsSplitScreen}");
#endif
            //
            //  added isplitscreen for mplayer support
            //
            if (!Game1.locations.Where(p => p.Name == StardewMeadowsLoacationName).Any())// && (Game1.IsMasterGame || Context.IsSplitScreen))
            {
                //
                //  add real estate office location
                //
                //if (RealtyOfficeInterior != null)
                //{
                //    expansionManager.expansionManager.AddGameLocation(RealtyOfficeInterior, "AddWarpRoom");
                //}
                //else
                //{
                //    if (utilitiesService.TryAddGameLocation(RealtyOfficeLocationName, RealtyOfficeInteriorMapName, "Realty Office", ContentManager, expansionManager, out RealtyOfficeInterior))
                //    {
                //        //WarpRoomAdded.SetValueForScreen(Context.ScreenId, true);
                //        //AddWarpRoomCustomizations();
                //    }
                //}


                //
                //  add Stardew Meadows map
                //
                if (StardewMeadows != null)
                {
                    expansionManager.expansionManager.AddGameLocation(StardewMeadows, "AddStardewMeadows");
                }
                else
                {
                    //    if (utilitiesService.TryAddGameLocation(StardewMeadowsLoacationName, StardewMeadowsMapName, StardewMeadowsDisplayName, ContentManager, expansionManager,null, out StardewMeadows, out _,IUtilitiesService.LocationType.FarmExpansion))
                    //    {
                    //          //Traverse.Create(StardewMeadows).Method("initNetFields");
                    //        WarpRoomAdded.SetValueForScreen(Context.ScreenId, true);
                    //        //AddWarpRoomCustomizations();
                    //    }
                    //Map warpmap = _utilitiesService.MapLoaderService.LoadMap(Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "Maps", WarpRoomLoacationName, warpRoomMapName).Replace(_utilitiesService.GameEnvironment.GamePath, ""), WarpRoomLoacationName, false, false);
                    //string assetPath = SDVPathUtilities.AssertAndNormalizeAssetName(Path.Combine("SDR", WarpRoomLoacationName, warpRoomMapName));
                    ////
                    ////  this is all loaded by AddWarproomDefinition
                    ////
                    ////ContentManager.ExternalReferences.Add(assetPath, warpmap);
                    //if (ContentManager.ExternalReferences.ContainsKey(assetPath))
                    //{
                    //    ContentManager.ExternalReferences.Remove(assetPath);
                    //}
                    //ContentManager.ExternalReferences.Add(assetPath, warpmap);
                    //////
                    //////  add warproom tilesheet sources
                    //////
                    //foreach (TileSheet oLayer in warpmap.TileSheets)
                    //{
                    //    oLayer.ImageSource = oLayer.ImageSource.Replace("\\", FEConstants.AssetDelimiter);
                    //    if (!string.IsNullOrEmpty(oLayer.ImageSource) && oLayer.ImageSource.StartsWith($"SDR{FEConstants.AssetDelimiter}{WarpRoomLoacationName}"))
                    //    {
                    //        string sFilename = Path.GetFileName(oLayer.ImageSource);
                    //        string sFullname = Path.Combine(_utilitiesService.GameEnvironment.ModPath, "data", "Maps", WarpRoomLoacationName, sFilename);

                    //        if (!ContentManager.ExternalReferences.ContainsKey(oLayer.ImageSource))
                    //        {
                    //            ContentManager.ExternalReferences.Add(oLayer.ImageSource, new StardewBitmap(sFullname).Texture());
                    //        }
                    //    }
                    //}

                    //WarpRoomAdded.SetValueForScreen(Context.ScreenId, true);
                    ////
                    ////  still need to re-add warproom
                    ////  LocationsData setting only loads the warproom at the
                    ////  start of the game
                    ////
                    //GameLocation glWarpRoom = new GameLocation(assetPath, WarpRoomLoacationName);
                    //glWarpRoom.modData.Add(IModDataKeysService.FEExpansionDisplayName, displayName);
                    //_expansionManager.expansionManager.AddGameLocation(glWarpRoom, "AddWarpRoom");
                    //AddWarpRoomCustomizations();
                    //WarpRoom = glWarpRoom;
                }
            }
            else if (StardewMeadows == null)
            {
                StardewMeadows = (FarmExpansionLocation)Game1.locations.Where(p => p.Name == StardewMeadowsLoacationName).FirstOrDefault();
            }
            if (StardewMeadows != null)
            {
                //
                //  when drone or train is active other player
                //  gets a ton of red
                //  'invalid state'
                //
                //CustomTrainService.train = StardewMeadows.NetFields;
                //CustomTrainService.trainTimer.Parent = StardewMeadows.NetFields;
                //StardewMeadows.NetFields.AddField(CustomTrainService.train).AddField(CustomTrainService.trainTimer);
                //StardewMeadows.NetFields.AddField(ints, "ints");
                //StardewMeadows.NetFields.AddField(CustomTrainService.trainTimer, "trainTimer");
                //int xx = 1;
            }

        }
        /// <summary>
        /// Handle sdr.warpback TouchAction
        /// returns the player back to the location they
        /// entered stardew meadows from
        /// </summary>
        /// <param name="location"></param>
        /// <param name="args"></param>
        /// <param name="who"></param>
        /// <param name="pos"></param>
        private void HandleWarpBackTouchAction(GameLocation location, string[] args, Farmer who, Vector2 pos)
        {
            MapWarp warpBack;
            if (!PlayerReturnPoints.TryGetValue(who.UniqueMultiplayerID, out warpBack))
            {
                warpBack = utilitiesService.GetPlayerHomeWarp();
                //warpBack = new Warp(0, 0, "Town", 10, 10, false);
            }
            Game1.warpFarmer(warpBack.ToMap, warpBack.ToX, warpBack.ToY, 1);
        }
        public void AddWarpRoomCustomizations()
        {
            FarmExpansionLocation glWarpRoom = (FarmExpansionLocation)Game1.getLocationFromName(StardewMeadowsLoacationName);
            if (glWarpRoom == null)
            {
                glWarpRoom = StardewMeadows;
                if (glWarpRoom != null)
                    Game1.locations.Add(StardewMeadows);
            }
            if (glWarpRoom != null)
            {
                glWarpRoom.utilitiesService = utilitiesService;
                if (modDataService.Config.UseWarpRing)
                {
                    //GameLocation glWarpRoom = warpRoom;
                    //if (glWarpRoom == null)

                    //glWarpRoom.mapPath.Set($"SDR\\{WarpRoomLoacationName}\\{warpRoomMapName}");
                    //glWarpRoom.uniqueName.Value = WarpRoomLoacationName;
                    //glWarpRoom.name.Value = WarpRoomLoacationName;


                    //glWarpRoom.reloadMap();
                    //glWarpRoom.loadObjects();
                    if (!glWarpRoom.objects.ContainsKey(vWarpRoomSignLocation))
                    {
                        logger.Log($"Adding WarpSign at {vWarpRoomSignLocation}", LogLevel.Debug);
                        if (warpSign == null)
                        {
                            warpSign = new WarpSign(vWarpRoomSignLocation, CaveEntrances) { Name = "warpsign", Text = "Warp Room" };
                        }
                        glWarpRoom.objects.Add(vWarpRoomSignLocation, warpSign);
                    }
                    PatchInWarpRing();
                }
                //
                //  add shipping bin
                //
                if (!glWarpRoom.buildings.Where(p => p.buildingType.Value == "Shipping Bin").Any())
                {
                    Building building = Building.CreateInstanceFromId("Shipping Bin", meadowsShippingBinPoint);
                    building.load();
                    glWarpRoom.buildings.Add(building);
                }
            }
#if !warpV2
                glWarpRoom.IsOutdoors = false;
                //FEFramework.ParseMapProperties(glWarpRoom.Map, glWarpRoom);
                glWarpRoom.setTileProperty(13, 1, "Buildings", "Action", "GoBuilder");
                glWarpRoom.setTileProperty(13, 0, "Buildings", "Action", "GoBuilder");
                glWarpRoom.setTileProperty(9, 1, "Buildings", "Action", "GoAnimals");
                glWarpRoom.setTileProperty(9, 0, "Buildings", "Action", "GoAnimals");
#endif


        }
        //public void AddWarpRoomDefinition()
        //{

        //    logger.LogDebug("Adding Warp Room definition");
        //    //
        //    //  added isplitscreen for mplayer support
        //    //
        //    if ((Game1.IsMasterGame || Context.IsSplitScreen) && Game1.getLocationFromName(StardewMeadowsLoacationName) == null)
        //    {
        //        //
        //        //  add warproom map
        //        //
        //        Map warpmap = utilitiesService.MapLoaderService.LoadMap(Path.Combine(utilitiesService.GameEnvironment.ModPath, "data", "Maps", StardewMeadowsLoacationName, StardewMeadowsMapName), StardewMeadowsLoacationName, false, false, false);
        //        string assetPath = SDVPathUtilities.AssertAndNormalizeAssetName(Path.Combine("SDR", StardewMeadowsLoacationName, StardewMeadowsMapName));

        //        if (ContentManager.ExternalReferences.ContainsKey(StardewMeadowsLoacationName))
        //        {
        //            ContentManager.ExternalReferences.Remove(StardewMeadowsLoacationName);
        //        }
        //        //ContentManager.ExternalReferences.Add(WarpRoomLoacationName, warpmap);
        //        ContentManager.ExternalReferences.Add(assetPath, warpmap);

        //        //
        //        //  add warproom tilesheet sources
        //        //
        //        foreach (TileSheet oLayer in warpmap.TileSheets)
        //        {
        //            oLayer.ImageSource = oLayer.ImageSource.Replace("/", "\\");
        //            if (!string.IsNullOrEmpty(oLayer.ImageSource) && oLayer.ImageSource.StartsWith($"SDR{FEConstants.AssetDelimiter}{StardewMeadowsLoacationName}"))
        //            {
        //                string sFilename = Path.GetFileName(oLayer.ImageSource);
        //                string sFullname = Path.Combine(utilitiesService.GameEnvironment.ModPath, "data", "Maps", StardewMeadowsLoacationName, sFilename);

        //                ContentManager.ExternalReferences.Add($"{oLayer.ImageSource}", new StardewBitmap(sFullname).Texture());
        //                if (Path.GetFileName(oLayer.ImageSource) != Path.GetFileNameWithoutExtension(oLayer.ImageSource))
        //                {
        //                    int end = oLayer.ImageSource.LastIndexOf('.');
        //                    if (end > -1)
        //                    {
        //                        ContentManager.ExternalReferences.Add(oLayer.ImageSource.Substring(0, end - 1), new StardewBitmap(sFullname).Texture());
        //                    }
        //                }
        //            }
        //        }
        //        //AddWarpRoomCustomizations();

        //        WarpRoomAdded.SetValueForScreen(Context.ScreenId, true);
        //    }
        //}
        public void RemoveWarpRoom()
        {
            if (Game1.getLocationFromName(StardewMeadowsLoacationName) != null)
            {
                logger.LogDebug($"   Warproom removed.");
                if (StardewMeadows == null)
                {
                    StardewMeadows = (FarmExpansionLocation)Game1.getLocationFromName(StardewMeadowsLoacationName);
                }
                Game1.locations.Remove(Game1.getLocationFromName(StardewMeadowsLoacationName));
                Game1._locationLookup.Remove(StardewMeadowsLoacationName);
#if !warpV2
                SetWarpRoomStatus(false);
#endif
            }
            //if (Game1.getLocationFromName(RealtyOfficeLocationName) != null)
            //{
            //    logger.LogDebug($"   Realty Office removed.");
            //    if (RealtyOfficeInterior == null)
            //    {
            //        RealtyOfficeInterior = Game1.getLocationFromName(RealtyOfficeLocationName);
            //    }
            //    Game1.locations.Remove(Game1.getLocationFromName(RealtyOfficeLocationName));
            //    Game1._locationLookup.Remove(RealtyOfficeLocationName);
            //}
        }
        private void HandlePlayerWarped(EventArgs eParam)
        {
            WarpedEventArgs e = (WarpedEventArgs)eParam;
#if DEBUG
            logger.Log($"player warped: {e.OldLocation.Name},{e.NewLocation.Name}", LogLevel.Debug);
#endif
#if !warpV2
            if (Context.IsMultiplayer && WarpRoomInUse && e.NewLocation.Name != WarpRoomLoacationName && e.Player.UniqueMultiplayerID == PlayerInWarpRoom)
            {
                logger.Log($"Warproom unlocked", LogLevel.Debug);
                SetWarpRoomStatus(false);
                return;
            }

            if (e.NewLocation.Name == StardewMeadowsLoacationName)
            {
                if (contentPackService.contentPackLoader.ValidContents.Count == 0)
                {
                    utilitiesService.PopHUDMessage(I18n.NeedPacks(),0,3000);
                    return;
                }
            }
#endif
            if (e.IsLocalPlayer)
            {
                //
                //  skip warping to meadows buildings
                //
                if ((e.OldLocation.GetParentLocation() != null && e.OldLocation.GetParentLocation().Name == StardewMeadowsLoacationName) || (MeadowsDefaultLocations.Contains(e.NewLocation.Name) && MeadowsDefaultLocations.Contains(e.OldLocation.Name)))
                    return;

                if (e.NewLocation.Name == StardewMeadowsLoacationName && e.OldLocation.Name != StardewMeadowsLoacationName)
                {
                    //  Player warped into the Meadows
                    //
                    //  update the in/out warp to point to the location
                    //  the player came from
                    //
#if warpV2
                    // add players return point to player return dictionary
                    Vector2 oldPoint = lastPlayerPoint;
                    //string oldLocation = e.OldLocation.Name;
                    if (oldPoint == Vector2.Zero && modDataService.validContents.ContainsKey(e.OldLocation.Name))
                    {
                        //oldLocation = contentPackService.contentPackLoader.ValidContents[e.OldLocation.Name].DisplayName;
                        ExpansionPack oPack = modDataService.validContents[e.OldLocation.Name];
                        oldPoint = new Vector2(oPack.CaveEntrance.WarpIn.X, oPack.CaveEntrance.WarpIn.Y);
                    }

                    if (lastPlayerPoint != Vector2.Zero)
                    {
                        lastPlayerPoint = Vector2.Zero;

                        PlayerReturnPoints[e.Player.UniqueMultiplayerID] = new MapWarp
                            (
                            0, 0, e.OldLocation.NameOrUniqueName, (int)oldPoint.X, (int)oldPoint.Y, false
                            );
                    }

#if DEBUG
                    //logger.Log($"PlayerReturnPoints={PlayerReturnPoints[e.Player.UniqueMultiplayerID]}", LogLevel.Debug);
#endif


#else
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
                        foreach (Warp p in oWarps)
                        {
                            warpRoom.warps.Remove(p);
                        }
                    }

                    warpRoom.warps.Add(new Warp((int)vExit.X, (int)vExit.Y, e.OldLocation.Name, (int)oldPoint.X, (int)oldPoint.Y, false));
#endif
                }
            }
        }
#if !warpV2
        public static bool isCollidingWithWarp_Prefix(Rectangle position, Character character, GameLocation __instance, ref Warp __result)
        {
            //
            //  intercept warps to warproom to check if the room is free
            //  if not, block the warp and pop a message
            //
            if (_expansionManager.expansionManager.farmExpansions.ContainsKey(__instance.Name))
            {
                IEnumerable<Warp> targetwarp = __instance.warps.Where(p => p.TargetName == WarpRoomLoacationName && (p.X == (int)Math.Floor((double)position.Left / 64.0) || p.X == (int)Math.Floor((double)position.Right / 64.0)) && (p.Y == (int)Math.Floor((double)position.Top / 64.0) || p.Y == (int)Math.Floor((double)position.Bottom / 64.0)));

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
                SendWarpRooomStatus(inUse, PlayerInWarpRoom);
        }
#endif
        private void WarpKeyPressed()
        {
            if (Game1.player.currentLocation != null && Game1.player.currentLocation.Name == StardewMeadowsLoacationName)
                return;
#if warpV2
            WarpToTheMeadows(Game1.player.Tile, Game1.player.currentLocation.Name, 0);
            //lastPlayerPoint = Game1.player.Tile;
            //lastPlayerLocation = Game1.player.currentLocation.Name;
            //Game1.warpFarmer(StardewMeadowsLoacationName, (int)WarpRoomEntrancePoint.X, (int)WarpRoomEntrancePoint.Y, 1);
#else
                if (Context.IsMultiplayer && WarpRoomInUse)
                {
                    PopWarpRoomBusy();
                }
                else
                {
                    lastPlayerPoint = Game1.player.Tile;
                    if (Context.IsMultiplayer)
                        SetWarpRoomStatus(true);


                Game1.warpFarmer(WarpRoomLoacationName, (int)WarpRoomEntrancePoint.X, (int)WarpRoomEntrancePoint.Y, 1);
                }
#endif
        }
        //private void GameLoop_UpdateTicking(EventArgs e)
        //{
        //    //
        //    //  Warp the Player to the WarpRoom
        //    //  update player last position before warping
        //    //
        //    if (Game1.hasLoadedGame && Config.WarpRoomKey.JustPressed())
        //    {
        //        // ignore if player is already in the warproom
        //        WarpKeyPressed();
        //    }
        //}

    }
}
