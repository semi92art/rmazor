namespace Network.PacketArgs
{
    public class RankRequestArgs : AccIdGameId
    {
        public string Type { get; set; }
        public bool Global { get; set; }
    }
}