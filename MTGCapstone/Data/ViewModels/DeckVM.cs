using Microsoft.EntityFrameworkCore;
using MTGCapstone.API.Data.Models;

namespace MTGCapstone.API.Data.ViewModels
{
    public class DeckVM
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }

        public string? Name { get; set; }
        public bool IsPrivate { get; set; }
        public string? Format { get; set; }
        public string? Primer { get; set; }
        public int Views { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastEditedAt { get; set; }

        public int Likes { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<DeckCategory> DeckCategories { get; set; } = new List<DeckCategory>();
        public List<CardVMForDeck> Cards { get; set; } = new List<CardVMForDeck>();

    }
}
