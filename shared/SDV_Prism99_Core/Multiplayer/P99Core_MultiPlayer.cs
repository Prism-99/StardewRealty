using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewValley.Network;
using Prism99_Core.Utilities;
using HarmonyLib;
using SDV_Realty_Core.Framework.Objects;


namespace Prism99_Core.MultiplayerUtils
{
    internal static class P99Core_MultiPlayer
    {

        private static Multiplayer mPlayer = null;
        private static IModHelper helper;
        private static SDVLogger logger;
        private static SDVLogger networkLog;
        public static List<string> NetworkLog = new List<string>();
        private static List<Action<PeerContextReceivedEventArgs>> PeerContextReceivedCallbacks = new List<Action<PeerContextReceivedEventArgs>>();
        private static List<Action<PeerConnectedEventArgs>> PeerConnectedCallbacks = new List<Action<PeerConnectedEventArgs>>();
        private static List<Action<PeerDisconnectedEventArgs>> PeerDisconnectedCallbacks = new List<Action<PeerDisconnectedEventArgs>>();
        private static List<Action<ModMessageReceivedEventArgs>> ModMessageReceivedCallbacks = new List<Action<ModMessageReceivedEventArgs>>();
        private static string LogPrefix = "[MP] ";
        private static bool initialized = false;
        public static bool IsMultiplayer { get { return Game1.IsMultiplayer || Context.ScreenId > 0; } }
        public static bool IsMasterGame
        {
            get
            {
                return Context.IsMainPlayer && Context.ScreenId == 0;
            }
        }
        public static bool IsSplitScreen { get; private set; }
        public static IGameServer GameServer { get; private set; }

        public static void Initialize(IModHelper oHelper, SDVLogger ologger)
        {
            //ologger.Log($"{initialized} Initialize Stack: {Environment.StackTrace}", LogLevel.Debug);
            if (initialized) return;
            helper = oHelper;
            mPlayer = oHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            logger = ologger;
            networkLog = new SDVLogger(null, oHelper.DirectoryPath, helper, false, "network.log");
            GameServer = Game1.server;


             IsSplitScreen = Context.IsSplitScreen;

            helper.Events.Multiplayer.PeerConnected += Multiplayer_PeerConnected;
            helper.Events.Multiplayer.PeerContextReceived += Multiplayer_PeerContextReceived;
            helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;
            helper.Events.Multiplayer.PeerDisconnected += Multiplayer_PeerDisconnected;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            initialized = true;
        }
        private static void GetGameServer()
        {
            if (GameServer == null)
            {
                GameServer = Game1.server;
            }
            if (mPlayer == null)
            {
                mPlayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            }
        }
        public static void AddAlwaysActiveLocations(GameLocation location)
        {
            GetGameServer();


            if (!mPlayer.activeLocations().Where(p => p.NameOrUniqueName == location.NameOrUniqueName).Any())
            {
                mPlayer.activeLocations().AddItem(location);
            }
        }
        public static List<GameLocation> GetAlwaysActiveLocations()
        {
            List<GameLocation> list = new List<GameLocation> { };

            GetGameServer();

            list = mPlayer.activeLocations().ToList();

            return list;
        }
        public static Dictionary<long, NetPlayer> GetNetPlayers(IModHelper helper)
        {
            Dictionary<long, NetPlayer> ret = new Dictionary<long, NetPlayer>();

            foreach (IMultiplayerPeer oPlayer in helper.Multiplayer.GetConnectedPlayers())
            {
                ret.Add(oPlayer.PlayerID, new NetPlayer(oPlayer, helper));
            }
            foreach (Farmer farmer in Game1.getAllFarmhands())
            {
                var tplayer = helper.Multiplayer.GetConnectedPlayers().Where(p => p.PlayerID == farmer.UniqueMultiplayerID);

                if (!tplayer.Any())
                {
                    ret.Add(farmer.UniqueMultiplayerID, new NetPlayer(farmer, helper));
                }
            }

            return ret;
        }
        public static string GetIdName(long playerId)
        {
            var owner = Game1.getAllFarmhands().Where(p => p.UniqueMultiplayerID == playerId).FirstOrDefault();

            if (owner == null)
            {
                return string.Empty;
            }
            else
            {
                return owner.Name;
            }
        }
        public static void EvalStatus()
        {
            //IsMultiplayer = Game1.IsMultiplayer;
            //IsMasterGame = Game1.IsMasterGame;
            IsSplitScreen = Context.IsSplitScreen;
            GetGameServer();
        }
        public static void AddNetLogMessage(string message)
        {
#if DEBUG
            networkLog.Log(message, LogLevel.Debug);
            var dt = DateTime.Now;
            string time = dt.ToString("yyyy-MM-dd") + " " + dt.ToString("HH:mm");

            NetworkLog.Add($"[{time}] {message}");
#endif
        }
        public static void SendMessage(object message, string action, long clientId = -1)
        {
            string msg = $"Sending to {clientId}: [{action}] {message}";
            if(message is SDRMultiplayer.FEMessage feMsg)
            {
                msg = $"Sending to {clientId}: [{action}] {feMsg.Code}:{feMsg.Data}";
            }
            AddNetLogMessage(msg);

            logger.Log($"{LogPrefix}{msg}", LogLevel.Debug);
            if (clientId == -1)
            {
                helper.Multiplayer.SendMessage(message, action, new string[] { helper.ModRegistry.ModID });
            }
            else
            {
                helper.Multiplayer.SendMessage(message, action, new string[] { helper.ModRegistry.ModID }, new long[] { clientId });
            }
        }
        public static void Add_PeerContextReceivedCallback(Action<PeerContextReceivedEventArgs> callback)
        {
            logger.Log($"{LogPrefix}Adding PeerContextReceivedCallback", LogLevel.Debug);
            PeerContextReceivedCallbacks.Add(callback);
        }
        public static void Add_PeerConnectedCallback(Action<PeerConnectedEventArgs> callback)
        {
            logger.Log($"{LogPrefix}Adding PeerConnectedCallback", LogLevel.Debug);
            PeerConnectedCallbacks.Add(callback);
        }
        public static void Add_PeerDisconnectedCallback(Action<PeerDisconnectedEventArgs> callback)
        {
            logger.Log($"{LogPrefix}Adding PeerDisconnectedCallback", LogLevel.Debug);
            PeerDisconnectedCallbacks.Add(callback);
        }
        public static void Add_ModMessageReceivedCallback(Action<ModMessageReceivedEventArgs> callback)
        {
            logger.Log($"{LogPrefix}Adding ModMessageReceivedCallback", LogLevel.Debug);
            ModMessageReceivedCallbacks.Add(callback);
        }
        private static void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            //if (!Game1.IsMasterGame)
            //{
            //    if (!oFrameWork.WarpRoomAdded.GetValueForScreen(Context.ScreenId) )
            //    {
            //        oFrameWork.AddWarpRoom();
            //    }
            //    int x = 1;
            //}
        }

