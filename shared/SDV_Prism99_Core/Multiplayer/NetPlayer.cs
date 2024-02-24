using System.Linq;



namespace Prism99_Core.MultiplayerUtils
{
    internal class NetPlayer
    {
        public NetPlayer() { }
        public NetPlayer(IMultiplayerPeer peer, IModHelper helper)
        {
            PlayerId = peer.PlayerID;
            HasSMAPI = peer.HasSmapi;
            Platform = peer.Platform.Value.ToString();
            IsSplitScreen = peer.IsSplitScreen;
            IsHost = peer.IsHost;
            IsOnline = IsIdOnline(peer.PlayerID, helper);
            Name = P99Core_MultiPlayer.GetIdName(peer.PlayerID);
            IPAddress=Game1.server.getUserName(peer.PlayerID);
        }
        public NetPlayer(Farmer famer, IModHelper helper)
        {
            Name = famer.Name;
            PlayerId = famer.UniqueMultiplayerID;
            Platform = famer.platformID.Value;
            IsOnline = false;
            IPAddress = "????";

            var players = helper.Multiplayer.GetConnectedPlayers().Where(p => p.PlayerID == PlayerId).ToList();

            if (players.Any())
            {
                IMultiplayerPeer peer = players.First();

                HasSMAPI = peer.HasSmapi;
                IsSplitScreen = peer.IsSplitScreen;
                IsHost = peer.IsHost;
                ScreenId = peer.ScreenID ?? -1;
                IsOnline = true;
                IPAddress = Game1.server.getUserName(peer.PlayerID);
            }
        }

        private bool IsIdOnline(long playerId, IModHelper helper)
        {
            return helper.Multiplayer.GetConnectedPlayers().Where(p => p.PlayerID == PlayerId).Any();
        }
        public string Name { get; set; }
        public long PlayerId { get; set; }
        public bool HasSMAPI { get; set; }
        public string Platform { get; set; }
        public bool IsOnline { get; set; }
        public bool IsHost { get; set; }
        public bool IsSplitScreen { get; set; }
        public int ScreenId { get; set; }
        public string IPAddress { get; set; }
    }
}
