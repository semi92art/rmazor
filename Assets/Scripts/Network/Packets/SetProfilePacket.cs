using Network.PacketArgs;

namespace Network.Packets
{
    public class SetProfilePacket : PacketBase
    {
        public override int Id => 8;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/profiles/set";

        public SetProfilePacket(SetProfileRequestArgs _Request) : base(_Request)
        { }
    }
}