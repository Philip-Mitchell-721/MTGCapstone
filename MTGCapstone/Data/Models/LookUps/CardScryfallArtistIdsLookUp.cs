namespace MTGCapstone.API.Data.Models
{
    public class CardScryfallArtistIdsLookUp
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int ScryfallArtistIdsLookUpId { get; set; }

        public Card? Card { get; set; }
        public ScryfallArtistIdsLookUp? ScryfallArtistIdsLookUp { get; set; }
    }
}
