namespace Entities
{
    public class Account
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string DeviceId { get; set; }
        public int Id { get; set; }
        public System.DateTime CreationTime { get; set; }
    }
}