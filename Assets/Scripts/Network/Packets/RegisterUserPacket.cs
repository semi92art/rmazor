using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class RegisterUserPacket : PacketBase
    {
        public override int Id => 6;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/accounts/register";
        public RegisterUserPacketResponseArgs Response { get; private set; }
        
        private readonly RegisterUserUserPacketRequestArgs m_Request;
        private RegisterUserPacketResponseArgs m_Response;

        public RegisterUserPacket(RegisterUserUserPacketRequestArgs _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            Response = GameClient.Deserialize<RegisterUserPacketResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}