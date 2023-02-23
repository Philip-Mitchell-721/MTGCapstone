namespace MTGCapstone.API.Data.Models
{
    public class ColorsLookUp
    {
        public int Id { get; set; }
        public string? Value { get; set; }
        public string? FullValue { get; set; }

        public List<CardColorsLookUp> Cards { get; set; } = new List<CardColorsLookUp>();
    }
}