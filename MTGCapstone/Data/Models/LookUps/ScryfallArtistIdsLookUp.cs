namespace MTGCapstone.API.Data.Models
{
    public class ScryfallArtistIdsLookUp
    {
        public int Id { get; set; }
        public string? Value { get; set; }

        public List<CardScryfallArtistIdsLookUp> Cards { get; set; } = new List<CardScryfallArtistIdsLookUp>();
    }
}
