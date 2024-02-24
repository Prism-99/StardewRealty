using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Network;
using Prism99_Core.Utilities;

//using SDV_Realty_Core.Framework.Objects;

namespace Prism99_Core
{
    internal static class P99Core_MultiPlayer
    {
        private static Multiplayer mPlayer;
        private static IModHelper helper;
        private static SDVLogger logger;
        //private static FEFramework oFrameWork;
        private static List<Action<PeerContextReceivedEventArgs>> PeerContextReceivedCallbacks = new List<Action<PeerContextReceivedEventArgs>>();
        private static List<Action<PeerConnectedEventArgs>> PeerConnectedCallbacks = new List<Action<PeerConnectedEventArgs>>();
        private static List<Action<PeerDisconnectedEventArgs>> PeerDisconnectedCallbacks = new List<Action<PeerDisconnectedEventArgs>>();
        private static List<Action<ModMessageReceivedEventArgs>> ModMessageReceivedCallbacks = new List<Action<ModMessageReceivedEventArgs>>();

        public static bool IsMultiplayer { get { return Game1.IsMultiplayer || Context.ScreenId > 0; } }
        public static bool IsMasterGame { get { return Context.IsMainPlayer  && Context.ScreenId==0; }  }
        public static bool IsSplitScreen { get; private set; }
        public static IGameServer GameServer { get; private set; }

        public static void Initialize(IModHelper oHelper,SDVLogger ologger)
        {
            helper = oHelper;
            mPlayer = oHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            logger = ologger;
            //oFrameWork = fw;

            GameServer = Game1.server;

            //IsMultiplayer = Game1.IsMultiplayer;
            //IsMasterGame = Game1.IsMasterGame;
            IsSplitScreen = Context.IsSplitScreen;

            helper.Events.Multiplayer.PeerConnected += Multiplayer_PeerConnected;
            helper.Events.Multiplayer.PeerContextReceived += Multiplayer_PeerContextReceived;
            helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;
            helper.Events.Multiplayer.PeerDisconnected += Multiplayer_PeerDisconnected;
            
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
        }
        private static void EvalStatus()
        {
            //IsMultiplayer = Game1.IsMultiplayer;
            //IsMasterGame = Game1.IsMasterGame;
            IsSplitScreen = Context.IsSplitScreen;
            if(GameServer== null)
            {
                GameServer = Game1.server;
            }
        }


        public static void Add_PeerContextReceivedCallback(Action<PeerContextReceivedEventArgs> callback)
        {
             PeerContextReceivedCallbacks.Add(callback);
        }
        public static void Add_PeerConnectedCallback(Action<PeerConnectedEventArgs> callback)
        {
            PeerConnectedCallbacks.Add(callback);
        }
        public static void Add_PeerDisconnectedCallbackk(Action<PeerDisconnectedEventArgs> callback)
        {
            PeerDisconnectedCallbacks.Add(callback);
        }
        public static void Add_ModMessageReceivedCallback(Action<ModMessageReceivedEventArgs> callback)
        {
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
            foreach (Action<ModMessageReceivedEventArgs> callback in ModMessageReceivedCallbacks)
            {
                callback(e);
            }
        }

        private static void Multiplayer_PeerContextReceived(object sender, PeerContextReceivedEventArgs e)
        {
            // context is fired before and after PeerConnected
            EvalStatus();

            foreach (Action<PeerContextReceivedEventArgs> callback in PeerContextReceivedCallbacks)
            {
                callback(e);
            }
        }
        private static void Multiplayer_PeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            EvalStatus();
            foreach (Action<PeerDisconnectedEventArgs> callback in PeerDisconnectedCallbacks)
            {
                callback(e);
            }
        }
        private static void Multiplayer_PeerConnected(object sender, PeerConnectedEventArgs e)
        {
            EvalStatus();
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
