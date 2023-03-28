namespace MTGCapstone.API.Data.Models
{
    public class RelatedUris
    {
        public int Id { get; set; }
        public int CardId { get; set; }

        public string? Gatherer { get; set; }
        public string? TcgplayerInfiniteArticles { get; set; }
        public string? TcgplayerInfiniteDecks { get; set; }
        public string? Edhrec { get; set; }
    }

}
