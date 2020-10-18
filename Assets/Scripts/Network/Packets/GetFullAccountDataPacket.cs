using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class GetFullAccountDataPacket : PacketBase
    {
        public override int Id => 1;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/accounts/getfulldata";
        public GetFullAccountDataPacketResponseArgs Response { get; private set; }

        public GetFullAccountDataPacket(AccountIdGameIdRequestdArgs _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.Utility.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Deserialize<GetFullAccountDataPacketResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}