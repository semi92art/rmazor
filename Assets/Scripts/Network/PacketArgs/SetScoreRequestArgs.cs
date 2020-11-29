namespace Network.PacketArgs
{
    public class SetScoreRequestArgs : AccIdGameId
    {
        public string Type { get; set; }
        public int Points { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
    }
}