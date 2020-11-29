namespace Network.PacketArgs
{
    public class RegisterUserPacketRequestArgs : LoginUserPacketRequestArgs
    {
        public int GameId { get; set; }
    }
}