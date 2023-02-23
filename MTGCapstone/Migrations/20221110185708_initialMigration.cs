using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTGCapstone.API.Migrations
{
    public partial class initialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulkData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScryfallId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompressedSize = table.Column<int>(type: "int", nullable: false),
                    DownloadUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentEncoding = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScryfallId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OracleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MtgoId = table.Column<int>(type: "int", nullable: false),
                    TcgplayerId = table.Column<int>(type: "int", nullable: false),
                    CardmarketId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleasedAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScryfallUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Layout = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighresImage = table.Column<bool>(type: "bit", nullable: false),
                    ImageStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManaCost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cmc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TypeLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OracleText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Power = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Toughness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reserved = table.Column<bool>(type: "bit", nullable: false),
                    Foil = table.Column<bool>(type: "bit", nullable: false),
                    Nonfoil = table.Column<bool>(type: "bit", nullable: false),
                    Oversized = table.Column<bool>(type: "bit", nullable: false),
                    Promo = table.Column<bool>(type: "bit", nullable: false),
                    Reprint = table.Column<bool>(type: "bit", nullable: false),
                    Variation = table.Column<bool>(type: "bit", nullable: false),
                    SetId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Set = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetSearchUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScryfallSetUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RulingsUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrintsSearchUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CollectorNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Digital = table.Column<bool>(type: "bit", nullable: false),
                    Rarity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardBackId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Artist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IllustrationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BorderColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frame = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullArt = table.Column<bool>(type: "bit", nullable: false),
                    Textless = table.Column<bool>(type: "bit", nullable: false),
                    Booster = table.Column<bool>(type: "bit", nullable: false),
                    StorySpotlight = table.Column<bool>(type: "bit", nullable: false),
                    EdhrecRank = table.Column<int>(type: "int", nullable: false),
                    FlavorText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PennyRank = table.Column<int>(type: "int", nullable: false),
                    MtgoFoilId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColorIdentityLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorIdentityLookUps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColorIndicatorLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorIndicatorLookUps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColorsLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorsLookUps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinishesLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishesLookUps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GamesLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamesLookUps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeywordsLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeywordsLookUps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreferredMarketLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferredMarketLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreferredPriceUnitLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferredPriceUnitLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rulings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OracleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rulings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScryfallArtistIdsLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScryfallArtistIdsLookUps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardFaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManaCost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OracleText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Power = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Toughness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlavorText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Artist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScryfallArtistId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IllustrationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlavorName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardFaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardFaces_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormatLegalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Standard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Future = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Historic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gladiator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pioneer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explorer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Legacy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pauper = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vintage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Penny = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Commander = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Brawl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Historicbrawl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alchemy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paupercommander = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Oldschool = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Premodern = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormatLegalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormatLegalities_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiverseIdsLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<int>(type: "int", nullable: true),
                    CardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiverseIdsLookUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiverseIdsLookUps_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Usd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UsdFoil = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UsdEtched = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Eur = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EurFoil = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Tix = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prices_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Tcgplayer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cardmarket = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cardhoarder = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseUris_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelatedUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    Gatherer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TcgplayerInfiniteArticles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TcgplayerInfiniteDecks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Edhrec = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelatedUris_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardColorIdentityLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: true),
                    ColorIdentityLookUpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardColorIdentityLookUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardColorIdentityLookUps_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CardColorIdentityLookUps_ColorIdentityLookUps_ColorIdentityLookUpId",
                        column: x => x.ColorIdentityLookUpId,
                        principalTable: "ColorIdentityLookUps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardFinishesLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    FinishesLookUpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardFinishesLookUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardFinishesLookUps_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardFinishesLookUps_FinishesLookUps_FinishesLookUpId",
                        column: x => x.FinishesLookUpId,
                        principalTable: "FinishesLookUps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardGamesLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    GamesLookUpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardGamesLookUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardGamesLookUps_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardGamesLookUps_GamesLookUps_GamesLookUpId",
                        column: x => x.GamesLookUpId,
                        principalTable: "GamesLookUps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardKeywordsLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    KeywordsLookUpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardKeywordsLookUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardKeywordsLookUps_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardKeywordsLookUps_KeywordsLookUps_KeywordsLookUpId",
                        column: x => x.KeywordsLookUpId,
                        principalTable: "KeywordsLookUps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredPriceUnitLookupId = table.Column<int>(type: "int", nullable: true),
                    PreferredMarketLookupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_PreferredMarketLookups_PreferredMarketLookupId",
                        column: x => x.PreferredMarketLookupId,
                        principalTable: "PreferredMarketLookups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_PreferredPriceUnitLookups_PreferredPriceUnitLookupId",
                        column: x => x.PreferredPriceUnitLookupId,
                        principalTable: "PreferredPriceUnitLookups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CardScryfallArtistIdsLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    ScryfallArtistIdsLookUpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardScryfallArtistIdsLookUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardScryfallArtistIdsLookUps_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardScryfallArtistIdsLookUps_ScryfallArtistIdsLookUps_ScryfallArtistIdsLookUpId",
                        column: x => x.ScryfallArtistIdsLookUpId,
                        principalTable: "ScryfallArtistIdsLookUps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardColorIndicatorLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: true),
                    CardFaceId = table.Column<int>(type: "int", nullable: true),
                    ColorIndicatorLookUpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardColorIndicatorLookUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardColorIndicatorLookUps_CardFaces_CardFaceId",
                        column: x => x.CardFaceId,
                        principalTable: "CardFaces",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CardColorIndicatorLookUps_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CardColorIndicatorLookUps_ColorIndicatorLookUps_ColorIndicatorLookUpId",
                        column: x => x.ColorIndicatorLookUpId,
                        principalTable: "ColorIndicatorLookUps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardColorsLookUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: true),
                    CardFaceId = table.Column<int>(type: "int", nullable: true),
                    ColorsLookUpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardColorsLookUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardColorsLookUps_CardFaces_CardFaceId",
                        column: x => x.CardFaceId,
                        principalTable: "CardFaces",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CardColorsLookUps_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CardColorsLookUps_ColorsLookUps_ColorsLookUpId",
                        column: x => x.ColorsLookUpId,
                        principalTable: "ColorsLookUps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: true),
                    CardFaceId = table.Column<int>(type: "int", nullable: true),
                    Small = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Normal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Large = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Png = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArtCrop = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BorderCrop = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageUris_CardFaces_CardFaceId",
                        column: x => x.CardFaceId,
                        principalTable: "CardFaces",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImageUris_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Primer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Views = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Decks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Packets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    DeckId = table.Column<int>(type: "int", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "Comments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeckCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeckId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckCategories_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    DeckId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Likes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PacketCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: true),
                    PacketId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacketCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PacketCards_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PacketCards_Packets_PacketId",
                        column: x => x.PacketId,
                        principalTable: "Packets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeckCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeckCategoryId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Board = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckCards_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeckCards_DeckCategories_DeckCategoryId",
                        column: x => x.DeckCategoryId,
                        principalTable: "DeckCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardColorIdentityLookUps_CardId",
                table: "CardColorIdentityLookUps",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardColorIdentityLookUps_ColorIdentityLookUpId",
                table: "CardColorIdentityLookUps",
                column: "ColorIdentityLookUpId");

            migrationBuilder.CreateIndex(
                name: "IX_CardColorIndicatorLookUps_CardFaceId",
                table: "CardColorIndicatorLookUps",
                column: "CardFaceId");

            migrationBuilder.CreateIndex(
                name: "IX_CardColorIndicatorLookUps_CardId",
                table: "CardColorIndicatorLookUps",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardColorIndicatorLookUps_ColorIndicatorLookUpId",
                table: "CardColorIndicatorLookUps",
                column: "ColorIndicatorLookUpId");

            migrationBuilder.CreateIndex(
                name: "IX_CardColorsLookUps_CardFaceId",
                table: "CardColorsLookUps",
                column: "CardFaceId");

            migrationBuilder.CreateIndex(
                name: "IX_CardColorsLookUps_CardId",
                table: "CardColorsLookUps",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardColorsLookUps_ColorsLookUpId",
                table: "CardColorsLookUps",
                column: "ColorsLookUpId");

            migrationBuilder.CreateIndex(
                name: "IX_CardFaces_CardId",
                table: "CardFaces",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardFinishesLookUps_CardId",
                table: "CardFinishesLookUps",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardFinishesLookUps_FinishesLookUpId",
                table: "CardFinishesLookUps",
                column: "FinishesLookUpId");

            migrationBuilder.CreateIndex(
                name: "IX_CardGamesLookUps_CardId",
                table: "CardGamesLookUps",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardGamesLookUps_GamesLookUpId",
                table: "CardGamesLookUps",
                column: "GamesLookUpId");

            migrationBuilder.CreateIndex(
                name: "IX_CardKeywordsLookUps_CardId",
                table: "CardKeywordsLookUps",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardKeywordsLookUps_KeywordsLookUpId",
                table: "CardKeywordsLookUps",
                column: "KeywordsLookUpId");

            migrationBuilder.CreateIndex(
                name: "IX_CardScryfallArtistIdsLookUps_CardId",
                table: "CardScryfallArtistIdsLookUps",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardScryfallArtistIdsLookUps_ScryfallArtistIdsLookUpId",
                table: "CardScryfallArtistIdsLookUps",
                column: "ScryfallArtistIdsLookUpId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId",
                table: "Categories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DeckId",
                table: "Comments",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCards_CardId",
                table: "DeckCards",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCards_DeckCategoryId",
                table: "DeckCards",
                column: "DeckCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCategories_DeckId",
                table: "DeckCategories",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_Decks_UserId",
                table: "Decks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FormatLegalities_CardId",
                table: "FormatLegalities",
                column: "CardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageUris_CardFaceId",
                table: "ImageUris",
                column: "CardFaceId",
                unique: true,
                filter: "[CardFaceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ImageUris_CardId",
                table: "ImageUris",
                column: "CardId",
                unique: true,
                filter: "[CardId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_DeckId",
                table: "Likes",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId",
                table: "Likes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiverseIdsLookUps_CardId",
                table: "MultiverseIdsLookUps",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_PacketCards_CardId",
                table: "PacketCards",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_PacketCards_PacketId",
                table: "PacketCards",
                column: "PacketId");

            migrationBuilder.CreateIndex(
                name: "IX_Packets_UserId",
                table: "Packets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_CardId",
                table: "Prices",
                column: "CardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseUris_CardId",
                table: "PurchaseUris",
                column: "CardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatedUris_CardId",
                table: "RelatedUris",
                column: "CardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PreferredMarketLookupId",
                table: "Users",
                column: "PreferredMarketLookupId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PreferredPriceUnitLookupId",
                table: "Users",
                column: "PreferredPriceUnitLookupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulkData");

            migrationBuilder.DropTable(
                name: "CardColorIdentityLookUps");

            migrationBuilder.DropTable(
                name: "CardColorIndicatorLookUps");

            migrationBuilder.DropTable(
                name: "CardColorsLookUps");

            migrationBuilder.DropTable(
                name: "CardFinishesLookUps");

            migrationBuilder.DropTable(
                name: "CardGamesLookUps");

            migrationBuilder.DropTable(
                name: "CardKeywordsLookUps");

            migrationBuilder.DropTable(
                name: "CardScryfallArtistIdsLookUps");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "DeckCards");

            migrationBuilder.DropTable(
                name: "FormatLegalities");

            migrationBuilder.DropTable(
                name: "ImageUris");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "MultiverseIdsLookUps");

            migrationBuilder.DropTable(
                name: "PacketCards");

            migrationBuilder.DropTable(
                name: "Prices");

            migrationBuilder.DropTable(
                name: "PurchaseUris");

            migrationBuilder.DropTable(
                name: "RelatedUris");

            migrationBuilder.DropTable(
                name: "Rulings");

            migrationBuilder.DropTable(
                name: "ColorIdentityLookUps");

            migrationBuilder.DropTable(
                name: "ColorIndicatorLookUps");

            migrationBuilder.DropTable(
                name: "ColorsLookUps");

            migrationBuilder.DropTable(
                name: "FinishesLookUps");

            migrationBuilder.DropTable(
                name: "GamesLookUps");

            migrationBuilder.DropTable(
                name: "KeywordsLookUps");

            migrationBuilder.DropTable(
                name: "ScryfallArtistIdsLookUps");

            migrationBuilder.DropTable(
                name: "DeckCategories");

            migrationBuilder.DropTable(
                name: "CardFaces");

            migrationBuilder.DropTable(
                name: "Packets");

            migrationBuilder.DropTable(
                name: "Decks");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PreferredMarketLookups");

            migrationBuilder.DropTable(
                name: "PreferredPriceUnitLookups");
        }
    }
}
