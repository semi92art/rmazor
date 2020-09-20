namespace Network.PacketArgs
{
    public class LoginUserPacketRequestArgs
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string DeviceId { get; set; }
    }
}