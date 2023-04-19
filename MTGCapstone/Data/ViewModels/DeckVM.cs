using MTGCapstone.API.Data.Models;

namespace MTGCapstone.API.Data.ViewModels
{
    public class DeckVM
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string? Name { get; set; }
        public bool IsPrivate { get; set; }
        public string? Format { get; set; }
        public string? Primer { get; set; }
        public int? Views { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastEditedAt { get; set; }

        public User? User { get; set; }
        public List<Like> Likes { get; set; } = new List<Like>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<DeckCategory> DeckCategories { get; set; } = new List<DeckCategory>();
        public List<DeckCard> DeckCards { get; set; } = new List<DeckCard>();

    }
}
