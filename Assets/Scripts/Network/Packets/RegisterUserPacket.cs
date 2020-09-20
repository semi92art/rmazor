using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class RegisterUserPacket : PacketBase
    {
        public override int Id => 100;
        public override object Request => m_Request;
        public override string Url { get; }
        public override string Method => "POST";
        public RegisterUserPacketResponseArgs Response { get; private set; }
        
        private readonly RegisterUserUserPacketRequestArgs m_Request;
        private RegisterUserPacketResponseArgs m_Response;

        public RegisterUserPacket(RegisterUserUserPacketRequestArgs _Request)
        {
            m_Request = _Request;
            Url = $"{GameClient.Instance.BaseUrl}/api/accounts/login";
        }

        public override void DeserializeResponse(string _Json)
        {
            Response = GameClient.Deserialize<RegisterUserPacketResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}