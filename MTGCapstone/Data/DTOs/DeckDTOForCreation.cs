using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.DTOs
{
    public class DeckDTOForCreation
    {
        [Required]
        public string? Name { get; set; }

        public bool IsPrivate { get; set; } = true;

        [Required]
        public string? Format { get; set; }

        public string? Primer { get; set; }
    }
}
