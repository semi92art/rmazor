using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class GetFullAccountDataPacket : PacketBase
    {
        public override int Id => 1;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/accounts/getfulldata";
        public FullAccountData Response { get; private set; }

        public GetFullAccountDataPacket(AccIdGameId _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.CommonUtils.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<FullAccountData>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}