using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class LoginUserPacket : PacketBase
    {
        public override int Id => 5;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/accounts/login";
        public Account Response { get; private set; }
        
        private readonly LoginUserPacketRequestArgs m_Request;

        public LoginUserPacket(LoginUserPacketRequestArgs _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.CommonUtils.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<Account>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}