using Network.PacketArgs;

namespace Network.Packets
{
    public sealed class GetScoresPacket : PacketBase
    {
        public override int Id => 4;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/scores/getsome";
        public GetScoresResponseArgs Response { get; private set; }


        public GetScoresPacket(AccountIdGameIdRequestdArgs _Request) : base(_Request)
        { }

        public override void DeserializeResponse(string _Json)
        {
            Response = GameClient.Deserialize<GetScoresResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}