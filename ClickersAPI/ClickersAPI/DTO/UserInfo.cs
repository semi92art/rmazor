using System.ComponentModel.DataAnnotations;

namespace ClickersAPI.DTO
{
    public class UserInfo
    {
        [EmailAddress]
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
