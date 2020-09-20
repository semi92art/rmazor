using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class AccountDataPacket : PacketBase
    {
        public override int Id { get; }
        public override object Request => m_Request;
        public override string Url { get; }
        public override string Method => "POST";
        public AccountDataPacketResponseArgs Response { get; private set; }

        private readonly AccountDataPacketRequestArgs m_Request;
        private AccountDataPacketResponseArgs m_Response;

        public AccountDataPacket(AccountDataPacketRequestArgs _Request)
        {
            m_Request = _Request;
            Url = $"{GameClient.Instance.BaseUrl}/api/accounts/getfulldatabyid";
        }

        public override void DeserializeResponse(string _Json)
        {
            Response = GameClient.Deserialize<AccountDataPacketResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}