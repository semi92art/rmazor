namespace Network.Packets
{
    public class AccountDataFieldsGetPacket : PacketBase
    {
        public override string Id => nameof(GameDataFieldsGetPacket);
        public override string Url => $"{GameClient.Instance.ServerApiUrl}/api/account_data_fields/get_list";
        public AccountFieldDto[] Response { get; private set; }
        
        public AccountDataFieldsGetPacket(AccountFieldListDtoLite _Request) : base(_Request) { }

        public override void DeserializeResponse(string _Json)
        {
            if (Utils.CommonUtils.IsInRange(ResponseCode, 200, 299))
                Response = GameClient.Instance.Deserialize<AccountFieldDto[]>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
}