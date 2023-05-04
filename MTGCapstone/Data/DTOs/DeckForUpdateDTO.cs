using System.ComponentModel.DataAnnotations;

namespace MTGCapstone.API.Data.DTOs
{
    public class DeckForUpdateDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public bool IsPrivate { get; set; }

        [Required]
        public string? Format { get; set; }
        public string? Primer { get; set; }
    }
}
