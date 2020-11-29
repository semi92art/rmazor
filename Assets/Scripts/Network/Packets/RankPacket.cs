using Network.PacketArgs;

namespace Network.Packets
{
    public class RankPacket : PacketBase
    {
        public override int Id => 10;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/scores/rank";
        public RankResponseArgs Response { get; private set; }
        
        public RankPacket(RankRequestArgs _Request) : base(_Request)
        {
        }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.CommonUtils.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<RankResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}