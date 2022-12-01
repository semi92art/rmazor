namespace Common.Network.Packets
{
    public class GameUserEventPacket : PacketBase
    {
        public GameUserEventPacket(GameUserDto _Request) : base(_Request) { }

        public override string Id => "999";
        public override string Url => $"{GameClientUtils.ServerApiUrl}/api/bot/send_message";
    }
}