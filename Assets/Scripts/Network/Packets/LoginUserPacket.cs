namespace Network.Packets
{
    public sealed class LoginUserPacket : PacketBase
    {
        public override string Id => nameof(LoginUserPacket);
        public override string Url => $"{GameClient.Instance.ServerApiUrl}/api/accounts/login";
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
    
    public class LoginUserPacketRequestArgs
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string DeviceId { get; set; }
    }
}