using System;

namespace ClickersAPI.Entities
{
    public class Account : IId, ICreationTime
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
