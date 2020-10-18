using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class LoginUserPacket : PacketBase
    {
        public override int Id => 5;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/accounts/login";
        public LoginUserPacketResponseArgs Response { get; private set; }
        
        private readonly LoginUserPacketRequestArgs m_Request;

        public LoginUserPacket(LoginUserPacketRequestArgs _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.Utility.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Deserialize<LoginUserPacketResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}