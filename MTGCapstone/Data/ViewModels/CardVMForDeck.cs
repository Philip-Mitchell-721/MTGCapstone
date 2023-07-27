namespace MTGCapstone.API.Data.ViewModels
{
    public class CardVMForDeck
    {
        public int DeckCardId { get; set; }
        public string? ScryfallId { get; set; }
        public int Quantity { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public string? Board { get; set; }


        public string? OracleId { get; set; }
        public string? Name { get; set; }
        public string? ReleasedAt { get; set; }
        public string? ScryfallUri { get; set; }
        public ImageUrisVM? ImageUris { get; set; }
        public string? ManaCost { get; set; }
        public decimal Cmc { get; set; }
        public string? TypeLine { get; set; }
        public string? OracleText { get; set; }
        public string? Power { get; set; }
        public string? Toughness { get; set; }
        public List<string> Colors { get; set; } = new List<string>();
        public List<string> ColorIndicator { get; set; } = new List<string>();
        public List<string> ColorIdentity { get; set; } = new List<string>();
        public List<string> Keywords { get; set; } = new List<string>();
        public FormatLegalitiesVM? Legalities { get; set; }
        public string? RulingsUri { get; set; }
        public string? PrintsSearchUri { get; set; }
        public string? Rarity { get; set; }
        public int EdhrecRank { get; set; }
        public PricesVM? Prices { get; set; }
        public RelatedUrisVM? RelatedUris { get; set; }
        public PurchaseUrisVM? PurchaseUris { get; set; }
        public int PennyRank { get; set; }
        public List<CardFaceVM> CardFaces { get; set; } = new List<CardFaceVM>();
    }
}
