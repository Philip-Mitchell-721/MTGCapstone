namespace MTGCapstone.API.Data.Models
{
    public class CardFace
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public string? Name { get; set; }
        public string? ManaCost { get; set; }
        public string? TypeLine { get; set; }
        public string? OracleText { get; set; }
        public List<CardColorsLookUp> Colors { get; set; } = new List<CardColorsLookUp>();
        public string? Power { get; set; }
        public string? Toughness { get; set; }
        public string? FlavorText { get; set; }
        public string? Artist { get; set; }
        public string? ScryfallArtistId { get; set; }
        public string? IllustrationId { get; set; }
        public ImageUris? ImageUris { get; set; }
        public string? FlavorName { get; set; }
        public List<CardColorIndicatorLookUp> ColorIndicator { get; set; } = new List<CardColorIndicatorLookUp>();
    }
}
