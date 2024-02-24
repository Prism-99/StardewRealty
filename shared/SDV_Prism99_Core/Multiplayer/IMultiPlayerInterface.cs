using StardewModdingAPI.Events;


namespace Prism99_Core.MultiplayerUtils
{
    abstract class IMultiPlayerInterface
    {
        public void AddHooks(IModHelper Helper)
        {
            P99Core_MultiPlayer.Add_PeerContextReceivedCallback(Handle_PeerContextReceived);
            P99Core_MultiPlayer.Add_PeerDisconnectedCallback(Handle_PeerDisconnected);
            P99Core_MultiPlayer.Add_PeerConnectedCallback(Handle_PeerConnected);
            P99Core_MultiPlayer.Add_ModMessageReceivedCallback(Handle_MessageReceived);

        }
        public abstract  void Handle_PeerContextReceived(PeerContextReceivedEventArgs e);
        public abstract void Handle_PeerDisconnected(PeerDisconnectedEventArgs e);
        public abstract void Handle_PeerConnected(PeerConnectedEventArgs e);
        public abstract void Handle_MessageReceived(ModMessageReceivedEventArgs e);
    }
}
