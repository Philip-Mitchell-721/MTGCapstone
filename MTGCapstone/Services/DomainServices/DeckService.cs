using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Responses;
using MTGCapstone.API.Data.ViewModels;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Extentions.LoggerMessages;
using MTGCapstone.API.Services.DomainServiceInterfaces;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace MTGCapstone.API.Services.DomainServices
{
    public class DeckService : IDeckService
    {
        private readonly CapstoneDbContext _capstoneDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<DeckService> _logger;

        private static List<string> GetFormats()
        {
            return new List<string>() { "standard", "future", "historic", "gladiator", "pioneer", "explorer", "modern", 
            "legacy", "pauper", "vintage", "penny", "commander", "brawl", "historicbrawl", "alchemy", "paupercommander",
            "duel", "oldschool", "premodern"};
        }

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

            if (!string.IsNullOrWhiteSpace(getDecksRequest.UserName))
            {
                collection = collection.Where(deck => deck.User != null 
                                                   && deck.User.UserName.ToLower() == getDecksRequest.UserName.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(getDecksRequest.Format))
            {
                collection = collection.Where(deck => deck.Format == getDecksRequest.Format);
            }

            if (!string.IsNullOrWhiteSpace(getDecksRequest.Commander))
            {
                
                collection = collection.Where(deck => deck.DeckCategories
                                                        .Any(dc => dc.DeckCategoryDeckCards
                                                                    .Any(dcdc => dcdc.DeckCard != null
                                                                                && dcdc.DeckCard.Card != null
                                                                                && dcdc.DeckCard.Card.Name != null
                                                                                && dcdc.DeckCard.Card.Name.ToLower() == getDecksRequest.Commander.ToLower())));

            }
            if (getDecksRequest.OrderBy != "Views")
            {
                DeckVM deckVM = new();
                PropertyInfo? orderByInfo = deckVM.GetType().GetProperty(getDecksRequest.OrderBy);
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

            Response<List<DeckVM>> response = new()
            {
                Value = _mapper.Map<List<DeckVM>>(collectionToReturn)
            };
            return response;

        }
        public async Task<Response<List<DeckVM>>> GetMyDecksAsync(int userId, PersonalDecksRequest decksRequest)
        {
            IQueryable<Deck> collection = _capstoneDbContext.Decks.Where(d => d.UserId == userId);
            if (!string.IsNullOrWhiteSpace(decksRequest.Search))
            {
                decksRequest.Search = decksRequest.Search.Trim();

                collection = collection.Where(d => (!string.IsNullOrWhiteSpace(d.Name) && d.Name.Contains(decksRequest.Search))
                    || d.DeckCards.Any(dc => dc.Card!.Name!.Contains(decksRequest.Search)));
            }
            if (!string.IsNullOrWhiteSpace(decksRequest.Format))
            {
                decksRequest.Format = decksRequest.Format.Trim();
                collection = collection.Where(d => !string.IsNullOrWhiteSpace(d.Format) && d.Format.Contains(decksRequest.Format));
            }
            //TODO: Add OrderBy options here.
            List<Deck> collectionToReturn = await collection
                .OrderBy(d => d.CreatedAt) //TODO: make this orderby last updated once all deck edits updated last updated property.
                .Skip(20 * (decksRequest.PageNumber - 1))
                .Take(20)
                .ToListAsync();

            
            return new Response<List<DeckVM>>() { Success = true, StatusCode = ResponseStatusCodes.Ok, Value = _mapper.Map<List<DeckVM>>(collectionToReturn) };

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
            List<string> _formats = GetFormats();
            if (!_formats.Contains(deckDTOForCreation.Format!.ToLower().Trim()))
            {
                return new Response<DeckVM> { StatusCode = ResponseStatusCodes.BadRequest, Errors = { "Format doesn't exist." } };
            }
            Deck deck = _mapper.Map<Deck>(deckDTOForCreation);
            deck.UserId = userId;
            deck.CreatedAt = DateTime.UtcNow;

            _capstoneDbContext.Decks.Add(deck);
            await _capstoneDbContext.SaveChangesAsync();
            
            return new Response<DeckVM> { StatusCode = ResponseStatusCodes.Created, Success = true, Value = _mapper.Map<DeckVM>(deck) };
        }
        public async Task<Response<Deck>> UpdateDeckAsync(int userId, int deckId, DeckForUpdateDto deckForUpdateDTO)
        {
            Response<Deck> response = await GetValidEditableDeckAsync(userId, deckId);
            if (!response.Success)
            {
                return response;
            }
            
            _mapper.Map(deckForUpdateDTO, response.Value);

            response.Value!.LastEditedAt = DateTime.UtcNow;

            await _capstoneDbContext.SaveChangesAsync();

            response.StatusCode = ResponseStatusCodes.NoContent;
            return response;
        }
        public async Task<Response<Deck>> DeleteDeckAsync(int userId, int deckId)
        {
            Response<Deck> response = await GetValidEditableDeckAsync(userId, deckId);
            if (!response.Success)
            {
                return response;
            }
            
            _capstoneDbContext.Decks.Remove(response.Value!);
            await _capstoneDbContext.SaveChangesAsync();
            response.StatusCode = ResponseStatusCodes.NoContent;
            
            return response;
        }
        private async Task<Response<Deck>> GetDeckAsync(int deckId)
        {
            Response<Deck> response = new()
            {
                Value = await _capstoneDbContext.Decks.FindAsync(deckId)
            };
            if (response.Value == null)
            {
                response.StatusCode = (ResponseStatusCodes)404;
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
                .Include(d => d.DeckCards).ThenInclude(dc => dc.DeckCategories).ThenInclude(dcdc => dcdc.DeckCategory)
                .Include(d => d.DeckCategories)
                .FirstOrDefaultAsync(d =>  d.Id == deckId);

            if (deck is null || deck.IsPrivate)
            {
                return new Response<DeckVM>() { Errors = { "Deck not found." }, StatusCode = ResponseStatusCodes.NotFound };
            }
            

            //get cards with related data
            IEnumerable<int?> cardIds = deck.DeckCards.Select(dc => dc.CardId);
            List<Card> cards = _capstoneDbContext.Cards
                .Include(card => card.ImageUris)
                .Include(card => card.Colors)
                .Include(card => card.ColorIdentity)
                .Include(card => card.Keywords)
                .Include(card => card.Legalities)                               
                .Include(card => card.Prices)                                   
                .Include(card => card.RelatedUris)                              
                .Include(card => card.PurchaseUris)                             
                .Include(card => card.CardFaces).ThenInclude(cf => cf.ImageUris)
                .Include(card => card.CardFaces).ThenInclude(cf => cf.Colors)
                .Where(c => cardIds.Contains(c.Id)).ToList();

            //map the list of cards to list of CardVMForDeck
            List<CardVMForDeck> cardVMs = new();
            foreach (Card card in cards)
            {
                DeckCard deckCard = deck.DeckCards.FirstOrDefault(dc => dc.CardId == card.Id)!;
                CardVMForDeck cardVM = CreateCardVMForDeck(card, deckCard);
                cardVMs.Add(cardVM);
            }
            

            DeckVM deckVM = _mapper.Map<DeckVM>(deck);
            deckVM.Cards = cardVMs;

            return new Response<DeckVM>() { Success = true, StatusCode = ResponseStatusCodes.Ok, Value = deckVM };
        }

        private CardVMForDeck CreateCardVMForDeck(Card card, DeckCard deckCard)
        {
            CardVMForDeck cardVM = _mapper.Map<CardVMForDeck>(card);
            cardVM.DeckCardId = deckCard.Id;
            cardVM.Quantity = deckCard.Quantity;
            cardVM.Board = deckCard.Board;

            foreach (DeckCategoryDeckCard category in deckCard.DeckCategories)
            {
                if (category.DeckCategory?.Name is not null)
                {
                    cardVM.Categories.Add(category.DeckCategory.Name);
                }
            }
            foreach (CardColorsLookUp color in card.Colors)
            {
                if (color.ColorsLookUp?.Value is not null)
                {
                    cardVM.Colors.Add(color.ColorsLookUp.Value);
                }
            }
            foreach (CardColorIdentityLookUp color in card.ColorIdentity)
            {
                if (color.ColorIdentityLookUp?.Value is not null)
                {
                    cardVM.ColorIdentity.Add(color.ColorIdentityLookUp.Value);
                }
            }
            foreach (CardKeywordsLookUp color in card.Keywords)
            {
                if (color.KeywordsLookUp?.Value is not null)
                {
                    cardVM.Keywords.Add(color.KeywordsLookUp.Value);
                }
            }
            if (card.CardFaces.Any())
            {
                foreach (CardFace face in card.CardFaces)
                {
                    CardFaceVM cardFaceVM = _mapper.Map<CardFaceVM>(face);
                    foreach (CardColorsLookUp color in face.Colors)
                    {
                        if (color.ColorsLookUp?.Value is not null)
                        {
                            cardFaceVM.Colors.Add(color.ColorsLookUp.Value);
                        }
                    }
                    cardVM.CardFaces.Add(cardFaceVM);
                }
            }

            return cardVM;
        }

        public async Task<Response<Deck>> GetValidEditableDeckAsync(int userId, int deckId)
        {
            Response<Deck> response = new();
            Deck? deck = await _capstoneDbContext.Decks.FindAsync(deckId);
            if (deck is null)
            {
                response.Errors.Add("Deck not found.");
                response.StatusCode = ResponseStatusCodes.NotFound;
                return response;
            }
            if (deck.UserId != userId)
            {
                _logger.LogInformation("User with id:{userId} trying to edit deck with id:{deckId}", userId, deckId);
                response.Errors.Add("Denied permission to edit this deck.");
                response.StatusCode = ResponseStatusCodes.Forbidden;
                return response;
            }
            response.Value = deck;
            response.Success = true;
            
            return response;
        }
        public async Task<Response<Deck>> GetValidEditableDeckWithDeckCardsAsync(int userId, int deckId)
        {
            Response<Deck> response = new();
            Deck? deck = await _capstoneDbContext.Decks
                .Include(d => d.DeckCards).ThenInclude(dc => dc.DeckCategories).ThenInclude(dcdc => dcdc.DeckCategory)
                .FirstOrDefaultAsync(d => d.Id == deckId);
            if (deck is null)
            {
                response.Errors.Add("Deck not found.");
                response.StatusCode = ResponseStatusCodes.NotFound;
                return response;
            }
            if (deck.UserId != userId)
            {
                _logger.LogInformation("User with id:{userId} trying to edit deck with id:{deckId}", userId, deckId);
                response.Errors.Add("Denied permission to edit this deck.");
                response.StatusCode = ResponseStatusCodes.Forbidden;
                return response;
            }
            response.Value = deck;
            response.Success = true;

            return response;
        }

        //DeckCards
        public async Task<DeckCard?> GetDeckCardByIdAsync(int deckCardId)
        {
            DeckCard? deckCard = await _capstoneDbContext.DeckCards.FindAsync(deckCardId);

            return deckCard;
        }
        public async Task<Response<CardVMForDeck>> AddCardToDeckAsync(int userId, int deckId, AddCardRequestDto requestDto)
        {
            if (!TryValidateObjectRequest(requestDto, out List<ValidationResult>? results))
            {
                return new Response<CardVMForDeck> { Errors = results?.Select(r => r.ErrorMessage ?? string.Empty).ToList() ?? new List<string>(), StatusCode = ResponseStatusCodes.BadRequest };
            }
            Response<Deck> deckResponse = await GetValidEditableDeckWithDeckCardsAsync(userId, deckId);
            Response<CardVMForDeck> cardResponse = new()
            {
                StatusCode = deckResponse.StatusCode,
                Message = deckResponse.Message,
                Errors = deckResponse.Errors,
                Success = deckResponse.Success
            };
            if (!cardResponse.Success)
            {
                return cardResponse;
            }
            DeckCard? deckCard = deckResponse.Value?.DeckCards.FirstOrDefault(dc => dc.CardId == requestDto.CardId);
            if (deckCard is not null)
            {
                deckCard.Quantity++;
                cardResponse.Success = true;
                cardResponse.StatusCode = ResponseStatusCodes.NoContent;
                await _capstoneDbContext.SaveChangesAsync();
                //TODO: Test that this save changes is still tracking the deckcard.
                return cardResponse;
            }
            Card? card = await GetCardWithRelatedTablesAsync(requestDto.CardId!.Value);

            if (card is null)
            {
                cardResponse.StatusCode = ResponseStatusCodes.NotFound;
                cardResponse.Errors.Add("Card not found.");
                cardResponse.Success = false;
                return cardResponse;
            }

            deckCard = new DeckCard
            {
                CardId = card.Id,
                DeckId = deckId,
                Quantity = 1,
                Board = DeckBoards.Main //TODO: change this to be configurable 
            };
            _capstoneDbContext.DeckCards.Add(deckCard);
            await _capstoneDbContext.SaveChangesAsync();

            CardVMForDeck cardVM = CreateCardVMForDeck(card, deckCard);
            cardResponse.Value = cardVM;

            cardResponse.Success = true;
            cardResponse.StatusCode = ResponseStatusCodes.Ok;
            return cardResponse;
        }

        private async Task<Card?> GetCardWithRelatedTablesAsync(int cardId)
        {
            return await _capstoneDbContext.Cards
                .Include(card => card.ImageUris)
                .Include(card => card.Colors)
                .Include(card => card.ColorIdentity)
                .Include(card => card.Keywords)
                .Include(card => card.Legalities)
                .Include(card => card.Prices)
                .Include(card => card.RelatedUris)
                .Include(card => card.PurchaseUris)
                .Include(card => card.CardFaces).ThenInclude(cf => cf.ImageUris)
                .Include(card => card.CardFaces).ThenInclude(cf => cf.Colors)
                .FirstOrDefaultAsync(c => cardId == c.Id);
        }

        public async Task<Response<CardVMForDeck>> UpdateDeckCardPrintingAsync(int userId, int deckId, int deckCardId, int cardId)
        {
            Response<Deck> deckResponse = await GetValidEditableDeckWithDeckCardsAsync(userId, deckId);

            Response<CardVMForDeck> cardResponse = new()
            {
                StatusCode = deckResponse.StatusCode,
                Message = deckResponse.Message,
                Errors = deckResponse.Errors,
                Success = deckResponse.Success
            };
            if (!cardResponse.Success)
            {
                return cardResponse;
            }

            DeckCard? deckCard = deckResponse.Value?.DeckCards.FirstOrDefault(dc => dc.Id == deckCardId);
            if (deckCard is null)
            {
                cardResponse.StatusCode = ResponseStatusCodes.NotFound;
                cardResponse.Errors.Add("Card not found in deck.");
                cardResponse.Success = false;
                return cardResponse;
            }
            
            Card? card = await GetCardWithRelatedTablesAsync(cardId);

            if (card is null)
            {
                cardResponse.StatusCode = ResponseStatusCodes.NotFound;
                cardResponse.Errors.Add("Card not found.");
                cardResponse.Success = false;
                return cardResponse;
            }

            deckCard.CardId = card.Id;
            await _capstoneDbContext.SaveChangesAsync();

            CardVMForDeck cardVM = CreateCardVMForDeck(card, deckCard);
            cardResponse.Value = cardVM;

            cardResponse.StatusCode = ResponseStatusCodes.Ok;
            return cardResponse;
        }
        public async Task<Response> RemoveCardFromDeckAsync(int userId, int deckId, int deckCardId)
        {
            Response<Deck> deckResponse = await GetValidEditableDeckWithDeckCardsAsync(userId, deckId);
            Response response = new()
            {
                StatusCode = deckResponse.StatusCode,
                Message = deckResponse.Message,
                Errors = deckResponse.Errors,
                Success = deckResponse.Success
            };

            if (!response.Success)
            {
                return response;
            }

            DeckCard? deckCard = deckResponse.Value?.DeckCards.FirstOrDefault(dc => dc.Id == deckCardId);
            if (deckCard is not null)
            {
                deckCard.Quantity--;
                if (deckCard.Quantity < 1)
                {
                    _capstoneDbContext.DeckCards.Remove(deckCard);
                }
                await _capstoneDbContext.SaveChangesAsync();
            }
            response.StatusCode = ResponseStatusCodes.NoContent;
            return response;
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
            DeckCategory deckCategoryToAdd = new()
            {
                DeckId = deckId,
                Name = name
            };

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
            Like like = new() 
            {
                DeckId = deckId,
                UserId = userId
            };

            _capstoneDbContext.Likes.Add(like);
            await _capstoneDbContext.SaveChangesAsync();

            return like;
        }
        public async Task<Response> UnLikeDeckAsync(int deckId, int userId)
        {
            Like? like = _capstoneDbContext.Likes.FirstOrDefault(like => like.DeckId == deckId && like.UserId == userId);
            if (like is not null)
            {
                _capstoneDbContext.Likes.Remove(like);
                await _capstoneDbContext.SaveChangesAsync();
                return new Response { Success = true, StatusCode = ResponseStatusCodes.NoContent };
            }
            return new Response { StatusCode = ResponseStatusCodes.NotFound };
        }

        //Comments
        //public async Task CommentOnDeckAsync(int deckId, int userId, CommentDTO commentDTO)
        //{
        //    //TODO:Finish this!
        //}



        //Card
        public async Task<bool> CardExistsAsync(int id)
        {
            return await _capstoneDbContext.Cards.AnyAsync(d => d.Id == id);
        }

        private static bool TryValidateObjectRequest(object requestObject, out List<ValidationResult> results)
        {
            results = new();
            ValidationContext context = new ValidationContext(requestObject, null, null);
            return Validator.TryValidateObject(requestObject, context, results);
        }
    }
}
