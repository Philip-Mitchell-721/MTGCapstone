namespace MTGCapstone.API.Data.Models
{
    public class ColorIndicatorLookUp
    {
        public int Id { get; set; }
        public string? Value { get; set; }
        public string? FullValue { get; set; }

        public List<CardColorIndicatorLookUp> Cards { get; set; } = new List<CardColorIndicatorLookUp>();
    }
}