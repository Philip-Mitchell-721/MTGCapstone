namespace MTGCapstone.API.Data.Models
{
    public class DeckCard
    {
        public int Id { get; set; }
        public int? CardId { get; set; }
        public int? DeckId { get; set; }
        public List<DeckCategoryDeckCard> DeckCategories { get; set; } = new List<DeckCategoryDeckCard>();

        public Card? Card { get; set; } //Cards.Where(c => c.Id == deckCard.cardId)
        /*
         SELECT * FROM DeckCards AS dc
         INNER JOIN Cards AS c ON dc.CardId = c.Id
         WHERE dc.Id = {deckCardId}
        */
        public Deck? Deck { get; set; }
    }
}
