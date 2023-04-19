using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.Models.Identity
{
    public class ChangePasswordRequestDTO
    {
        [Required]
        public string UserName { get; set; } = null!;
    }
}