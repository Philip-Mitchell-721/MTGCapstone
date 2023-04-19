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
        Task<List<DeckVM>?> GetDecksAsync(GetDecksRequest getDecksRequest);
        Task<DeckForUpdateResponse> GetDeckForUpdateDTOAsync(int userId, int id);
        Task<DeckVM?> GetDeckVMAsync(int id);
        Task<Response<DeckVM>> CreateDeckAsync(int userId, DeckDTOForCreation deckDTOForCreation);
        Task<DeckResponse> UpdateDeck(int userId, int deckId, DeckForUpdateDTO deckForUpdateDTO);
        Task<DeckResponse> DeleteDeck(int userId, int deckId);
        Task<DeckResponse> IsOwnerAsync(int userId, int deckId);
        Task<bool> DeckExistsAsync(int deckId);



    //DeckCards
        Task<List<Card>> GetCardsForDeck(int deckId);
        Task<DeckCard?> GetDeckCardByIdAsync(int deckCardId);
        Task<DeckCard> AddCardToDeckAsync(int deckId, int cardId);
        Task UpdateDeckCardPrintingAsync(int deckCardId, int cardId);
        Task DeleteDeckCardAsync(int deckCardId);
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
        Task UnLikeDeckAsync(int deckId, int userId);

    //Comments
       Task CommentOnDeckAsync(int deckId, int userId, CommentDTO commentDTO);
    }
} 
