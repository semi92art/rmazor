using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class GetScorePacket : PacketBase
    {
        public override int Id => 3;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/scores/get";
        public Score Response { get; private set; }

        public GetScorePacket(GetScoreRequestArgs _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.Utility.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<Score>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}