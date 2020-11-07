using System.Collections.Generic;
using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class GetScoresPacket : PacketBase
    {
        public override int Id => 4;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/scores/getsome";
        public List<GetScoreResponseArgs> Response { get; private set; }


        public GetScoresPacket(AccIdGameId _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.Utility.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<List<GetScoreResponseArgs>>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}