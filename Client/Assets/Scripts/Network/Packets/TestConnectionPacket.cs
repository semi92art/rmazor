using Utils;

namespace Network.Packets
{
    public class TestConnectionPacket : PacketBase
    {
        public override string Id => nameof(TestConnectionPacket);
        public override string Method => "GET";
        public override string Url => $"{GameClientUtils.ServerApiUrl}/timetest";
        
        public TestConnectionPacket() : base(null)
        { }
    }
}