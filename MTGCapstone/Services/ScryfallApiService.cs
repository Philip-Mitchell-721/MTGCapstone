using AutoMapper;
using AutoMapper.Execution;
using Microsoft.EntityFrameworkCore;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Extentions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

namespace MTGCapstone.API.Services
{
    public class ScryfallApiService : IScryfallApiService
    {
        private readonly ILogger<ScryfallApiService> _logger;
        private readonly ScryfallClient _scryfallClient;
        private readonly CapstoneDbContext _capstoneDbContext;
        private readonly IMapper _mapper;

        public ScryfallApiService(
            ILogger<ScryfallApiService> logger,
            ScryfallClient scryfallClient,
            CapstoneDbContext capstoneDbContext,
            IMapper mapper)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _scryfallClient = scryfallClient
                ?? throw new ArgumentNullException(nameof(scryfallClient));
            _capstoneDbContext = capstoneDbContext
                ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task GetBulkDataSourcesAsync(CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage response = await _scryfallClient.Client
                    .GetAsync("https://api.scryfall.com/bulk-data", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                
                using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    BulkDataSource? bulkDataSource = stream.ReadAndDeserializeFromJson<BulkDataSource>();

                    if (bulkDataSource?.data is not null)
                    {
                        if (_capstoneDbContext.BulkData is not null)
                        {
                            //List<BulkData> bulkDataList = _capstoneDbContext.BulkData.ToList();
                            _capstoneDbContext.BulkData.RemoveRange(_capstoneDbContext.BulkData);

                            foreach (BulkDataDTO item in bulkDataSource.data)
                            {
                                //BulkData? bulkData = bulkDataList.FirstOrDefault(bd => bd.Type == item.type);
                                BulkData bulkData = new BulkData();
                                if (bulkData is not null)
                                {
                                    bulkData.ScryfallId = item.id;
                                    bulkData.Type = item.type;
                                    bulkData.UpdatedAt = item.updated_at;
                                    bulkData.Uri = item.uri;
                                    bulkData.Name = item.name;
                                    bulkData.Description = item.description;
                                    bulkData.CompressedSize = item.compressed_size;
                                    bulkData.DownloadUri = item.download_uri;
                                    bulkData.ContentType = item.content_type;
                                    bulkData.ContentEncoding = item.content_encoding;
                                    _capstoneDbContext.BulkData.Add(bulkData);
                                }
                                _capstoneDbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Warning, "Bulk Data recieved from capstoneDbContext was null");
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "Bulk Data recieved from Scryfall API was null");
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Warning, $"Status Code {response.StatusCode} recieved from Scryfall API");
                }

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Something happened");
            }
        }

