using Entities;

namespace Network.PacketArgs
{
    public class GetFullAccountDataPacketResponseArgs
    {
        public Account Acocunt { get; set; }
        public Score[] Scores { get; set; }
        public Profile Profile { get; set; }
    }
}