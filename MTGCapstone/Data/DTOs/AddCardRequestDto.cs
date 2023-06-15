using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.DTOs
{
    public class AddCardRequestDto
    {
        [Required]
        public int? CardId { get; set; } = null;
        public string Board { get; set; } = "main";
    }
}
