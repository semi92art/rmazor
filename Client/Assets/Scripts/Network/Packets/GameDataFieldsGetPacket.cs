using Utils;

namespace Network.Packets
{
    public sealed class GameDataFieldsGetPacket : PacketBase
    {
        public override string Id => nameof(GameDataFieldsGetPacket);
        public override string Url => $"{GameClientUtils.ServerApiUrl}/api/game_data_fields/get_list";
        public GameFieldDto[] Response { get; private set; }
        
        public GameDataFieldsGetPacket(GameFieldListDtoLite _Request) : base(_Request) { }

        public override void DeserializeResponse(string _Json)
        {
            if (CommonUtils.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<GameFieldDto[]>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}