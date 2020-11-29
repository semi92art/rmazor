using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class RegisterUserPacket : PacketBase
    {
        public override int Id => 6;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/accounts/register";
        public RegisterUserPacketResponseArgs Response { get; private set; }
        
        private readonly RegisterUserPacketRequestArgs m_Request;
        private RegisterUserPacketResponseArgs m_Response;

        public RegisterUserPacket(RegisterUserPacketRequestArgs _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.CommonUtils.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<RegisterUserPacketResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}