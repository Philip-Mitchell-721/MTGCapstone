namespace MTGCapstone.API.Data.Models
{
    public class KeywordsLookUp
    {
        public int Id { get; set; }
        public string? Value { get; set; }

        public List<CardKeywordsLookUp> Cards { get; set; } = new List<CardKeywordsLookUp>();
    }
}