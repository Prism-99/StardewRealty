using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using StardewModdingAPI.Events;
using SDV_Realty_Core.Framework.Expansions;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModData;
using SDV_Realty_Core.Framework.ServiceInterfaces.ModMechanics;
using HarmonyLib;
using StardewValley.Network;
using Netcode;
using Prism99_Core.MultiplayerUtils;
using Prism99_Core.Utilities;
using SDV_Realty_Core.Framework.Locations;
using SDV_Realty_Core.Framework.ServiceInterfaces;

namespace SDV_Realty_Core.Framework.Objects
{
    internal class SDRMultiplayer
    {
        private const string action_activations = "activatations";
        private const string action_openexits = "openexits";
        private const string data_indoorlocaiton = "indoorname";
        private const string action_forsale = "forsale";
        private const string action_sold = "sold";
        private const string action_purchase = "purchase";
        private const string data_location = "locationdata";
        private const string client_conn = "clientconn";
        private const string warproom_status = "warp_status";
        private const string logPrefix = "SDRMP: ";
        private  int clientCount = 0;
        private  IModHelper helper;
        private  SDVLogger logger;
        public  int defaultInterpolationTicks = 15;
        //private IWarproomService warproomService;
        //private IFrameworkService frameworkService;
        private IExpansionManager expansionManager;
        private ILandManager landManager;
        private IExitsService exitsService;
        private enum WarpRoomStatus
        {
            Free,
            Busy
        }
        public class FEMessage
        {
            public List<Tuple<int, string>> Locations { get; set; }
            public int Code { get; set; }
            public byte[] Data { get; set; }
            public long Player { get; set; }
            public BinaryReader Reader;
        }
        public  void Initialize(SDVLogger olog,IModHelper oHelper,IExpansionManager expansionManager, ILandManager landManager, IExitsService exitsService )
        {
            helper = oHelper;
            logger = olog;
            //this.warproomService= warproomService;
            //this.frameworkService= frameworkService;
            this.expansionManager = expansionManager;
            this.landManager = landManager;
            this.exitsService = exitsService;

            P99Core_MultiPlayer.Initialize(helper, logger);
        }
        public  void AddHooks(EventArgs e)
        {
            //
            //  add required hooks for multiplayer games
            //
            if (P99Core_MultiPlayer.IsMasterGame)
            {
                P99Core_MultiPlayer.Add_PeerContextReceivedCallback(Handle_PeerContextReceived);
                P99Core_MultiPlayer.Add_PeerDisconnectedCallback(Multiplayer_PeerDisconnected);
                P99Core_MultiPlayer.Add_PeerConnectedCallback(Multiplayer_PeerConnected);
                P99Core_MultiPlayer.Add_ModMessageReceivedCallback(Handle_ModMessageReceived);

                logger.Log($"{logPrefix}Server, adding hooks", LogLevel.Trace);
                logger.Log($"My PlayerId is: {Game1.player.UniqueMultiplayerID}", LogLevel.Debug);
                //oHelper.Events.Multiplayer.PeerConnected += SDRMultiplayer.Multiplayer_PeerConnected;
                //oHelper.Events.Multiplayer.PeerDisconnected += SDRMultiplayer.Multiplayer_PeerDisconnected;
                //oHelper.Events.Multiplayer.ModMessageReceived += SDRMultiplayer.Multiplayer_ServerMessageReceived;
            }
            else if (true || P99Core_MultiPlayer.IsMultiplayer)
            {
                logger.Log($"{logPrefix}Client, adding hooks", LogLevel.Trace);
                logger.Log($"My PlayerId is: {Game1.player.UniqueMultiplayerID}", LogLevel.Debug);
                logger.Log($"Master PlayerId is: {Game1.MasterPlayer.UniqueMultiplayerID}", LogLevel.Debug);
                P99Core_MultiPlayer.Add_ModMessageReceivedCallback(Multiplayer_ClientModMessageReceived);
                //oHelper.Events.Multiplayer.PeerDisconnected += SDRMultiplayer.Multiplayer_ServerDisconnected;
                Multiplayer_SendPeerReady();
            }
        }
        public  void Handle_PeerContextReceived(PeerContextReceivedEventArgs e)
        {

        }
        public  void broadcastSprites(GameLocation location, params TemporaryAnimatedSprite[] sprites)
        {
            Traverse.Create(Game1.game1).Field("multiplayer").Method("broadcastSprites", new object[] { location, sprites });
        }
        public  void SendFarmHandPurchase(string expansionName)
        {
            logger.Log($"{logPrefix}Send land purchase '{expansionName}', to {Game1.player.UniqueMultiplayerID}");
            SendMessageToClient(new FEMessage() { Locations = new List<Tuple<int, string>>() { Tuple.Create(-1, expansionName) } }, action_purchase, Game1.MasterPlayer.UniqueMultiplayerID);

        }
        public  void SendNewLandForSale(long clientId, string expansionName)
        {
            logger.Log($"{logPrefix}Send land for sale '{expansionName}', to {clientId}");
            SendMessageToClient(new FEMessage() { Locations = new List<Tuple<int, string>>() { Tuple.Create(-1, expansionName) } }, action_forsale, clientId);
        }
        public  void SendLandSold(long clientId, string expansionNam)
        {
            logger.Log($"{logPrefix}Send land sold '{expansionNam}', to {clientId}");
            SendMessageToClient(new FEMessage() { Locations = new List<Tuple<int, string>>() { Tuple.Create(-1, expansionNam) } }, action_sold, clientId);
        }
        public  void SendActivateExpansions(long clientId, List<FarmExpansionLocation> expansions)
        {
            logger.Log($"{logPrefix}Sending Activate Expansions to {clientId}", LogLevel.Debug);

            FEMessage message = new FEMessage()
            {
                Code = 1,
                Locations = expansions.Select(p => Tuple.Create(p.GridId, p.Name)).ToList()
            };

            SendMessageToClient(message, action_activations, clientId);

            //foreach(var item in expansions)
            //{
            //    foreach(Building bld in item.buildings)
            //    {
            //        if (bld.indoors.Value != null)
            //        {
            //            sendLocation(clientId, bld.indoors.Value);
            //        }
            //    }
            //}
        }
        public  void SendActivateFarmExits(long clienId)
        {

            SendMessageToClient(new FEMessage(), action_openexits, clienId);
        }
        public  void SendMessageToClient(FEMessage message, string action, long clientId = -1)
        {
            P99Core_MultiPlayer.SendMessage(message, action, clientId);
            //logger.Log($"{logPrefiex}Sending '{action}', to {clientId}", StardewModdingAPI.LogLevel.Info);
            //if (clientId == -1)
            //{
            //    FEFramework.helper.Multiplayer.SendMessage(message, action, new string[] { FEFramework.helper.ModRegistry.ModID });
            //}
            //else
            //{
            //    FEFramework.helper.Multiplayer.SendMessage(message, action, new string[] { FEFramework.helper.ModRegistry.ModID }, new long[] { clientId });
            //}
        }
        public  bool HaveClients()
        {
            return clientCount > 0;
        }
        private  void SendMessageToServer(object message, string action)
        {
            P99Core_MultiPlayer.SendMessage(message, action, Game1.MasterPlayer.UniqueMultiplayerID);
        }
        public  void Multiplayer_SendPeerReady()
        {
            SendMessageToServer(new FEMessage(), client_conn);
        }
        public  void Multiplayer_PeerConnected(PeerConnectedEventArgs e)
        {
            clientCount++;
            //
            //  update client
            //
            logger.Log($"{logPrefix}[Server] Received peer connect from {e.Peer.PlayerID}", LogLevel.Info);
            //SendClientActiveExpansionList(e.Peer.PlayerID);
        }
        public  void Handle_PeerDisconnected(PeerDisconnectedEventArgs e)
        {
            if (P99Core_MultiPlayer.IsMasterGame)
            {
                Multiplayer_PeerDisconnected(e);
            }
            else
            {
                Multiplayer_ServerDisconnected(e);
            }
        }
        public  void Multiplayer_PeerDisconnected(PeerDisconnectedEventArgs e)
        {
            clientCount--;
        }
        public  void Multiplayer_ServerDisconnected(PeerDisconnectedEventArgs e)
        {
            //FEFramework.ResetForNewGame();
        }
        public  void Handle_ModMessageReceived(ModMessageReceivedEventArgs e)
        {
            logger.Log($"{logPrefix}Message Received", LogLevel.Debug);

            if (P99Core_MultiPlayer.IsMasterGame)
            {
                Multiplayer_ClientModMessageReceived(e);
            }
            else
            {
                Multiplayer_ServerMessageReceived(e);
            }
        }
        private  void SendClientActiveExpansionList(long clientId)
        {
            logger.Log($"{logPrefix}Sending Active Expansion list to {clientId}", LogLevel.Debug);
            //
            //  send activated expansions
            //
            List<FarmExpansionLocation> lActiveExpansions = expansionManager.expansionManager.GetActiveFarmExpansions().OrderBy(p => p.GridId).ToList();

            if (lActiveExpansions.Any())
            {
                SendActivateExpansions(clientId, lActiveExpansions);

                //sendLocation(e.FromPlayerID, FEFramework.GetBuildingIndoorLocation("Barn3787d4f52-4d89-4776-9d22-1a6a9f05e8ec"), true);
                //if (FEFramework.ExitsLoaded)
                //{
                //    FEFramework.monitor.Log($"Sending new client {e.FromPlayerID}, open farm exits", StardewModdingAPI.LogLevel.Trace);
                //    SendActivateFarmExits(e.FromPlayerID);
                //    // Traverse.Create(Game1.server).Method("sendLocation", new object[] {e.FromPlayerID,Game1.getLocationFromName("Backwoods") });
                //}
            }

        }
        public  void Multiplayer_ServerMessageReceived(ModMessageReceivedEventArgs e)
        {
            //
            //  message processing for master player
            //
            logger.Log($"{logPrefix}[Server] Received {e.Type}, from modId={e.FromModID}", LogLevel.Debug);
            if (e.FromModID == helper.ModRegistry.ModID)
            {

                switch (e.Type)
                {
                    case warproom_status:
                        //
                        //  update current usage of the warproom
                        //
                        FEMessage warp = e.ReadAs<FEMessage>();
                        switch ((WarpRoomStatus)warp.Code)
                        {
                            case WarpRoomStatus.Busy:
                                // warp room in use
                                WarproomManager.WarpRoomInUse = true;
                                WarproomManager.PlayerInWarpRoom = warp.Player;
                                break;
                            case WarpRoomStatus.Free:
                                //  warp room free
                                WarproomManager.WarpRoomInUse = false;
                                WarproomManager.PlayerInWarpRoom = -1;
                                break;
                        }
                        break;
                    case client_conn:
                        //
                        //  send activated expansions
                        //
                        SendClientActiveExpansionList(e.FromPlayerID);
                        //List<FarmExpansionLocation> lActiveExpansions = FEFramework.GetActiveFarmExpansions().OrderBy(p => p.GridId).ToList();

                        //if (lActiveExpansions.Any())
                        //{
                        //    SendActivateExpansions(e.FromPlayerID, lActiveExpansions);

                        //    //sendLocation(e.FromPlayerID, FEFramework.GetBuildingIndoorLocation("Barn3787d4f52-4d89-4776-9d22-1a6a9f05e8ec"), true);
                        //    //if (FEFramework.ExitsLoaded)
                        //    //{
                        //    //    FEFramework.monitor.Log($"Sending new client {e.FromPlayerID}, open farm exits", StardewModdingAPI.LogLevel.Trace);
                        //    //    SendActivateFarmExits(e.FromPlayerID);
                        //    //    // Traverse.Create(Game1.server).Method("sendLocation", new object[] {e.FromPlayerID,Game1.getLocationFromName("Backwoods") });
                        //    //}
                        //}
                        foreach (string forsale in landManager.LandForSale)
                        {
                            SendNewLandForSale(e.FromPlayerID, forsale);
                        }
                        break;
                    case action_purchase:
                        foreach (var expansion in e.ReadAs<FEMessage>().Locations)
                        {
                            landManager.PurchaseLand(expansion.Item2, true,e.FromPlayerID);
                        }
                        break;

                }
            }
        }
        public  void SendWarpRooomStatus(bool inUse, long userId)
        {
            FEMessage fEMessage = new FEMessage()
            {
                Player = userId,
                Code = (int)(inUse ? WarpRoomStatus.Busy : WarpRoomStatus.Free)
            };
            SendMessageToClient(fEMessage, warproom_status, -1);

        }
        public  void Multiplayer_ClientModMessageReceived(ModMessageReceivedEventArgs e)
        {
            logger.Log($"{logPrefix}[Client] Received {e.Type}, from clientId={e.FromPlayerID}", LogLevel.Debug);

            FEMessage inboud = e.ReadAs<FEMessage>();

            switch (e.Type)
            {
                case warproom_status:
                    //
                    //  update current usage of the warproom
                    //
                    FEMessage warp = e.ReadAs<FEMessage>();
                    switch ((WarpRoomStatus)warp.Code)
                    {
                        case WarpRoomStatus.Busy:
                            // warp room in use
                            WarproomManager.WarpRoomInUse = true;
                            WarproomManager.PlayerInWarpRoom = warp.Player;
                            break;
                        case WarpRoomStatus.Free:
                            //  warp room free
                            WarproomManager.WarpRoomInUse = false;
                            WarproomManager.PlayerInWarpRoom = -1;
                            break;
                    }
                    break;
                case action_openexits:
                    exitsService.AddFarmExits();
                    break;
                case action_activations:
                    foreach (var activation in inboud.Locations)
                    {
                        string logmsg = $"{logPrefix}Activating {activation.Item1},{activation.Item2}";
                        logger.Log(logmsg, LogLevel.Debug);
                        P99Core_MultiPlayer.AddNetLogMessage(logmsg);
                        expansionManager.expansionManager.ActivateExpansion(activation.Item2, activation.Item1);
                    }
                    break;
                case action_forsale:
                    foreach (var forsale in inboud.Locations)
                    {
                        landManager.LandForSale.Add(forsale.Item2);
                    }
                    break;
                case action_sold:
                    foreach (var forsale in inboud.Locations)
                    {
                        if (landManager.LandForSale.Contains(forsale.Item2))
                        {
                            landManager.LandForSale.Remove(forsale.Item2);
                        }
                    }
                    break;
                case data_location:
                    try
                    {
                        inboud.Reader = new BinaryReader(new MemoryStream(inboud.Data));

                        logger.Log($"Location length {inboud.Reader.BaseStream.Length}", LogLevel.Debug);

                        NetRoot<GameLocation> root = readObjectFull<GameLocation>(inboud.Reader);
                        logger.Log($"{logPrefix}Received GameLocation for '{root.Value.NameOrUniqueName}'", LogLevel.Debug);
                        if (Game1.getLocationFromName(root.Value.NameOrUniqueName) == null)
                        {
                            root.Value.reloadMap();
                            root.Value.resetForPlayerEntry();
                            //root.Value.load
                            Game1.locations.Add(root.Value);
                        }
                        else
                        {
                            logger.Log($"{logPrefix}Duplicate GameLocation received {root.Value.NameOrUniqueName}", LogLevel.Trace);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{logPrefix}Error receiving location data", ex);
                    }
                    break;
            }

        }

        //
        //  game code
        //
        public  NetRoot<T> readObjectFull<T>(BinaryReader reader) where T : class, INetObject<INetSerializable>
        {
            NetRoot<T> netRoot = NetRoot<T>.Connect(reader);
            netRoot.Clock.InterpolationTicks = defaultInterpolationTicks;
            return netRoot;
        }
        protected virtual void readActiveLocation(IncomingMessage msg)
        {
            bool force_current_location = msg.Reader.ReadBoolean();
            NetRoot<GameLocation> root = readObjectFull<GameLocation>(msg.Reader);
            //if (isAlwaysActiveLocation(root.Value))
            //{
            //    for (int i = 0; i < Game1.locations.Count; i++)
            //    {
            //        if (!Game1.locations[i].Equals(root.Value))
            //        {
            //            continue;
            //        }
            //        if (Game1.locations[i] == root.Value)
            //        {
            //            break;
            //        }
            //        if (Game1.locations[i] != null)
            //        {
            //            if (Game1.currentLocation == Game1.locations[i])
            //            {
            //                Game1.currentLocation = root.Value;
            //            }
            //            if (Game1.player.currentLocation == Game1.locations[i])
            //            {
            //                Game1.player.currentLocation = root.Value;
            //            }
            //            Game1.removeLocationFromLocationLookup(Game1.locations[i]);
            //        }
            //        Game1.locations[i] = root.Value;
            //        break;
            //    }
            //}
            if (!(Game1.locationRequest != null || force_current_location))
            {
                return;
            }
            if (Game1.locationRequest != null)
            {
                Game1.currentLocation = Game1.findStructure(root.Value, Game1.locationRequest.Name);
                if (Game1.currentLocation == null)
                {
                    Game1.currentLocation = root.Value;
                }
            }
            else if (force_current_location)
            {
                Game1.currentLocation = root.Value;
            }
            if (Game1.locationRequest != null)
            {
                Game1.locationRequest.Location = root.Value;
                Game1.locationRequest.Loaded(root.Value);
            }
            Game1.currentLocation.resetForPlayerEntry();
            Game1.player.currentLocation = Game1.currentLocation;
            if (Game1.locationRequest != null)
            {
                Game1.locationRequest.Warped(root.Value);
            }
            Game1.currentLocation.updateSeasonalTileSheets();
            if (Game1.IsDebrisWeatherHere())
            {
                Game1.populateDebrisWeatherArray();
            }
            Game1.locationRequest = null;
        }
        public  void sendMessage(long peerId, byte messageType, Farmer sourceFarmer, params object[] data)
        {
            sendMessage(peerId, new OutgoingMessage(messageType, sourceFarmer, data));
        }
        public  void sendLocation(long peer, GameLocation location, bool force_current = false)
        {
            logger.Log($"{logPrefix}Sending location_data for {location.NameOrUniqueName}", LogLevel.Debug);
            //location.Root.Value = null;
            FEMessage message = new FEMessage
            {
                Data = writeObjectFullBytes(locationRoot(location), peer)
            };
            logger.Log($"{logPrefix} location data length={message.Data.Length}", LogLevel.Debug);
            SendMessageToClient(message, data_location, peer);
            //sendMessage(peer, 3, Game1.serverHost.Value, force_current, writeObjectFullBytes(locationRoot(location), peer));
        }
        public  void sendMessage(long peerId, OutgoingMessage message)
        {

            Traverse.Create(Game1.game1).Field("server").Method("sendMessage", new object[] { peerId, message });
            //foreach (Server server in  servers)
            //{
            //    server.sendMessage(peerId, message);
            //}
        }
        public  byte[] writeObjectFullBytes<T>(NetRoot<T> root, long? peer) where T : class, INetObject<INetSerializable>
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = createWriter(stream);
            root.CreateConnectionPacket(writer, peer);
            return stream.ToArray();
        }
        public  NetRoot<GameLocation> locationRoot(GameLocation location)
        {
            if (location.Root.Value == null && Game1.IsMasterGame)
            {
                new NetRoot<GameLocation>().Set(location);
                location.Root.Clock.InterpolationTicks = interpolationTicks();
                location.Root.MarkClean();
            }
            return location.Root;
        }
        protected  BinaryWriter createWriter(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            //if (logging.IsLogging)
            //{
            //    writer = new LoggingBinaryWriter(writer);
            //}
            return writer;
        }
        public  int interpolationTicks()
        {
            if (!allowSyncDelay())
            {
                return 0;
            }
            if (LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
            {
                return 4;
            }
            return defaultInterpolationTicks;
        }
        public  bool allowSyncDelay()
        {
            return Game1.newDaySync == null;
        }
    }
}
