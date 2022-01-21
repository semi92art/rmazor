namespace Common.Network.Packets
{
    public sealed class GameDataFieldsSetPacket : PacketBase
    {
        public override string Id => nameof(GameDataFieldsSetPacket);
        public override string Url => $"{GameClientUtils.ServerApiUrl}/api/game_data_fields/set_list";
        
        public GameDataFieldsSetPacket(GameFieldDto[] _Request) : base(_Request) { }
    }
}