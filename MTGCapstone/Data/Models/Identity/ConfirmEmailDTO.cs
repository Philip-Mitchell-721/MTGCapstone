using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.Models.Identity
{
    public class ConfirmEmailDTO
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string token { get; set; } = null!;
    }
}