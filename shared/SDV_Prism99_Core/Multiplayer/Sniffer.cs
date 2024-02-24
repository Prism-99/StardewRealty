using Prism99_Core.Utilities;
using StardewValley.Network;
using System.Collections.Generic;


namespace Prism99_Core.MultiplayerUtils
{
    public enum MessageTypes
    {
        farmerDelta = 0,
        serverIntroduction = 1,
        playerIntroduction = 2,
        locationIntroduction = 3,
        forceEvent = 4,
        warpFarmer = 5,
        locationDelta = 6,
        locationSprites = 7,
        characterWarp = 8,
        availableFarmhands = 9,
        chatMessage = 10,
        connectionMessage = 11,
        worldDelta = 12,
        teamDelta = 13,
        newDaySync = 14,
        chatInfoMessage = 15,
        userNameUpdate = 16,
        farmerGainExperience = 17,
        serverToClientsMessage = 18,
        disconnecting = 19,
        sharedAchievement = 20,
        globalMessage = 21,
        partyWideMail = 22,
        forceKick = 23,
        removeLocationFromLookup = 24,
        farmerKilledMonster = 25,
        requestGrandpaReevaluation = 26,
        digBuriedNut = 27,
        requestPassout = 28,
        passout = 29
    }

    internal static class Sniffer
    {
        public struct netMessage
        {
            public bool Out;
            public string From;
            public long recipientId;
            public string Lang;
            public string Message;
            public MessageTypes MessageType;
        }

        private static SDVLogger logger;
        public static List<netMessage> ChatMessages = new List<netMessage>();

        public static void Initialize(SDVLogger olog,bool captureChat,bool captureGame)
        {
            logger = olog;

            //if (captureChat)
            //{
            //    GamePatches.AddPatch(new GamePatch
            //    {
            //        IsPrefix = false,
            //        Original = AccessTools.Method(typeof(Multiplayer), "receiveChatMessage", new Type[] { typeof(Farmer), typeof(long), typeof(LocalizedContentManager.LanguageCode), typeof(string) }),
            //        Target = new HarmonyMethod(typeof(Sniffer), nameof(Sniffer.receiveChatMessage)),
            //        Description = "Receive chat messages."
            //    }, "networking");
            //}
            //if (captureGame)
            //{
            //    GamePatches.AddPatch(new GamePatch
            //    {
            //        IsPrefix = false,
            //        Original = AccessTools.Method(typeof(GameServer), "sendMessage", new Type[] { typeof(long), typeof(OutgoingMessage) }),
            //        Target = new HarmonyMethod(typeof(Sniffer), nameof(Sniffer.sendMessage)),
            //        Description = "Receive sent chat messages."
            //    }, "networking");
            //}
            //GamePatches.ApplyPatches(null);
        }
        public static void sendMessage(IGameServer __instance, long peerId, OutgoingMessage message)
        {
            //string packetData = "";
            //MemoryStream ms = new MemoryStream();
            //BinaryWriter writer = new BinaryWriter(ms);

            //message.Write(writer);

            //IncomingMessage messageIn = new IncomingMessage();
            //ms.Seek(0, SeekOrigin.Begin);
            //messageIn.Read(new BinaryReader(ms));
            ////ms.Position = 0;
            //ms.Seek(0, SeekOrigin.Begin);
            //byte mtype=messageIn.Reader.ReadByte();
            //long mid = messageIn.Reader.ReadInt64();
            //switch ((MessageTypes) message.MessageType)
            //{
            //    case MessageTypes.locationDelta:
            //         packetData=messageIn.Reader.ReadString();
            //        break;
            //}
            //logger.Log($"Received send message {(MessageTypes)message.MessageType} to {peerId}, Data: {packetData}", StardewModdingAPI.LogLevel.Debug);

            ChatMessages.Add(new netMessage
            {
                From = "Me",
                recipientId = peerId,
                Lang = "ENU",
                MessageType = (MessageTypes)message.MessageType,
                Message = message.ToString(),
                Out = true
            });
        }
        public static void receiveChatMessage(Multiplayer __instance, Farmer sourceFarmer, long recipientID, LocalizedContentManager.LanguageCode language, string message)
        {
            logger.Log($"Received chat message", LogLevel.Debug);

            ChatMessages.Add(new netMessage
            {
                From = sourceFarmer.Name,
                recipientId = recipientID,
                Lang = language.ToString(),
                Message = message,
                Out = false
            });
        }
 

        public static void sendChatMessage(long reciepientId)
        {
            Game1.server.sendMessage(reciepientId, new OutgoingMessage((byte)MessageTypes.chatMessage, reciepientId, new object[] { "Hello, world" }));
        }
    }
}
