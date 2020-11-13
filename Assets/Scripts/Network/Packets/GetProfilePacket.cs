using Network.PacketArgs;

namespace Network.Packets
{
    public class GetProfilePacket : PacketBase
    {
        public override int Id => 2;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/profiles/get";

        public ProfileResponse Response { get; private set; }
        
        public GetProfilePacket(AccIdGameId _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.Utility.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<ProfileResponse>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}