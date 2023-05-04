namespace MTGCapstone.API.Data.ViewModels
{
    public class CardFaceVM
    {
        public string? Name { get; set; }
        public string? ManaCost { get; set; }
        public string? TypeLine { get; set; }
        public string? OracleText { get; set; }
        public List<string> Colors { get; set; } = new List<string>();
        public string? Power { get; set; }
        public string? Toughness { get; set; }
        public ImageUrisVM? ImageUris { get; set; }
        public List<string> ColorIndicator { get; set; } = new List<string>();
    }
}
