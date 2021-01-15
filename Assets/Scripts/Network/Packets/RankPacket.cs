namespace Network.Packets
{
    public class RankPacket : PacketBase
    {
        public override string Id => nameof(RankPacket);
        public override string Url => $"{GameClient.Instance.ServerApiUrl}/api/scores/rank";
        public RankResponseArgs Response { get; private set; }
        
        public RankPacket(RankRequestArgs _Request) : base(_Request) { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.CommonUtils.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<RankResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
    
    public class RankRequestArgs : AccIdGameId
    {
        public string Type { get; set; }
        public bool Global { get; set; }
    }
    
    public class RankResponseArgs : AccIdGameId
    {
        public int Rank { get; set; }
    }
}