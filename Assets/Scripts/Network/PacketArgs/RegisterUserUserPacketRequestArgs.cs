namespace Network.PacketArgs
{
    public class RegisterUserUserPacketRequestArgs : LoginUserPacketRequestArgs
    {
        public string CountryKey { get; set; }
        public int GameId { get; set; }
    }
}