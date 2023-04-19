using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Responses;
using MTGCapstone.API.Data.ViewModels;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Extentions.LoggerMessages;
using MTGCapstone.API.Services.DomainServiceInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace MTGCapstone.API.Services.DomainServices
{
    public class DeckService : IDeckService
    {
        private readonly CapstoneDbContext _capstoneDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<DeckService> _logger;

        public DeckService(CapstoneDbContext capstoneDbContext,
            IMapper mapper,
            ILogger<DeckService> logger)
        {
            _capstoneDbContext = capstoneDbContext ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = _capstoneDbContext.ColorIdentityLookUps.ToList();
            _ = _capstoneDbContext.ColorsLookUps.ToList();
            _ = _capstoneDbContext.FinishesLookUps.ToList();
            _ = _capstoneDbContext.ColorIdentityLookUps.ToList();
        }


    //Decks
        public async Task<Response<List<DeckVM>?>> GetDecksAsync(GetDecksRequest getDecksRequest)
        {
            IQueryable<Deck> collection = _capstoneDbContext.Decks;

            if (getDecksRequest.UserName is not null)
            {
                var user = await _capstoneDbContext.Users.FirstOrDefaultAsync(u => u.UserName ==  getDecksRequest.UserName);
                if (user is not null)
                {
                    var userId = user.Id;
                    collection = collection.Where(deck => deck.UserId == userId);
                }
            }

            if (getDecksRequest.Format is not null)
            {
                collection = collection.Where(deck => deck.Format == getDecksRequest.Format);
            }

            if (getDecksRequest.Commander is not null)
            {

                collection = collection.Where(deck => deck.DeckCategories
                                                        .Any(dc => dc.DeckCategoryDeckCards
                                                                    .Any(dcdc => dcdc.DeckCard != null
                                                                                && dcdc.DeckCard.Card != null
                                                                                && dcdc.DeckCard.Card.Name != null
                                                                                && dcdc.DeckCard.Card.Name.ToLower() == getDecksRequest.Commander.ToLower())));

            }

            getDecksRequest.OrderBy ??= "Views";
            if (getDecksRequest.OrderBy != "Views")
            {
                DeckVM deckVM = new();
                var orderByInfo = deckVM.GetType().GetProperty(getDecksRequest.OrderBy);
                if (orderByInfo is not null)
                {

                    getDecksRequest.OrderBy = orderByInfo.Name;
                }
                else
                {
                    getDecksRequest.OrderBy = "Views";
                }
            }

                .OrderBy(deck => deck.GetType().GetProperty(getDecksRequest.OrderBy));


            var collectionToReturn = await collection.Skip(getDecksRequest.PageSize * (getDecksRequest.PageNumber - 1))
                .Take(getDecksRequest.PageSize)
                .ToListAsync();

            return _mapper.Map<List<DeckVM>>(collectionToReturn);

        }
        public async Task<DeckForUpdateResponse> GetDeckForUpdateDTOAsync(int userId, int deckId)
        {
            var response = await GetDeckForOwnerAsync(userId, deckId);
            DeckForUpdateResponse updateResponse = new();
            if (!response.DeckExists)
            {
                return updateResponse;
            }
            updateResponse.DeckExists = true;
            if (!response.IsOwner)
            {
                return updateResponse;
            }
            updateResponse.IsOwner = true;
            updateResponse.Success = true;
            updateResponse.DeckForUpdate = _mapper.Map<DeckForUpdateDTO>(response.Deck);
            return updateResponse;
        }
        public async Task<DeckVM?> GetDeckVMAsync(int id)
        {
            var deck = await GetDeckAsync(id);

            var deckVM = _mapper.Map<DeckVM>(deck);
            return deckVM;
        }
        public async Task<Response<DeckVM>> CreateDeckAsync(int userId, DeckDTOForCreation deckDTOForCreation)
        {
            //Testing out the MappingGenerator
            //Deck deck = deckDTOForCreation.MapToDeck();
            Deck deck = _mapper.Map<Deck>(deckDTOForCreation);
            deck.UserId = userId;
            deck.CreatedAt = DateTime.UtcNow;

            _capstoneDbContext.Decks.Add(deck);
            await _capstoneDbContext.SaveChangesAsync();
            
            return new Response<DeckVM> { Value = _mapper.Map<DeckVM>(deck) };
        }
        public async Task<DeckResponse> UpdateDeck(int userId, int deckId, DeckForUpdateDTO deckForUpdateDTO)
        {
            //TODO Do the IsOwnerCHeck here, not in the GetDeckAsync
            //CONTINUE: implement GetDeckForOwnerAsync in all places
            var response = await GetDeckAsync(deckId);
            if (!response.Success)
            {
                return response;
            }
            Deck deckToUpdate = response.Deck!;
            //TODO: Check that this doesn't wipe away props
            //in the deckToUpdate that aren't present in the deckForUpdateDTO.
            _mapper.Map(deckForUpdateDTO, deckToUpdate);

            deckToUpdate.LastEditedAt = DateTime.UtcNow;

            await _capstoneDbContext.SaveChangesAsync();
            return response;
        }
        public async Task<Response<Deck>> DeleteDeck(int userId, int deckId)
        {
            var response = await GetDeckAsync(deckId);
            if (!response.Success)
            {
                return response;
            }
            
            _capstoneDbContext.Decks.Remove(response.Value);
            await _capstoneDbContext.SaveChangesAsync();
            
            return response;
        }
        private async Task<Response<Deck>> GetDeckAsync(int deckId)
        {
            var response = new Response<Deck>();
            response.Value = await _capstoneDbContext.Decks.FindAsync(deckId);
            if (response.Value == null)
            {
                response.StatusCode = 404;
                return response;
            }
            return response;
        }
        public async Task<bool> DeckExistsAsync(int deckId)
        {
            var deck = await _capstoneDbContext.Decks.FindAsync(deckId);
            if (deck is null)
            {
                return false;
            }
            return true;
        }

        public async Task<Response<Deck>> GetValidEditableDeck(int userId, int deckId)
        {
            Response<Deck> response = new();
            response.Value = await _capstoneDbContext.Decks.FindAsync(deckId);
            if (response.Value is null)
            {
                response.StatusCode = 404;
                return response;
            }
            if (response.Value.Id != userId)
            {
                response.StatusCode = 401;
                return response;
            }
            response.Success = true;
            
            return response;
            //TODO: Make changes in Controller.  Add switch to check statusCodes.
        }

    //DeckCards
        public async Task<List<Card>> GetCardsForDeck(int deckId)
        {

            var deckCardsThroughDeck = await _capstoneDbContext.Decks
                .Include(d => d.DeckCards)//.ThenInclude(dc => dc.Card)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card.ImageUris)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card.ColorIdentity)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card).ThenInclude(c => c.Colors).ThenInclude(cc => cc.ColorsLookUp)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card).ThenInclude(c => c.Colors).ThenInclude(cc => cc.ColorsLookUp)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card).ThenInclude(c => c.Colors).ThenInclude(cc => cc.ColorsLookUp)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card).ThenInclude(c => c.Colors).ThenInclude(cc => cc.ColorsLookUp)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card).ThenInclude(c => c.Colors).ThenInclude(cc => cc.ColorsLookUp)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card).ThenInclude(c => c.Colors).ThenInclude(cc => cc.ColorsLookUp)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card).ThenInclude(c => c.Colors).ThenInclude(cc => cc.ColorsLookUp)
                ////.Include(d => d.DeckCards).ThenInclude()
                .FirstOrDefaultAsync(d =>  d.Id == deckId);

            var cardIds = deckCardsThroughDeck.DeckCards.Select(dc => dc.CardId);
            var cards = _capstoneDbContext.Cards
                .Include(card => card.ImageUris)
                .Include(card => card.ColorIdentity)
                .Where(c => cardIds.Contains(c.Id)).ToList();

            cards.First().ColorIdentity.Select(ci => ci.ColorIdentityLookUp.Value).ToList();
            if (deckCardsThroughDeck is null)
            {
                return new List<Card>();
            }
            var cards = new List<Card>();

            foreach (var deckCard in deckCardsThroughDeck.DeckCards)
            {
                cards.Add(deckCard.Card);
            }
            //ASK: I need help with this.  Before learning about relationships, a Deck would have a List<Card> as a property.
            
            
            var deckCards = await _capstoneDbContext.DeckCards.Include(dc => dc.Card)
                .Where(dc => dc.DeckId == deckId).ToListAsync();
            var cards2 = deckCards.fo



            return cards;
        }
        public async Task<DeckCard?> GetDeckCardByIdAsync(int deckCardId)
        {
            var deckCard = await _capstoneDbContext.DeckCards.FindAsync(deckCardId);

            return deckCard;
        }
        public async Task<DeckCard> AddCardToDeckAsync(int deckId, int cardId)
        {

            var deckCardToAdd = new DeckCard
            {
                CardId = cardId,
                DeckId = deckId
            };

            _capstoneDbContext.DeckCards.Add(deckCardToAdd);
            await _capstoneDbContext.SaveChangesAsync();

            return deckCardToAdd;
        }
        public async Task UpdateDeckCardPrintingAsync(int deckCardId, int cardId)
        {
            var deckCard = await _capstoneDbContext.DeckCards.FindAsync(deckCardId);
            if (deckCard is not null)
                deckCard.CardId = cardId;

            await _capstoneDbContext.SaveChangesAsync();
        }
        public async Task DeleteDeckCardAsync(int deckCardId)
        {
            var deckCard = await _capstoneDbContext.DeckCards.FindAsync(deckCardId);
            if (deckCard is not null)
            {
                _capstoneDbContext.DeckCards.Remove(deckCard);
                await _capstoneDbContext.SaveChangesAsync();
            }
        }

    //DeckCategories
        public async Task<List<DeckCategory>> GetDeckCategoriesForDeck(int deckId)
        {
            var deckCategories = await _capstoneDbContext.DeckCategories.Where(dc => dc.DeckId == deckId).ToListAsync();
            return deckCategories;
        }
        public async Task<DeckCategory?> GetDeckCategoryByIdAsync(int deckCategoryId)
        {
            var deckCategory = await _capstoneDbContext.DeckCategories.FindAsync(deckCategoryId);

            return deckCategory;
        }
        public async Task<DeckCategory> AddCategoryToDeckAsync(int deckId, string name)
        {
            var deckCategoryToAdd = new DeckCategory();

            deckCategoryToAdd.DeckId = deckId;
            deckCategoryToAdd.Name = name;

            _capstoneDbContext.DeckCategories.Add(deckCategoryToAdd);
            await _capstoneDbContext.SaveChangesAsync();

            return deckCategoryToAdd;
        }
        public async Task UpdateDeckCategoryAsync(int deckCategoryId, string name)
        {
            var deckCategory = await _capstoneDbContext.DeckCategories.FindAsync(deckCategoryId);
            if (deckCategory is not null)
                deckCategory.Name = name;

            await _capstoneDbContext.SaveChangesAsync();

        }
        public async Task DeleteDeckCategoryAsync(int deckCategoryId)
        {
            var deckCategory = await _capstoneDbContext.DeckCategories.FindAsync(deckCategoryId);
            if (deckCategory is not null)
            {
                _capstoneDbContext.DeckCategories.Remove(deckCategory);
                await _capstoneDbContext.SaveChangesAsync();
            }
        }

    //Likes
        public async Task<Like?> GetLikeByIdAsync(int likeId)
        {
            var like = await _capstoneDbContext.Likes.FindAsync(likeId);
            return like;
        }
        public async Task<Like> LikeDeckAsync(int deckId, int userId)
        {
            var like = new Like();

            like.DeckId = deckId;
            like.UserId = userId;

            _capstoneDbContext.Likes.Add(like);
            await _capstoneDbContext.SaveChangesAsync();

            return like;
        }
        public async Task UnLikeDeckAsync(int deckId, int userId)
        {
            var like = _capstoneDbContext.Likes.FirstOrDefault(like => like.DeckId == deckId && like.UserId == userId);
            if (like is not null)
            {
                _capstoneDbContext.Likes.Remove(like);
                await _capstoneDbContext.SaveChangesAsync();
            }
        }

        //Comments
        public async Task CommentOnDeckAsync(int deckId, int userId, CommentDTO commentDTO)
        {
            //TODO:Finish this!
        }



        //Card
        public async Task<bool> CardExistsAsync(int id)
        {
            return await _capstoneDbContext.Cards.AnyAsync(d => d.Id == id);
        }

    }
}
