using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Responses;
using MTGCapstone.API.Data.ViewModels;
using MTGCapstone.API.DbContexts;
using System.Reflection;

namespace MTGCapstone.API.Services
{
    public class CardService : ICardService
    {
        private readonly CapstoneDbContext _context;
        private readonly ILogger<CardService> _logger;
        private readonly IMapper _mapper;

        public CardService(CapstoneDbContext context,
            ILogger<CardService> logger,
            IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<Card?> GetCardAsync(int id)
        {
            return await _context.Cards.FindAsync(id);
        }

        public async Task<Response<List<CardVMForDeck>>> GetCardsAsync(CardResourceParameters cardResourceParameters)
        {
            IQueryable<Card> collection = _context.Cards;


            if (!string.IsNullOrWhiteSpace(cardResourceParameters.Name))
            {
                cardResourceParameters.Name = cardResourceParameters.Name.Trim();
                collection = collection.Where(c => c.Name != null && c.Name.ToLower().Contains(cardResourceParameters.Name.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(cardResourceParameters.Search))
            {
                cardResourceParameters.Search = cardResourceParameters.Search.Trim();

                collection = collection.Where(c => (c.Name != null && c.Name.Contains(cardResourceParameters.Search))
                    || (c.OracleText != null && c.OracleText.Contains(cardResourceParameters.Search)));
            }


            if (cardResourceParameters.OrderBy != "EdhrecRank")
            {
                CardVMForDeck card = new();
                PropertyInfo? orderByInfo = card.GetType().GetProperty(cardResourceParameters.OrderBy);
                if (orderByInfo is not null)
                {

                    cardResourceParameters.OrderBy = orderByInfo.Name;
                }
                else
                {
                    cardResourceParameters.OrderBy = "EdhrecRank";
                }
            }

            collection = collection.Where(c => c.Language == cardResourceParameters.Language
                    && c.EdhrecRank != 0);

            //ASK:TODO: No idea why the F this won't work...
            IQueryable<Card> collectionToReturn = collection
                .GroupBy(c => c.OracleId)
                .Select(c => c.First());
                //.OrderBy(c => c.EdhrecRank);


            //var collectionToReturn = collection
            //    .GroupBy(c => c.OracleId)
            //    .OrderBy(group => group.Min(card => card.EdhrecRank))
            //    .Select(c => c.FirstOrDefault());

            //SELECT x.* FROM Cards as x
            //INNER JOIN(Select Id, OracleId, Min(EdhrecRank) as EdhrecRank1 
            //from Cards as y 
            //where y.language = 'en' and y.EdhrecRank <> 0 
            //GROUP BY y.OracleId 
            //ORDER BY EdhrecRank1 ASC) as j on x.Id = j.id


            List<Card> collection2 = await collection.Skip(cardResourceParameters.PageSize * (cardResourceParameters.PageNumber - 1))
                .Take(cardResourceParameters.PageSize).ToListAsync();

            return new Response<List<CardVMForDeck>>() { Value = _mapper.Map<List<CardVMForDeck>>(collection2) };
        }
    }
}
