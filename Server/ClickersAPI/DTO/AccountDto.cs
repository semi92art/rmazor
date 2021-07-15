using System;

namespace ClickersAPI.DTO
{
    public class AccountDto : AccountCreationDto
    {
        public int Id { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
