namespace Network.Packets
{
    public class DeleteTestUsersPacket : PacketBase
    {
        public override int Id => 11;
        public override string Url => $"{GameClient.Instance.BaseUrl}/api/accounts/delete_test_accounts";
        
        public DeleteTestUsersPacket() : base(null)
        { }
    }
}