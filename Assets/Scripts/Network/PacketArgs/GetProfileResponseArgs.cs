using System.Collections.Generic;

namespace Network.PacketArgs
{
    public class GetProfileResponseArgs
    {
        public int AccountId { get; set; }
        public int Gold { get; set; }
        public int Diamonds { get; set; }
    }
}