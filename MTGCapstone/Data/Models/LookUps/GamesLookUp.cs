namespace MTGCapstone.API.Data.Models
{
    public class GamesLookUp //paper, arena, and mtgo
    {
        public int Id { get; set; }
        public string? Value { get; set; }

        public List<CardGamesLookUp> Cards { get; set; } = new List<CardGamesLookUp>();
    }
}
