namespace MTGCapstone.API.Data.Models
{
    public class DeckCategoryDeckCard 
    {
        public int Id { get; set; }
        public int? DeckCategoryId { get; set; }
        public int? DeckCardId { get; set; }

        public int Quantity { get; set; }
        public DeckCard? DeckCard { get; set; }
        public DeckCategory? DeckCategory { get; set; }
    }
}
