namespace Network.Packets
{
    public class TestConnectionPacket : PacketBase
    {
        public override int Id => 99;
        public override string Method => "GET";
        public override string Url => $"{GameClient.Instance.BaseUrl}/timetest";
        
        public TestConnectionPacket() : base(null)
        { }
    }
}