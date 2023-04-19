using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.Models.Identity
{
    public class RefreshTokenDTO
    {
        [Required]
        public string AccessToken { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
