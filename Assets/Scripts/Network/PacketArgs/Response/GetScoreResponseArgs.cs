namespace Network.PacketArgs
{
    public class GetScoreResponseArgs
    {
        public int AccountId { get; set; }
        public int GameId { get; set; }
        public int Type { get; set; }
        public int Points { get; set; }
    }
}