        private static void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            AddNetLogMessage($"Msg Rcvd from: {e.FromPlayerID}, Type: {e.Type}");

            if (e.FromModID == helper.ModRegistry.ModID)
            {
                logger.Log($"{LogPrefix}Multiplayer_ModMessageReceived", LogLevel.Debug);
                foreach (Action<ModMessageReceivedEventArgs> callback in ModMessageReceivedCallbacks)
                {
                    callback(e);
                }
            }
        }

        private static void Multiplayer_PeerContextReceived(object sender, PeerContextReceivedEventArgs e)
        {
            // context is fired before and after PeerConnected
            EvalStatus();
            AddNetLogMessage($"Peer Context Rcvd from {e.Peer.PlayerID}");
            logger.Log($"{LogPrefix}Multiplayer_PeerContextReceived", LogLevel.Debug);

            foreach (Action<PeerContextReceivedEventArgs> callback in PeerContextReceivedCallbacks)
            {
                callback(e);
            }
        }
        private static void Multiplayer_PeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            EvalStatus();
            logger.Log($"{LogPrefix}Multiplayer_PeerDisconnected", LogLevel.Debug);
            AddNetLogMessage($"Peer Disconnected Rcvd from {e.Peer.PlayerID}");
            foreach (Action<PeerDisconnectedEventArgs> callback in PeerDisconnectedCallbacks)
            {
                callback(e);
            }
        }
        private static void Multiplayer_PeerConnected(object sender, PeerConnectedEventArgs e)
        {

            EvalStatus();
            logger.Log($"{LogPrefix}Multiplayer_PeerConnected", LogLevel.Debug);
            AddNetLogMessage($"Peer Connected Rcvd from {e.Peer.PlayerID}");
            foreach (Action<PeerConnectedEventArgs> callback in PeerConnectedCallbacks)
            {
                callback(e);
            }

        }

        public static void DumpTheWorld()
        {
            logger.LogInfo("IsMP", $"{IsMultiplayer},{Context.IsMultiplayer}");
            logger.LogInfo("IsMaster", $"{IsMasterGame}");
            logger.LogInfo("IsSplitScrren", $"{IsSplitScreen}");
            logger.LogInfo("InstanceId", $"{Game1.game1.instanceId}");
            if (IsSplitScreen)
            {
                logger.LogInfo("ScreenId", $"{Context.ScreenId}");
            }
        }
    }
}
