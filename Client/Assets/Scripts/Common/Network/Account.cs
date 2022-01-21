using System;

namespace Common.Network
{
    public class Account
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string DeviceId { get; set; }
        public DateTime CreationTime { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }
    }
}