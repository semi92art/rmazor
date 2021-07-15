using System;

namespace Network
{
    public class Account
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string DeviceId { get; set; }
        public int Id { get; set; }
        public DateTime CreationTime { get; set; }
    }
}