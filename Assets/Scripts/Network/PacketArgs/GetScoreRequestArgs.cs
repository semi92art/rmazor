namespace Network.PacketArgs
{
    public class GetScoreRequestArgs : AccIdGameId
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }
}