namespace Network.Packets
{
    public class AccountDataFieldsSetPacket : PacketBase
    {
        public override string Id => nameof(GameDataFieldsSetPacket);
        public override string Url => $"{GameClient.Instance.ServerApiUrl}/api/account_data_fields/set_list";
        
        public AccountDataFieldsSetPacket(AccountFieldDto[] _Request) : base(_Request) { }
    }
}