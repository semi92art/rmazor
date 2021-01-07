using Network.PacketArgs;

namespace Network.Packets
{
    public class DisableAdsPacket : PacketBase
    {
        public override int Id => 91;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/profiles/disableads";
        
        public DisableAdsPacket(AccIdGameId _Request) : base(_Request) { }
    }
}