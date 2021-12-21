using Utils;

namespace Network.Packets
{
    public sealed class LoginUserPacket : PacketBase
    {
        public override string Id => nameof(LoginUserPacket);
        public override string Url => $"{GameClientUtils.ServerApiUrl}/api/accounts/login";
        public Account Response { get; private set; }
        
        private readonly LoginUserPacketRequestArgs m_Request;

        public LoginUserPacket(LoginUserPacketRequestArgs _Request) : base(_Request) { }

        public override void DeserializeResponse(string _Json)
        {
            if (NetworkUtils.IsPacketSuccess(ResponseCode))
                Response = Deserialize<Account>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
    
    public class LoginUserPacketRequestArgs
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
    }
}