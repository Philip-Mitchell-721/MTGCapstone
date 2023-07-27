using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.DTOs
{
    public class AddCardRequestDto
    {
        public int? CardId { get; set; } = null;
        public string Board { get; set; } = "main";
        public string? ScryfallId { get; set; } = null;
        public int? DeckCardId { get; set; } = null;
    }
}
