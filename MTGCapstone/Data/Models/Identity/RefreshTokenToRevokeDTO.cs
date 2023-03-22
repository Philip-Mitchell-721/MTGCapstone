using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.Models.Identity
{
    public class RefreshTokenToRevokeDTO
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}