using Microsoft.EntityFrameworkCore;

namespace MTGCapstone.API.Data.Models
{
    public class Card //printing 
    {
        public int Id { get; set; }
        public string? ScryfallId { get; set; }
        public string? OracleId { get; set; }
        public List<MultiverseIdsLookUp> MultiverseIds { get; set; } = new List<MultiverseIdsLookUp>();
        public int MtgoId { get; set; }
        public int TcgplayerId { get; set; }
        public int CardmarketId { get; set; }
        public string? Name { get; set; }
        public string? Language { get; set; }
        public string? ReleasedAt { get; set; }
        public string? Uri { get; set; }
        public string? ScryfallUri { get; set; }
        public string? Layout { get; set; }
        public bool HighresImage { get; set; }
        public string? ImageStatus { get; set; }
        public ImageUris? ImageUris { get; set; }
        public string? ManaCost { get; set; }

        [Precision(18, 2)]
        public decimal Cmc { get; set; }
        public string? TypeLine { get; set; }
        public string? OracleText { get; set; }
        public string? Power { get; set; }
        public string? Toughness { get; set; }
        public List<CardColorsLookUp> Colors { get; set; } = new List<CardColorsLookUp>();
        public List<CardColorIndicatorLookUp> ColorIndicator { get; set; } = new List<CardColorIndicatorLookUp>();
        public List<CardColorIdentityLookUp> ColorIdentity { get; set; } = new List<CardColorIdentityLookUp>();
        public List<CardKeywordsLookUp> Keywords { get; set; } = new List<CardKeywordsLookUp>();
        public FormatLegalities? Legalities { get; set; }
        public List<CardGamesLookUp> Games { get; set; } = new List<CardGamesLookUp>();  //paper, arena, and/or mtgo
        public bool Reserved { get; set; }
        public bool Foil { get; set; }
        public bool Nonfoil { get; set; }
        public List<CardFinishesLookUp> Finishes { get; set; } = new List<CardFinishesLookUp>();
        public bool Oversized { get; set; }
        public bool Promo { get; set; }
        public bool Reprint { get; set; }
        public bool Variation { get; set; }
        public string? SetId { get; set; }
        public string? Set { get; set; }
        public string? SetName { get; set; }
        public string? SetType { get; set; }
        public string? SetUri { get; set; }
        public string? SetSearchUri { get; set; }
        public string? ScryfallSetUri { get; set; }
        public string? RulingsUri { get; set; }
        public string? PrintsSearchUri { get; set; }
        public string? CollectorNumber { get; set; }
        public bool Digital { get; set; }
        public string? Rarity { get; set; }
        public string? CardBackId { get; set; }
        public string? Artist { get; set; }
        public List<CardScryfallArtistIdsLookUp> ScryfallArtistIds { get; set; } = new List<CardScryfallArtistIdsLookUp>();
        public string? IllustrationId { get; set; }
        public string? BorderColor { get; set; }
        public string? Frame { get; set; }
        public bool FullArt { get; set; }
        public bool Textless { get; set; }
        public bool Booster { get; set; }
        public bool StorySpotlight { get; set; }
        public int EdhrecRank { get; set; }
        public Prices? Prices { get; set; }
        public RelatedUris? RelatedUris { get; set; }
        public PurchaseUris? PurchaseUris { get; set; }
        public string? FlavorText { get; set; }
        public int PennyRank { get; set; }
        public int MtgoFoilId { get; set; }
        public List<CardFace> CardFaces { get; set; } = new List<CardFace>();
    }
}
