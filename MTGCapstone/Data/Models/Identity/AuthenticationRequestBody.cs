using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.Models.Identity
{
    public class AuthenticationRequestBody
    {

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

    }
}
