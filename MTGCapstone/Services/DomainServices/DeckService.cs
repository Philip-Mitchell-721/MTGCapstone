using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.ViewModels;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Services.DomainServiceInterfaces;
using System.Reflection;

namespace MTGCapstone.API.Services.DomainServices
{
    public class DeckService : IDeckService
    {
        private readonly CapstoneDbContext _capstoneDbContext;
        private readonly IMapper _mapper;

        public DeckService(CapstoneDbContext capstoneDbContext,
            IMapper mapper)
        {
            _capstoneDbContext = capstoneDbContext
                ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
        }


    //Decks
        public async Task<List<DeckVM>?> GetDecksAsync(DeckSearchFilterParameters deckSearchFilterParameters)
        {
            IQueryable<Deck> collection = _capstoneDbContext.Decks;

            if (deckSearchFilterParameters.UserId is not null)
            {
                collection = collection.Where(deck => deck.UserId == deckSearchFilterParameters.UserId);
            }

            if (deckSearchFilterParameters.Format is not null)
            {
                collection = collection.Where(deck => deck.Format == deckSearchFilterParameters.Format);
            }

            if (deckSearchFilterParameters.Commander is not null)
            {

                collection = collection.Where(deck => deck.DeckCategories
                                                        .Any(dc => dc.DeckCategoryDeckCards
                                                                    .Any(dcdc => dcdc.DeckCard != null
                                                                                && dcdc.DeckCard.Card != null
                                                                                && dcdc.DeckCard.Card.Name != null
                                                                                && dcdc.DeckCard.Card.Name.ToLower() == deckSearchFilterParameters.Commander.ToLower())));

            }

            deckSearchFilterParameters.OrderBy ??= "Views";
            if (deckSearchFilterParameters.OrderBy != "Views")
            {
                DeckVM deckVM = new();
                var orderByInfo = deckVM.GetType().GetProperty(deckSearchFilterParameters.OrderBy);
                if (orderByInfo is not null)
                {

                    deckSearchFilterParameters.OrderBy = orderByInfo.Name;
                }
                else
                {
                    deckSearchFilterParameters.OrderBy = "Views";
                }
            }

            collection = collection.OrderBy(deck => deck.GetType().GetProperty(deckSearchFilterParameters.OrderBy));


            var collectionToReturn = await collection.Skip(deckSearchFilterParameters.PageSize * (deckSearchFilterParameters.PageNumber - 1))
                .Take(deckSearchFilterParameters.PageSize)
                .ToListAsync();

            return _mapper.Map<List<DeckVM>>(collectionToReturn);

        }
        private async Task<Deck?> GetDeckAsync(int id)
        {
            var deck = await _capstoneDbContext.Decks.FindAsync(id);

            return deck;
        }
        public async Task<DeckForUpdateDTO?> GetDeckForUpdateDTOAsync(int id)
        {
            var deck = await GetDeckAsync(id);
            var deckForUpdateDTO = _mapper.Map<DeckForUpdateDTO>(deck);
            return deckForUpdateDTO;
        }
        public async Task<DeckVM?> GetDeckVMAsync(int id)
        {
            var deck = await GetDeckAsync(id);
            var deckVM = _mapper.Map<DeckVM>(deck);
            return deckVM;
        }
        public async Task<DeckVM> CreateDeckAsync(DeckDTOForCreation deckDTOForCreation)
        {
            Deck deck = _mapper.Map<Deck>(deckDTOForCreation);
            deck.CreatedAt = DateTime.UtcNow;

            _capstoneDbContext.Decks.Add(deck);

            await _capstoneDbContext.SaveChangesAsync();

            return _mapper.Map<DeckVM>(deck);
        }
        public async Task UpdateDeck(int deckId, DeckForUpdateDTO deckForUpdateDTO)
        {
            var deckToUpdate = await _capstoneDbContext.Decks.FindAsync(deckId);
            if (deckToUpdate is not null)
            {
                _mapper.Map(deckForUpdateDTO, deckToUpdate);

                deckToUpdate.LastEditedAt = DateTime.UtcNow;

                await _capstoneDbContext.SaveChangesAsync();

            }

        }
        public async Task DeleteDeck(int deckId)
        {
            var deck = await _capstoneDbContext.Decks.FindAsync(deckId);
            if (deck is not null)
            {
                _capstoneDbContext.Decks.Remove(deck);
                await _capstoneDbContext.SaveChangesAsync();
            }
        }
        public async Task<bool> DeckExistsAsync(int id)
        {
            return await _capstoneDbContext.Decks.AnyAsync(d => d.Id == id);
        }

    //DeckCards
        public async Task<List<DeckCard>> GetDeckCardsForDeck(int deckId)
        {
            var deckCards = await _capstoneDbContext.DeckCards.Where(dc => dc.DeckId == deckId).ToListAsync();
            return deckCards;
        }
        public async Task<DeckCard?> GetDeckCardByIdAsync(int deckCardId)
        {
            var deckCard = await _capstoneDbContext.DeckCards.FindAsync(deckCardId);

            return deckCard;
        }
        public async Task<DeckCard> AddCardToDeckAsync(int deckId, int cardId)
        {

            var deckCardToAdd = new DeckCard();

            //ASK: (disclaimer: tired thoughts.  Do I need to check if 0 here, since I already did that check on the Controller?
            if (cardId is not 0 && deckId is not 0)
            {
                deckCardToAdd.CardId = cardId;
                deckCardToAdd.DeckId = deckId;


                _capstoneDbContext.DeckCards.Add(deckCardToAdd);
                await _capstoneDbContext.SaveChangesAsync();
            }

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
        public async Task<Like> LikeDeckAsync(int deckId, string userId)
        {
            var like = new Like();

            like.DeckId = deckId;
            like.UserId = userId;

            _capstoneDbContext.Likes.Add(like);
            await _capstoneDbContext.SaveChangesAsync();

            return like;
        }
        public async Task UnLikeDeckAsync(int deckId, string userId)
        {
            var like = _capstoneDbContext.Likes.FirstOrDefault(like => like.DeckId == deckId && like.UserId == userId);
            if (like is not null)
            {
                _capstoneDbContext.Likes.Remove(like);
                await _capstoneDbContext.SaveChangesAsync();
            }
        }

        //Comments
        public async Task CommentOnDeckAsync(int deckId, string userId, CommentDTO commentDTO)
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
