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
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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
            _ = _capstoneDbContext.ColorIndicatorLookUps.ToList();
            _ = _capstoneDbContext.ColorsLookUps.ToList();
            _ = _capstoneDbContext.FinishesLookUps.ToList();
            _ = _capstoneDbContext.ColorIdentityLookUps.ToList();
            _ = _capstoneDbContext.KeywordsLookUps.ToList();
            
        }


    //Decks
        public async Task<Response<List<DeckVM>>> GetDecksAsync(GetDecksRequest getDecksRequest)
        {
            IQueryable<Deck> collection = _capstoneDbContext.Decks;

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
                System.Reflection.PropertyInfo? orderByInfo = deckVM.GetType().GetProperty(getDecksRequest.OrderBy);
                if (orderByInfo is not null)
                {

                    getDecksRequest.OrderBy = orderByInfo.Name;
                }
                else
                {
                    //if the value in the request doesn't exist on the deckVM
                    getDecksRequest.OrderBy = "Views";
                }
            }

            collection.OrderBy(deck => deck.GetType().GetProperty(getDecksRequest.OrderBy));


            List<Deck> collectionToReturn = await collection.Skip(getDecksRequest.PageSize * (getDecksRequest.PageNumber - 1))
                .Take(getDecksRequest.PageSize)
                .ToListAsync();

            Response<List<DeckVM>> response = new Response<List<DeckVM>?>()
            {
                Value = _mapper.Map<List<DeckVM>>(collectionToReturn)
            };
            return response;

        }
        public async Task<Response<List<DeckVM>>> GetMyDecksAsync(int userId, PersonalDecksRequest decksRequest)
        {

            List<Deck> collectionToReturn = await _capstoneDbContext.Decks
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.CreatedAt) //TODO: make this orderby last updated once all deck edits updated last updated property.
                .Skip(decksRequest.PageSize * (decksRequest.PageNumber - 1))
                .Take(decksRequest.PageSize)
                .ToListAsync();

            
            return new Response<List<DeckVM>>() { Success = true, StatusCode = 200, Value = _mapper.Map<List<DeckVM>>(collectionToReturn) };

        }
        public async Task<Response<DeckForUpdateDto>> GetDeckForPatchDTOAsync(int userId, int deckId)
        {
            Response<Deck> response = await GetValidEditableDeckAsync(userId, deckId);

            //TODO: Find a way to map Response<T> with automapper.
            //Is it simply adding mapping profile from Response<Deck> to Response<DeckForupdateDTO>?
            Response<DeckForUpdateDto> updateResponse = new()
            {
                StatusCode = response.StatusCode,
                Message = response.Message,
                Errors = response.Errors,
                Value = _mapper.Map<DeckForUpdateDto>(response.Value),
                Success = response.Success
            };
                                
            if (!response.Success)
            {
                return updateResponse;
            }
            
            return updateResponse;
        }
        public async Task<DeckVM?> GetDeckVMAsync(int id)
        {
            Response<Deck> deck = await GetDeckAsync(id);

            DeckVM deckVM = _mapper.Map<DeckVM>(deck);
            return deckVM;
        }
        public async Task<Response<DeckVM>> CreateDeckAsync(int userId, DeckForCreationDto deckDTOForCreation)
        {
            Deck deck = _mapper.Map<Deck>(deckDTOForCreation);
            deck.UserId = userId;
            deck.CreatedAt = DateTime.UtcNow;

            _capstoneDbContext.Decks.Add(deck);
            await _capstoneDbContext.SaveChangesAsync();
            
            return new Response<DeckVM> { StatusCode = 201, Success = true, Value = _mapper.Map<DeckVM>(deck) };
        }
        public async Task<Response<Deck>> UpdateDeckAsync(int userId, int deckId, DeckForUpdateDto deckForUpdateDTO)
        {
            var response = await GetValidEditableDeckAsync(userId, deckId);
            if (!response.Success || response.Value is null)
            {
                return response;
            }
            
            _mapper.Map(deckForUpdateDTO, response.Value);

            response.Value.LastEditedAt = DateTime.UtcNow;

            //TODO: Make sure that the response.Value (deck) is still tracked, and therefore saved/updated.
            await _capstoneDbContext.SaveChangesAsync();
            return response;
        }
        public async Task<Response<Deck>> DeleteDeckAsync(int userId, int deckId)
        {
            Response<Deck> response = await GetValidEditableDeckAsync(userId, deckId);
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
            Response<Deck> response = new Response<Deck>();
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
            Deck? deck = await _capstoneDbContext.Decks.FindAsync(deckId);
            if (deck is null)
            {
                return false;
            }
            return true;
        }
        public async Task<Response<DeckVM>> GetDeckWithCardsAsync(int deckId)
        {
            //get deck with deckcards
            Deck? deck = await _capstoneDbContext.Decks
                .Include(d => d.DeckCards).ThenInclude(dc => dc.DeckCategories)
                //TODO: figure out why .theninclude deckcategories is a list
                .Include(d => d.DeckCategories)
                //.ThenInclude(dc => dc.Card)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card.ImageUris)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card.ColorIdentity)
                //.Include(d => d.DeckCards).ThenInclude(dc => dc.Card).ThenInclude(c => c.Colors).ThenInclude(cc => cc.ColorsLookUp)
                ////.Include(d => d.DeckCards).ThenInclude()
                .FirstOrDefaultAsync(d =>  d.Id == deckId);

            if (deck is null)
            {
                return new Response<DeckVM>()  { Errors = { "Deck not found." }, StatusCode = 404 };
            }

            //get cards with related data
            IEnumerable<int?> cardIds = deck.DeckCards.Select(dc => dc.CardId);
            List<Card> cards = _capstoneDbContext.Cards
                .Include(card => card.ImageUris)
                .Include(card => card.Colors)//.ThenInclude(c => c.ColorsLookUp)
                //.Include(card => card.ColorIndicator)//.ThenInclude(c => c.ColorIndicatorLookUp)
                .Include(card => card.ColorIdentity)//.ThenInclude(c => c.ColorIdentityLookUp)
                .Include(card => card.Keywords)//.ThenInclude(c => c.KeywordsLookUp)
                .Include(card => card.Legalities)                               
                .Include(card => card.Prices)                                   
                .Include(card => card.RelatedUris)                              
                .Include(card => card.PurchaseUris)                             
                .Include(card => card.CardFaces).ThenInclude(cf => cf.ImageUris)
                .Include(card => card.CardFaces).ThenInclude(cf => cf.Colors)//.ThenInclude(c => c.ColorsLookUp)
                //.Include(card => card.CardFaces).ThenInclude(cf => cf.ColorIndicator)//.ThenInclude(c => c.ColorIndicatorLookUp)
                .Where(c => cardIds.Contains(c.Id)).ToList();

            //map the list of cards to list of CardVMForDeck
            List<CardVMForDeck> cardVMs = new();
            //region is mapping
            #region 
            foreach (Card card in cards)
            {
                CardVMForDeck cardVM = _mapper.Map<CardVMForDeck>(card);
                DeckCard deckCard = deck.DeckCards.FirstOrDefault(dc => dc.CardId == card.Id)!;
                cardVM.DeckCardId = deckCard.Id;
                cardVM.Quantity = deckCard.Quantity;
                cardVM.Board = deckCard.Board;

                foreach (DeckCategoryDeckCard category in deckCard.DeckCategories)
                {
                    cardVM.Categories.Add(category.DeckCategory.Name);
                }
                //Continue: working on getting the quantity for the card VM from the list of deckcategorydeckcards.
                //Consider changing deckcards to have quantity and boards on it.

                foreach (CardColorsLookUp color in card.Colors)
                {
                    if (true)
                    {
                        //TODO: Add null checks or decide to add the null exclamation 
                    }
                    cardVM.Colors.Add(color!.ColorsLookUp!.Value!);
                }
                foreach (CardColorIdentityLookUp color in card.ColorIdentity)
                {
                    cardVM.ColorIdentity.Add(color.ColorIdentityLookUp.Value);
                }
                foreach (CardKeywordsLookUp color in card.Keywords)
                {
                    cardVM.Keywords.Add(color.KeywordsLookUp.Value);
                }
                if (card.CardFaces.Any())
                {
                    foreach (CardFace face in card.CardFaces)
                    {
                        CardFaceVM cardFaceVM = _mapper.Map<CardFaceVM>(face);
                        foreach (CardColorsLookUp color in face.Colors)
                        {
                            cardFaceVM.Colors.Add(color.ColorsLookUp.Value);
                        }
                        cardVM.CardFaces.Add(cardFaceVM);
                    }
                }
                cardVMs.Add(cardVM);
            }
            #endregion

            DeckVM deckVM = _mapper.Map<DeckVM>(deck);
            deckVM.Cards = cardVMs;

            return new Response<DeckVM>() { Success = true, StatusCode = 200, Value = deckVM };
        }

        public async Task<Response<Deck>> GetValidEditableDeckAsync(int userId, int deckId)
        {
            Response<Deck> response = new();
            response.Value = await _capstoneDbContext.Decks.FindAsync(deckId);
            if (response.Value is null)
            {
                response.StatusCode = 404;
                return response;
            }
            if (response.Value.UserId != userId)
            {
                response.StatusCode = 403;
                return response;
            }
            response.Success = true;
            
            return response;
        }

    //DeckCards
        public async Task<DeckCard?> GetDeckCardByIdAsync(int deckCardId)
        {
            DeckCard? deckCard = await _capstoneDbContext.DeckCards.FindAsync(deckCardId);

            return deckCard;
        }
        public async Task<DeckCard> AddCardToDeckAsync(int deckId, int cardId)
        {

            DeckCard deckCardToAdd = new DeckCard
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
            DeckCard? deckCard = await _capstoneDbContext.DeckCards.FindAsync(deckCardId);
            if (deckCard is not null)
                deckCard.CardId = cardId;

            await _capstoneDbContext.SaveChangesAsync();
        }
        public async Task DeleteDeckCardAsync(int deckCardId)
        {
            DeckCard? deckCard = await _capstoneDbContext.DeckCards.FindAsync(deckCardId);
            if (deckCard is not null)
            {
                _capstoneDbContext.DeckCards.Remove(deckCard);
                await _capstoneDbContext.SaveChangesAsync();
            }
        }

    //DeckCategories
        public async Task<List<DeckCategory>> GetDeckCategoriesForDeck(int deckId)
        {
            List<DeckCategory> deckCategories = await _capstoneDbContext.DeckCategories.Where(dc => dc.DeckId == deckId).ToListAsync();
            return deckCategories;
        }
        public async Task<DeckCategory?> GetDeckCategoryByIdAsync(int deckCategoryId)
        {
            DeckCategory? deckCategory = await _capstoneDbContext.DeckCategories.FindAsync(deckCategoryId);

            return deckCategory;
        }
        public async Task<DeckCategory> AddCategoryToDeckAsync(int deckId, string name)
        {
            DeckCategory deckCategoryToAdd = new DeckCategory();

            deckCategoryToAdd.DeckId = deckId;
            deckCategoryToAdd.Name = name;

            _capstoneDbContext.DeckCategories.Add(deckCategoryToAdd);
            await _capstoneDbContext.SaveChangesAsync();

            return deckCategoryToAdd;
        }
        public async Task UpdateDeckCategoryAsync(int deckCategoryId, string name)
        {
            DeckCategory? deckCategory = await _capstoneDbContext.DeckCategories.FindAsync(deckCategoryId);
            if (deckCategory is not null)
                deckCategory.Name = name;

            await _capstoneDbContext.SaveChangesAsync();

        }
        public async Task DeleteDeckCategoryAsync(int deckCategoryId)
        {
            DeckCategory? deckCategory = await _capstoneDbContext.DeckCategories.FindAsync(deckCategoryId);
            if (deckCategory is not null)
            {
                _capstoneDbContext.DeckCategories.Remove(deckCategory);
                await _capstoneDbContext.SaveChangesAsync();
            }
        }

    //Likes
        public async Task<Like?> GetLikeByIdAsync(int likeId)
        {
            Like? like = await _capstoneDbContext.Likes.FindAsync(likeId);
            return like;
        }
        public async Task<Like> LikeDeckAsync(int deckId, int userId)
        {
            Like like = new Like();

            like.DeckId = deckId;
            like.UserId = userId;

            _capstoneDbContext.Likes.Add(like);
            await _capstoneDbContext.SaveChangesAsync();

            return like;
        }
        public async Task UnLikeDeckAsync(int deckId, int userId)
        {
            Like? like = _capstoneDbContext.Likes.FirstOrDefault(like => like.DeckId == deckId && like.UserId == userId);
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
