namespace MTGCapstone.API.Data.Models
{
    public class Like //kinda the join between a deck and ANOTHER user (not the deck's creator)
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? DeckId { get; set; }

        public User? User { get; set; }
        public Deck? Deck { get; set; }
    }
}
