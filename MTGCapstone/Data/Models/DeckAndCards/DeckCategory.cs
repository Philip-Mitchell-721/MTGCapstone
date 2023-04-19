namespace MTGCapstone.API.Data.Models
{
    public class DeckCategory 
    {
        public int Id { get; set; }
        public int DeckId { get; set; }

        public string? Name { get; set; }

        public Deck? Deck { get; set; }
        public List<DeckCategoryDeckCard> DeckCategoryDeckCards { get; set; } = new List<DeckCategoryDeckCard>(); 
    }
}
