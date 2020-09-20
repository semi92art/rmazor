using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class SetScorePacket : PacketBase
    {
        public override int Id => 7;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/scores/set";

        public SetScorePacket(SetScoreRequestArgs _Request) : base(_Request)
        { }
    }
}