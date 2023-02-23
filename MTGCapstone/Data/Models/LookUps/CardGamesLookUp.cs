namespace MTGCapstone.API.Data.Models
{
    public class CardGamesLookUp
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int GamesLookUpId { get; set; }

        public Card? Card { get; set; }
        public GamesLookUp? GamesLookUp { get; set; }
    }
}
