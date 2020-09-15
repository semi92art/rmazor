using System;

namespace Network
{
    [Serializable]
    public abstract class PacketData
    {
        public int GameId { get; set; }
        public int AccountId { get; set; }
    }
}