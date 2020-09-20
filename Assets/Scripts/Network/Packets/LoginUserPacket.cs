using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class LoginUserPacket : PacketBase
    {
        public override int Id => 101;
        public override object Request => m_Request;
        public override string Url { get; }
        public override string Method => "GET";
        public LoginUserPacketResponseArgs Response { get; private set; }
        
        private readonly LoginUserPacketRequestArgs m_Request;
        private LoginUserPacketResponseArgs m_Response;

        public LoginUserPacket(LoginUserPacketRequestArgs _Request)
        {
            m_Request = _Request;
            Url = $"{GameClient.Instance.BaseUrl}/api/accounts/login";
        }

        public override void DeserializeResponse(string _Json)
        {
            Response = GameClient.Deserialize<LoginUserPacketResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}