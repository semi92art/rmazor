using Utils;

namespace Network.Packets
{
    public class DeleteTestUsersPacket : PacketBase
    {
        public override string Method => "GET";
        public override string Id => nameof(DeleteTestUsersPacket);
        public override string Url => 
            $"{GameClientUtils.ServerApiUrl}/api/accounts/delete_test_accounts";
        
        public DeleteTestUsersPacket() : base(null) { }
    }
}