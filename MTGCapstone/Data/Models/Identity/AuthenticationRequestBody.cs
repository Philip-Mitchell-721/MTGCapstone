using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.Models.Identity
{
    public class AuthenticationRequestBody
    {

        [Required]
        public string? UserName { get; set; }

        [Required]
        public string? Password { get; set; }

    }
}
