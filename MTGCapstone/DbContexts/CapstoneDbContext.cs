using Microsoft.EntityFrameworkCore;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;

namespace MTGCapstone.API.DbContexts
{
    public class CapstoneDbContext : DbContext
    {

        public CapstoneDbContext(DbContextOptions<CapstoneDbContext> options) : base(options)
        {

        }

        public DbSet<BulkData> BulkData { get; set; } = null!;

        public DbSet<Card> Cards { get; set; } = null!;
        public DbSet<CardFace> CardFaces { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Deck> Decks { get; set; } = null!;
        public DbSet<DeckCard> DeckCards { get; set; } = null!;
        public DbSet<DeckCategoryDeckCard> DeckCategoryDeckCards { get; set; } = null!;
        public DbSet<DeckCategory> DeckCategories { get; set; } = null!;
        public DbSet<FormatLegalities> FormatLegalities { get; set; } = null!;
        public DbSet<ImageUris> ImageUris { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<Packet> Packets { get; set; } = null!;
        public DbSet<PacketCard> PacketCards { get; set; } = null!;
        public DbSet<Prices> Prices { get; set; } = null!;
        public DbSet<PurchaseUris> PurchaseUris { get; set; } = null!;
        public DbSet<RelatedUris> RelatedUris { get; set; } = null!;
        public DbSet<Ruling> Rulings { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;


        #region Lookups
        public DbSet<CardColorIdentityLookUp> CardColorIdentityLookUps { get; set; } = null!;
        public DbSet<CardColorIndicatorLookUp> CardColorIndicatorLookUps { get; set; } = null!;
        public DbSet<CardColorsLookUp> CardColorsLookUps { get; set; } = null!;
        public DbSet<CardFinishesLookUp> CardFinishesLookUps { get; set; } = null!;
        public DbSet<CardGamesLookUp> CardGamesLookUps { get; set; } = null!;
        public DbSet<CardKeywordsLookUp> CardKeywordsLookUps { get; set; } = null!;
        public DbSet<CardScryfallArtistIdsLookUp> CardScryfallArtistIdsLookUps { get; set; } = null!;
        public DbSet<ColorIdentityLookUp> ColorIdentityLookUps { get; set; } = null!;
        public DbSet<ColorIndicatorLookUp> ColorIndicatorLookUps { get; set; } = null!;
        public DbSet<ColorsLookUp> ColorsLookUps { get; set; } = null!;
        public DbSet<FinishesLookUp> FinishesLookUps { get; set; } = null!;
        public DbSet<GamesLookUp> GamesLookUps { get; set; } = null!;
        public DbSet<KeywordsLookUp> KeywordsLookUps { get; set; } = null!;
        public DbSet<MultiverseIdsLookUp> MultiverseIdsLookUps { get; set; } = null!;
        public DbSet<PreferredMarketLookup> PreferredMarketLookups { get; set; } = null!;
        public DbSet<PreferredPriceUnitLookup> PreferredPriceUnitLookups { get; set; } = null!;
        public DbSet<ScryfallArtistIdsLookUp> ScryfallArtistIdsLookUps { get; set; } = null!;
        #endregion
    }
}
