using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Responses;
using MTGCapstone.API.Data.ViewModels;
using System.Security.Claims;

namespace MTGCapstone.API.Services.DomainServiceInterfaces
{
    public interface IDeckService
    {
        //Decks
        Task<Response<List<DeckVM>>> GetDecksAsync(GetDecksRequest getDecksRequest);
        Task<Response<List<DeckVM>>> GetMyDecksAsync(int userId, PersonalDecksRequest decksRequest);
        Task<Response<DeckForUpdateDto>> GetDeckForPatchDTOAsync(int userId, int id);
        Task<DeckVM?> GetDeckVMAsync(int id);
        Task<Response<DeckVM>> CreateDeckAsync(int userId, DeckForCreationDto deckDTOForCreation);
        Task<Response<Deck>> UpdateDeckAsync(int userId, int deckId, DeckForUpdateDto deckForUpdateDTO);
        Task<Response<Deck>> DeleteDeckAsync(int userId, int deckId);
        Task<Response<Deck>> GetValidEditableDeckAsync(int userId, int deckId);
        Task<Response<DeckVM>> GetDeckWithCardsAsync(int? userId, int deckId);
        Task<bool> DeckExistsAsync(int deckId);



    //DeckCards
        //Task<List<Card>> GetCardsForDeck(int deckId);
        Task<DeckCard?> GetDeckCardByIdAsync(int deckCardId);
        Task<Response<CardVMForDeck>> AddCardToDeckAsync(int userId, int deckId, AddCardRequestDto requestDto);
        Task<Response<CardVMForDeck>> UpdateDeckCardPrintingAsync(int userId, int deckId, int deckCardId, int cardId);
        Task<Response> RemoveCardFromDeckAsync(int userId, int deckId, int deckCardId);
        Task<bool> CardExistsAsync(int id);

    //DeckCategories
        Task<List<DeckCategory>> GetDeckCategoriesForDeck(int deckId);
        Task<DeckCategory?> GetDeckCategoryByIdAsync(int deckCategoryId);
        Task<DeckCategory> AddCategoryToDeckAsync(int deckId, string name);
        Task UpdateDeckCategoryAsync(int deckCategoryId, string name);
        Task DeleteDeckCategoryAsync(int deckCategoryId);


    //Likes
        Task<Like?> GetLikeByIdAsync(int likeId);
        Task<Like> LikeDeckAsync(int deckId, int userId);
        Task<Response> UnLikeDeckAsync(int deckId, int userId);

    //Comments
       //Task CommentOnDeckAsync(int deckId, int userId, CommentDTO commentDTO);
    }
} 