        public async Task ImportRulingsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await GetBulkDataSourcesAsync(cancellationToken);
                if (_capstoneDbContext.BulkData is not null)
                {
                    BulkData? rulingsBulk = await _capstoneDbContext.BulkData.FirstOrDefaultAsync(bd => bd.Name == "Rulings");
                    if (rulingsBulk is not null)
                    {
                        HttpResponseMessage response = await _scryfallClient.Client
                            .GetAsync($"{rulingsBulk.DownloadUri}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                        using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            //List of RulingsDTOs from the stream
                            List<RulingsDTO>? bulkRulings = stream.ReadAndDeserializeFromJson<List<RulingsDTO>>();

                            if (bulkRulings is not null && _capstoneDbContext.Rulings is not null)
                            {
                                //Mapping previous List<RulingsDTO> to List<Ruling>
                                List<Ruling> rulings = new List<Ruling>();

                                foreach (RulingsDTO item in bulkRulings)
                                {
                                    Ruling incomingRuling = new Ruling();
                                    if (item is not null)
                                    {
                                        incomingRuling.OracleId = item.oracle_id;
                                        incomingRuling.Source = item.source;
                                        incomingRuling.PublishedAt = item.published_at;
                                        incomingRuling.Comment = item.comment;
                                        rulings.Add(incomingRuling);
                                    }
                                }
                                //BulkMergeAsync makes a new temp table for List<Ruling> from above.
                                //It then merges this new table with my Rulings table.
                                await _capstoneDbContext.Rulings.BulkMergeAsync(rulings);
                            }
                            else
                            {
                                _logger.Log(LogLevel.Warning, "Bulk Data recieved from Scryfall API was null");
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Warning, $"Status Code {response.StatusCode} recieved from Scryfall API");
                        }

                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "Rulings BulkData recieved from capstoneDbContext was null");
                    }

                }
                else
                {
                    _logger.Log(LogLevel.Warning, "Bulk Data recieved from capstoneDbContext was null");
                }

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Something happened");
            }
        }

        public async Task ImportCardsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await GetBulkDataSourcesAsync(cancellationToken);
                if (_capstoneDbContext.BulkData is not null)
                {
                    BulkData? allCardsBulkData = await _capstoneDbContext.BulkData.FirstOrDefaultAsync(bd => bd.Name == "All Cards");
                    if (allCardsBulkData is not null)
                    {
                        HttpResponseMessage response = await _scryfallClient.Client
                            .GetAsync($"{allCardsBulkData.DownloadUri}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                        using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            //List of ScryfallCards from the stream
                            //List<ScryfallCard>? allScryfallCards = stream.ReadAndDeserializeFromJson<List<ScryfallCard>>();
                            List<ScryfallCard>? allScryfallCards = stream.ReadAndDeserializeFromJson<List<ScryfallCard>>();

                            if (allScryfallCards is not null && _capstoneDbContext.Cards is not null)
                            {

                                List<Card> cards = new List<Card>();
                                List<ColorsLookUp> colorsLookUps = _capstoneDbContext.ColorsLookUps?.ToList() ?? new List<ColorsLookUp>();
                                List<ColorIndicatorLookUp> colorIndicatorLookUps = _capstoneDbContext.ColorIndicatorLookUps?.ToList() ?? new List<ColorIndicatorLookUp>();
                                List<ColorIdentityLookUp> colorIdentityLookUps = _capstoneDbContext.ColorIdentityLookUps?.ToList() ?? new List<ColorIdentityLookUp>();
                                List<FinishesLookUp> finishesLookUps = _capstoneDbContext.FinishesLookUps?.ToList() ?? new List<FinishesLookUp>();
                                List<GamesLookUp> gamesLookUps = _capstoneDbContext.GamesLookUps?.ToList() ?? new List<GamesLookUp>();
                                List<KeywordsLookUp> keywordsLookUps = _capstoneDbContext.KeywordsLookUps?.ToList() ?? new List<KeywordsLookUp>();
                                List<ScryfallArtistIdsLookUp> scryfallArtistIdsLookUps = _capstoneDbContext.ScryfallArtistIdsLookUps?.ToList() ?? new List<ScryfallArtistIdsLookUp>();



                                //Mapping previous List<ScryfallCard> to List<Card>
                                foreach (ScryfallCard scryfallCard in allScryfallCards)
                                {
                                    if (scryfallCard is not null)
                                    {

                                        Card incomingCard = MapScryfallCardToCard(scryfallCard);

                                        cards.Add(incomingCard);


                                        #region Creating needed lookups
                                        if (scryfallCard.artist_ids is not null)
                                        {
                                            foreach (string artistId in scryfallCard.artist_ids)
                                            {
                                                if (!scryfallArtistIdsLookUps.Any(c => c.Value == artistId))
                                                {
                                                    ScryfallArtistIdsLookUp newArtistId = new ScryfallArtistIdsLookUp() { Value = artistId };
                                                    scryfallArtistIdsLookUps.Add(newArtistId);
                                                }
                                            }
                                        }
                                        if (scryfallCard.keywords is not null)
                                        {
                                            foreach (string keyword in scryfallCard.keywords)
                                            {
                                                if (!keywordsLookUps.Any(c => c.Value == keyword))
                                                {
                                                    KeywordsLookUp newKeyword = new KeywordsLookUp() { Value = keyword };
                                                    keywordsLookUps.Add(newKeyword);
                                                }
                                            }
                                        }
                                        if (scryfallCard.games is not null)
                                        {
                                            foreach (string game in scryfallCard.games)
                                            {
                                                if (!gamesLookUps.Any(c => c.Value == game))
                                                {
                                                    GamesLookUp newGame = new GamesLookUp() { Value = game };
                                                    gamesLookUps.Add(newGame);
                                                }
                                            }
                                        }
                                        if (scryfallCard.finishes is not null)
                                        {
                                            foreach (string finish in scryfallCard.finishes)
                                            {
                                                if (!finishesLookUps.Any(c => c.Value == finish))
                                                {
                                                    FinishesLookUp newFinish = new FinishesLookUp() { Value = finish };
                                                    finishesLookUps.Add(newFinish);
                                                }
                                            }
                                        }
                                        if (scryfallCard.color_identity is not null)
                                        {
                                            foreach (string colorIdentity in scryfallCard.color_identity)
                                            {
                                                if (!colorIdentityLookUps.Any(c => c.Value == colorIdentity))
                                                {
                                                    ColorIdentityLookUp newColorIdentity = new ColorIdentityLookUp() { Value = colorIdentity };
                                                    colorIdentityLookUps.Add(newColorIdentity);
                                                }
                                            }
                                        }
                                        if (scryfallCard.color_indicator is not null)
                                        {
                                            foreach (string colorIndicator in scryfallCard.color_indicator)
                                            {
                                                if (!colorIndicatorLookUps.Any(c => c.Value == colorIndicator))
                                                {
                                                    ColorIndicatorLookUp newColorIndicator = new ColorIndicatorLookUp() { Value = colorIndicator };
                                                    colorIndicatorLookUps.Add(newColorIndicator);
                                                }
                                            }
                                        }
                                        if (scryfallCard.colors is not null)
                                        {
                                            foreach (string color in scryfallCard.colors)
                                            {
                                                if (!colorsLookUps.Any(c => c.Value == color))
                                                {
                                                    ColorsLookUp newColor = new ColorsLookUp() { Value = color };
                                                    colorsLookUps.Add(newColor);
                                                }
                                            }
                                        }

                                        #endregion
                                    }
                                }

                                //BulkMergeAsync makes a new temp table for List<Card> from above.
                                //It then merges this new table with my Cards table.
                                await _capstoneDbContext.Cards.BulkMergeAsync(cards);
                                await _capstoneDbContext.ColorsLookUps.BulkMergeAsync(colorsLookUps);
                                await _capstoneDbContext.ColorIndicatorLookUps.BulkMergeAsync(colorIndicatorLookUps);
                                await _capstoneDbContext.ColorIdentityLookUps.BulkMergeAsync(colorIdentityLookUps);
                                await _capstoneDbContext.FinishesLookUps.BulkMergeAsync(finishesLookUps);
                                await _capstoneDbContext.GamesLookUps.BulkMergeAsync(gamesLookUps);
                                await _capstoneDbContext.KeywordsLookUps.BulkMergeAsync(keywordsLookUps);
                                await _capstoneDbContext.ScryfallArtistIdsLookUps.BulkMergeAsync(scryfallArtistIdsLookUps);

                                List<CardFace> cardFaces = new List<CardFace>();
                                foreach (ScryfallCard scryfallCard in allScryfallCards)
                                {
                                    if (scryfallCard.card_faces is not null)
                                    {
                                        Card? cardToMapFaceTo = cards.FirstOrDefault(c => c.ScryfallId == scryfallCard.id);
                                        if (cardToMapFaceTo is not null)
                                        {
                                            MapCardFaceDataToCard(cardToMapFaceTo, scryfallCard);
                                            cardFaces.AddRange(cardToMapFaceTo.CardFaces);
                                        }
                                    }

                                }
                                await _capstoneDbContext.CardFaces.BulkMergeAsync(cardFaces);

                                List<MultiverseIdsLookUp> multiverseIdsLookUps = new List<MultiverseIdsLookUp>();
                                List<ImageUris> imageUris = new List<ImageUris>();
                                List<CardColorsLookUp> cardcolorsLookUps = new List<CardColorsLookUp>();
                                List<CardColorIndicatorLookUp> cardColorIndicatorLookUps = new List<CardColorIndicatorLookUp>();
                                List<CardColorIdentityLookUp> cardColorIdentityLookUps = new List<CardColorIdentityLookUp>();
                                List<CardKeywordsLookUp> cardKeywordsLookUps = new List<CardKeywordsLookUp>();
                                List<FormatLegalities> formatLegalities = new List<FormatLegalities>();
                                List<CardGamesLookUp> cardGamesLookUps = new List<CardGamesLookUp>();
                                List<CardFinishesLookUp> cardFinishesLookUps = new List<CardFinishesLookUp>();
                                List<CardScryfallArtistIdsLookUp> cardScryfallArtistIdsLookUps = new List<CardScryfallArtistIdsLookUp>();
                                List<Prices> prices = new List<Prices>();
                                List<RelatedUris> relatedUris = new List<RelatedUris>();
                                List<PurchaseUris> purchaseUris = new List<PurchaseUris>();
                                int incrementCount = 0;

                                foreach (ScryfallCard scryfallCard in allScryfallCards)
                                {
                                    Card? cardToMapRelatedDataTo = cards.FirstOrDefault(c => c.ScryfallId == scryfallCard.id);
                                    if (cardToMapRelatedDataTo is not null)
                                    {
                                        MapRelatedDataToCard(cardToMapRelatedDataTo,
                                        scryfallCard,
                                        colorsLookUps,
                                        colorIndicatorLookUps,
                                        colorIdentityLookUps,
                                        keywordsLookUps,
                                        gamesLookUps,
                                        finishesLookUps,
                                        scryfallArtistIdsLookUps);

                                        
                                        multiverseIdsLookUps.AddRange(cardToMapRelatedDataTo.MultiverseIds);
                                        if(cardToMapRelatedDataTo.ImageUris is not null) imageUris.Add(cardToMapRelatedDataTo.ImageUris);
                                        cardcolorsLookUps.AddRange(cardToMapRelatedDataTo.Colors);
                                        cardColorIndicatorLookUps.AddRange(cardToMapRelatedDataTo.ColorIndicator);
                                        cardColorIdentityLookUps.AddRange(cardToMapRelatedDataTo.ColorIdentity);
                                        cardKeywordsLookUps.AddRange(cardToMapRelatedDataTo.Keywords);
                                        if (cardToMapRelatedDataTo.Legalities is not null) formatLegalities.Add(cardToMapRelatedDataTo.Legalities);
                                        cardGamesLookUps.AddRange(cardToMapRelatedDataTo.Games);
                                        cardFinishesLookUps.AddRange(cardToMapRelatedDataTo.Finishes);
                                        cardScryfallArtistIdsLookUps.AddRange(cardToMapRelatedDataTo.ScryfallArtistIds);
                                        if (cardToMapRelatedDataTo.Prices is not null) prices.Add(cardToMapRelatedDataTo.Prices);
                                        if (cardToMapRelatedDataTo.RelatedUris is not null) relatedUris.Add(cardToMapRelatedDataTo.RelatedUris);
                                        if (cardToMapRelatedDataTo.PurchaseUris is not null) purchaseUris.Add(cardToMapRelatedDataTo.PurchaseUris);
                                        if (cardToMapRelatedDataTo.CardFaces is not null)
                                        {
                                            foreach (CardFace cardFace in cardToMapRelatedDataTo.CardFaces)
                                            {
                                                cardcolorsLookUps.AddRange(cardFace.Colors);
                                                cardColorIndicatorLookUps.AddRange(cardFace.ColorIndicator);
                                                if (cardFace.ImageUris is not null) imageUris.Add(cardFace.ImageUris);

                                            }
                                        }
                                    }
                                    incrementCount++;
                                    if (incrementCount >= 1000)
                                    {
                                        await _capstoneDbContext.MultiverseIdsLookUps.BulkMergeAsync(multiverseIdsLookUps);
                                        await _capstoneDbContext.ImageUris.BulkMergeAsync(imageUris);
                                        await _capstoneDbContext.CardColorsLookUps.BulkMergeAsync(cardcolorsLookUps);
                                        await _capstoneDbContext.CardColorIndicatorLookUps.BulkMergeAsync(cardColorIndicatorLookUps);
                                        await _capstoneDbContext.CardColorIdentityLookUps.BulkMergeAsync(cardColorIdentityLookUps);
                                        await _capstoneDbContext.CardKeywordsLookUps.BulkMergeAsync(cardKeywordsLookUps);
                                        await _capstoneDbContext.FormatLegalities.BulkMergeAsync(formatLegalities);
                                        await _capstoneDbContext.CardGamesLookUps.BulkMergeAsync(cardGamesLookUps);
                                        await _capstoneDbContext.CardFinishesLookUps.BulkMergeAsync(cardFinishesLookUps);
                                        await _capstoneDbContext.CardScryfallArtistIdsLookUps.BulkMergeAsync(cardScryfallArtistIdsLookUps);
                                        await _capstoneDbContext.Prices.BulkMergeAsync(prices);
                                        await _capstoneDbContext.RelatedUris.BulkMergeAsync(relatedUris);
                                        await _capstoneDbContext.PurchaseUris.BulkMergeAsync(purchaseUris);

                                        multiverseIdsLookUps.Clear();
                                        imageUris.Clear();
                                        cardcolorsLookUps.Clear();
                                        cardColorIndicatorLookUps.Clear();
                                        cardColorIdentityLookUps.Clear();
                                        cardKeywordsLookUps.Clear();
                                        formatLegalities.Clear();
                                        cardGamesLookUps.Clear();
                                        cardFinishesLookUps.Clear();
                                        cardScryfallArtistIdsLookUps.Clear();
                                        prices.Clear();
                                        relatedUris.Clear();
                                        purchaseUris.Clear();
                                        incrementCount = 0;
                                    }
                                }
                                await _capstoneDbContext.MultiverseIdsLookUps.BulkMergeAsync(multiverseIdsLookUps);
                                await _capstoneDbContext.ImageUris.BulkMergeAsync(imageUris);
                                await _capstoneDbContext.CardColorsLookUps.BulkMergeAsync(cardcolorsLookUps);
                                await _capstoneDbContext.CardColorIndicatorLookUps.BulkMergeAsync(cardColorIndicatorLookUps);
                                await _capstoneDbContext.CardColorIdentityLookUps.BulkMergeAsync(cardColorIdentityLookUps);
                                await _capstoneDbContext.CardKeywordsLookUps.BulkMergeAsync(cardKeywordsLookUps);
                                await _capstoneDbContext.FormatLegalities.BulkMergeAsync(formatLegalities);
                                await _capstoneDbContext.CardGamesLookUps.BulkMergeAsync(cardGamesLookUps);
                                await _capstoneDbContext.CardFinishesLookUps.BulkMergeAsync(cardFinishesLookUps);
                                await _capstoneDbContext.CardScryfallArtistIdsLookUps.BulkMergeAsync(cardScryfallArtistIdsLookUps);
                                await _capstoneDbContext.Prices.BulkMergeAsync(prices);
                                await _capstoneDbContext.RelatedUris.BulkMergeAsync(relatedUris);
                                await _capstoneDbContext.PurchaseUris.BulkMergeAsync(purchaseUris);
                            }
                            else
                            {
                                _logger.Log(LogLevel.Warning, "Bulk Data recieved from Scryfall API was null");
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Warning, "Status Code {response.StatusCode} recieved from Scryfall API", response.StatusCode);
                        }

                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "allCardsBulkData recieved from capstoneDbContext was null");
                    }

                }
                else
                {
                    _logger.Log(LogLevel.Warning, "Bulk Data recieved from capstoneDbContext was null");
                }

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Unhandled Error");
            }
        }

        private Card MapScryfallCardToCard(ScryfallCard scryfallCard)
        {
            Card newCard = new Card();

            //newCard.Id = new Guid();
            newCard.ScryfallId = scryfallCard.id;
            newCard.OracleId = scryfallCard.oracle_id;
            newCard.MtgoId = scryfallCard.mtgo_id;
            newCard.TcgplayerId = scryfallCard.tcgplayer_id;
            newCard.CardmarketId = scryfallCard.cardmarket_id;
            newCard.Name = scryfallCard.name;
            newCard.Language = scryfallCard.lang;
            newCard.ReleasedAt = scryfallCard.released_at;
            newCard.Uri = scryfallCard.uri;
            newCard.ScryfallUri = scryfallCard.scryfall_uri;
            newCard.Layout = scryfallCard.layout;
            newCard.HighresImage = scryfallCard.highres_image;
            newCard.ImageStatus = scryfallCard.image_status;
            newCard.ManaCost = scryfallCard.mana_cost;
            newCard.Cmc = scryfallCard.cmc;
            newCard.TypeLine = scryfallCard.type_line;
            newCard.OracleText = scryfallCard.oracle_text;
            newCard.Power = scryfallCard.power;
            newCard.Toughness = scryfallCard.toughness;
            newCard.Reserved = scryfallCard.reserved;
            newCard.Foil = scryfallCard.foil;
            newCard.Nonfoil = scryfallCard.nonfoil;
            newCard.Oversized = scryfallCard.oversized;
            newCard.Promo = scryfallCard.promo;
            newCard.Reprint = scryfallCard.reprint;
            newCard.Variation = scryfallCard.variation;
            newCard.SetId = scryfallCard.SetId;
            newCard.Set = scryfallCard.set;
            newCard.SetName = scryfallCard.SetName;
            newCard.SetType = scryfallCard.set_type;
            newCard.SetUri = scryfallCard.SetUri;
            newCard.SetSearchUri = scryfallCard.set_search_uri;
            newCard.ScryfallSetUri = scryfallCard.scryfall_set_uri;
            newCard.RulingsUri = scryfallCard.rulings_uri;
            newCard.PrintsSearchUri = scryfallCard.prints_search_uri;
            newCard.CollectorNumber = scryfallCard.collector_number;
            newCard.Digital = scryfallCard.digital;
            newCard.Rarity = scryfallCard.rarity;
            newCard.CardBackId = scryfallCard.card_back_id;
            newCard.Artist = scryfallCard.artist;
            newCard.IllustrationId = scryfallCard.illustration_id;
            newCard.BorderColor = scryfallCard.border_color;
            newCard.Frame = scryfallCard.frame;
            newCard.FullArt = scryfallCard.full_art;
            newCard.Textless = scryfallCard.textless;
            newCard.Booster = scryfallCard.booster;
            newCard.StorySpotlight = scryfallCard.story_spotlight;
            newCard.EdhrecRank = scryfallCard.edhrec_rank;
            newCard.FlavorText = scryfallCard.flavor_text;
            newCard.PennyRank = scryfallCard.penny_rank;
            newCard.MtgoFoilId = scryfallCard.mtgo_foil_id;

            return newCard;
        }

        private void MapRelatedDataToCard(Card card,
            ScryfallCard scryfallCard,
            List<ColorsLookUp> colorsLookUps,
            List<ColorIndicatorLookUp> colorIndicatorLookUps,
            List<ColorIdentityLookUp> colorIdentityLookUps,
            List<KeywordsLookUp> keywordsLookUps,
            List<GamesLookUp> gamesLookUps,
            List<FinishesLookUp> finishesLookUps,
            List<ScryfallArtistIdsLookUp> scryfallArtistIdsLookUps)
        {
            //map multiverseIds
            if (scryfallCard.multiverse_ids is not null)
            {
                foreach (int lookup in scryfallCard.multiverse_ids)
                {
                    MultiverseIdsLookUp multiverseId = new MultiverseIdsLookUp()
                    {
                        Value = lookup,
                        CardId = card.Id,
                        Card = card
                    };
                    card.MultiverseIds.Add(multiverseId);
                }
            }

            //map ImageUris
            if (scryfallCard.image_uris is not null)
            {

                card.ImageUris = new ImageUris()
                {
                    CardId = card.Id,
                    Small = scryfallCard.image_uris.small,
                    Normal = scryfallCard.image_uris.normal,
                    Large = scryfallCard.image_uris.large,
                    Png = scryfallCard.image_uris.png,
                    ArtCrop = scryfallCard.image_uris.art_crop,
                    BorderCrop = scryfallCard.image_uris.border_crop
                };
            }

            //map Colors
            if (scryfallCard.colors is not null)
            {
                foreach (string color in scryfallCard.colors)
                {
                    ColorsLookUp? colorsLookUp = colorsLookUps.FirstOrDefault(c => c.Value == color);
                    if (colorsLookUp is not null)
                    {
                        CardColorsLookUp cardColorsLookUp = new CardColorsLookUp()
                        {
                            CardId = card.Id,
                            Card = card,
                            ColorsLookUpId = colorsLookUp.Id,
                            ColorsLookUp = colorsLookUp
                        };
                        card.Colors.Add(cardColorsLookUp);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "Color on {card.Name} could not be matched to ColorLookUp in database.", card.Name);
                    }
                }
            }

            //map ColorIndicators
            if (scryfallCard.color_indicator is not null)
            {
                foreach (string color in scryfallCard.color_indicator)
                {
                    ColorIndicatorLookUp? colorIndicatorLookUp = colorIndicatorLookUps.FirstOrDefault(c => c.Value == color);
                    if (colorIndicatorLookUp is not null)
                    {
                        CardColorIndicatorLookUp cardColorIndicatorLookUp = new CardColorIndicatorLookUp()
                        {
                            CardId = card.Id,
                            Card = card,
                            ColorIndicatorLookUpId = colorIndicatorLookUp.Id,
                            ColorIndicatorLookUp = colorIndicatorLookUp
                        };
                        card.ColorIndicator.Add(cardColorIndicatorLookUp);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "ColorIndicator on {card.Name} could not be matched to ColorIndicatorLookUp in database.", card.Name);

                    }
                }
            }

            //map ColorIdentity
            if (scryfallCard.color_identity is not null)
            {
                foreach (string color in scryfallCard.color_identity)
                {
                    ColorIdentityLookUp? colorIdentityLookUp = colorIdentityLookUps.FirstOrDefault(c => c.Value == color);
                    if (colorIdentityLookUp is not null)
                    {
                        CardColorIdentityLookUp cardColorIdentityLookUp = new CardColorIdentityLookUp()
                        {
                            CardId = card.Id,
                            Card = card,
                            ColorIdentityLookUpId = colorIdentityLookUp.Id,
                            ColorIdentityLookUp = colorIdentityLookUp
                        };
                        card.ColorIdentity.Add(cardColorIdentityLookUp);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "ColorIdentity on {card.Name} could not be matched to ColorIdentityLookUp in database.", card.Name);
                    }
                }
            }

            //map Keywords
            if (scryfallCard.keywords is not null)
            {
                foreach (string keyword in scryfallCard.keywords)
                {
                    KeywordsLookUp? keywordsLookUp = keywordsLookUps.FirstOrDefault(c => c.Value == keyword);
                    if (keywordsLookUp is not null)
                    {
                        CardKeywordsLookUp cardKeywordsLookUp = new CardKeywordsLookUp()
                        {
                            CardId = card.Id,
                            Card = card,
                            KeywordsLookUpId = keywordsLookUp.Id,
                            KeywordsLookUp = keywordsLookUp
                        };
                        card.Keywords.Add(cardKeywordsLookUp);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "Keyword on {card.Name} could not be matched to KeywordsLookUp in database.", card.Name);
                    }
                }
            }

            //map FormatLegalities
            if (scryfallCard.legalities is not null)
            {

                card.Legalities = new FormatLegalities()
                {
                    CardId = card.Id,
                    Standard = scryfallCard.legalities.standard,
                    Future = scryfallCard.legalities.future,
                    Historic = scryfallCard.legalities.historic,
                    Gladiator = scryfallCard.legalities.gladiator,
                    Pioneer = scryfallCard.legalities.pioneer,
                    Explorer = scryfallCard.legalities.explorer,
                    Modern = scryfallCard.legalities.modern,
                    Legacy = scryfallCard.legalities.legacy,
                    Pauper = scryfallCard.legalities.pauper,
                    Vintage = scryfallCard.legalities.vintage,
                    Penny = scryfallCard.legalities.penny,
                    Commander = scryfallCard.legalities.commander,
                    Brawl = scryfallCard.legalities.brawl,
                    Historicbrawl = scryfallCard.legalities.historicbrawl,
                    Alchemy = scryfallCard.legalities.alchemy,
                    Paupercommander = scryfallCard.legalities.paupercommander,
                    Duel = scryfallCard.legalities.duel,
                    Oldschool = scryfallCard.legalities.oldschool,
                    Premodern = scryfallCard.legalities.premodern
                };
            }

            //map Games
            if (scryfallCard.games is not null)
            {
                foreach (string game in scryfallCard.games)
                {
                    GamesLookUp? gamesLookUp = gamesLookUps.FirstOrDefault(c => c.Value == game);
                    if (gamesLookUp is not null)
                    {
                        CardGamesLookUp cardgamesLookUp = new CardGamesLookUp()
                        {
                            CardId = card.Id,
                            Card = card,
                            GamesLookUpId = gamesLookUp.Id,
                            GamesLookUp = gamesLookUp
                        };
                        card.Games.Add(cardgamesLookUp);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "Game on {card.Name} could not be matched to GamesLookUp in database.", card.Name);
                    }
                }
            }

            //map Finishes
            if (scryfallCard.finishes is not null)
            {
                foreach (string finish in scryfallCard.finishes)
                {
                    FinishesLookUp? finishesLookUp = finishesLookUps.FirstOrDefault(c => c.Value == finish);
                    if (finishesLookUp is not null)
                    {
                        CardFinishesLookUp cardFinishesLookUp = new CardFinishesLookUp()
                        {
                            CardId = card.Id,
                            Card = card,
                            FinishesLookUpId = finishesLookUp.Id,
                            FinishesLookUp = finishesLookUp
                        };
                        card.Finishes.Add(cardFinishesLookUp);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, $"Finish on {card.Name} could not be matched to FinishesLookUp in database.");
                    }
                }
            }

            //map ArtistIds
            if (scryfallCard.artist_ids is not null)
            {
                foreach (string artistId in scryfallCard.artist_ids)
                {
                    ScryfallArtistIdsLookUp? artistIdLookUp = scryfallArtistIdsLookUps.FirstOrDefault(c => c.Value == artistId);
                    if (artistIdLookUp is not null)
                    {
                        CardScryfallArtistIdsLookUp cardArtistIdLookUp = new CardScryfallArtistIdsLookUp()
                        {
                            CardId = card.Id,
                            Card = card,
                            ScryfallArtistIdsLookUpId = artistIdLookUp.Id,
                            ScryfallArtistIdsLookUp = artistIdLookUp
                        };
                        card.ScryfallArtistIds.Add(cardArtistIdLookUp);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "Artist on {card.Name} could not be matched to ArtistIdLookUp in database.", card.Name);
                    }
                }
            }

            //map Prices
            if (scryfallCard.prices is not null)
            {
                card.Prices = new Prices() { CardId = card.Id };
                if (scryfallCard.prices.usd is not null) card.Prices.Usd = Decimal.Parse(scryfallCard.prices.usd);
                if (scryfallCard.prices.usd_foil is not null) card.Prices.UsdFoil = Decimal.Parse(scryfallCard.prices.usd_foil);
                if (scryfallCard.prices.usd_etched is not null) card.Prices.UsdEtched = Decimal.Parse(scryfallCard.prices.usd_etched);
                if (scryfallCard.prices.eur is not null) card.Prices.Eur = Decimal.Parse(scryfallCard.prices.eur);
                if (scryfallCard.prices.eur_foil is not null) card.Prices.EurFoil = Decimal.Parse(scryfallCard.prices.eur_foil);
                if (scryfallCard.prices.tix is not null) card.Prices.Tix = Decimal.Parse(scryfallCard.prices.tix);
            }

            //map RelatedUris
            if (scryfallCard.related_uris is not null)
            {
                card.RelatedUris = new RelatedUris()
                {
                    CardId = card.Id,
                    Gatherer = scryfallCard.related_uris.gatherer,
                    TcgplayerInfiniteArticles = scryfallCard.related_uris.tcgplayer_infinite_articles,
                    TcgplayerInfiniteDecks = scryfallCard.related_uris.tcgplayer_infinite_decks,
                    Edhrec = scryfallCard.related_uris.edhrec
                };
            }

            //map PurchaseUris
            if (scryfallCard.purchase_uris is not null)
            {
                card.PurchaseUris = new PurchaseUris()
                {
                    CardId = card.Id,
                    Tcgplayer = scryfallCard.purchase_uris.tcgplayer,
                    Cardmarket = scryfallCard.purchase_uris.cardmarket,
                    Cardhoarder = scryfallCard.purchase_uris.cardhoarder
                };
            }

            //map CardFaces
            if (scryfallCard.card_faces is not null)
            {
                foreach (Card_Face scryfallCardFace in scryfallCard.card_faces)
                {
                    CardFace? entityCardFace = card.CardFaces.FirstOrDefault(c => c.CardId == card.Id);
                    if (entityCardFace is not null)
                    {
                        //map Colors to CardFace
                        if (scryfallCardFace.colors is not null)
                        {
                            foreach (string color in scryfallCardFace.colors)
                            {

                                ColorsLookUp? colorsLookUp = colorsLookUps.FirstOrDefault(c => c.Value == color);
                                if (colorsLookUp is not null)
                                {
                                    CardColorsLookUp cardColorsLookUp = new CardColorsLookUp()
                                    {
                                        CardFaceId = entityCardFace.Id,
                                        CardFace = entityCardFace,
                                        ColorsLookUpId = colorsLookUp.Id,
                                        ColorsLookUp = colorsLookUp
                                    };
                                    entityCardFace.Colors.Add(cardColorsLookUp);
                                }
                                else
                                {
                                    _logger.Log(LogLevel.Warning, $"Color on {card.Name}/{entityCardFace.Name} could not be matched to ColorLookUp in database.");
                                }
                            }
                        }
                        //map ColorIndicators to CardFace
                        if (scryfallCardFace.color_indicator is not null)
                        {
                            foreach (string color in scryfallCardFace.color_indicator)
                            {
                                ColorIndicatorLookUp? colorIndicatorLookUp = colorIndicatorLookUps.FirstOrDefault(c => c.Value == color);
                                if (colorIndicatorLookUp is not null)
                                {
                                    CardColorIndicatorLookUp cardColorIndicatorLookUp = new CardColorIndicatorLookUp()
                                    {
                                        CardFaceId = entityCardFace.Id,
                                        CardFace = entityCardFace,
                                        ColorIndicatorLookUpId = colorIndicatorLookUp.Id,
                                        ColorIndicatorLookUp = colorIndicatorLookUp
                                    };
                                    entityCardFace.ColorIndicator.Add(cardColorIndicatorLookUp);
                                }
                                else
                                {
                                    _logger.Log(LogLevel.Warning, $"ColorIndicator on {card.Name}/{entityCardFace.Name} could not be matched to ColorIndicatorLookUp in database.");
                                }
                            }
                        }
                        //map ImageUris to CardFace
                        if (scryfallCardFace.image_uris is not null)
                        {
                            entityCardFace.ImageUris = new ImageUris()
                            {
                                CardFaceId = entityCardFace.Id,
                                Small = scryfallCardFace.image_uris.small,
                                Normal = scryfallCardFace.image_uris.normal,
                                Large = scryfallCardFace.image_uris.large,
                                Png = scryfallCardFace.image_uris.png,
                                ArtCrop = scryfallCardFace.image_uris.art_crop,
                                BorderCrop = scryfallCardFace.image_uris.border_crop
                            };
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, "CardFace on {card.Name} could not be matched to CardFaces in database.", card.Name);
                    }
                }
            }
        }

        public static void MapCardFaceDataToCard(Card card, ScryfallCard scryfallCard)
        {
            if (scryfallCard.card_faces is not null)
            {
                foreach (Card_Face face in scryfallCard.card_faces)
                {
                    CardFace newFace = new CardFace()
                    {
                        CardId = card.Id,
                        Name = face.name,
                        ManaCost = face.mana_cost,
                        TypeLine = face.type_line,
                        OracleText = face.oracle_text,
                        Power = face.power,
                        Toughness = face.toughness,
                        FlavorText = face.flavor_text,
                        Artist = face.artist,
                        ScryfallArtistId = face.artist_id,
                        IllustrationId = face.illustration_id,
                        FlavorName = face.flavor_name
                    };
                    card.CardFaces.Add(newFace);
                }
            }
        }
    }
}
