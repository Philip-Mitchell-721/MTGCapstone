using Microsoft.EntityFrameworkCore;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.DbContexts;
using System.Reflection;

namespace MTGCapstone.API.Services
{
    public class CardService : ICardService
    {
        private readonly CapstoneDbContext _context;
        private readonly ILogger<CardService> _logger;

        public CardService(CapstoneDbContext context,
            ILogger<CardService> logger)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Card?> GetCardAsync(int id)
        {
            return await _context.Cards.FindAsync(id);
        }

        public async Task<List<Card>?> GetCardsAsync(CardResourceParameters cardResourceParameters)
        {
            var collection = _context.Cards as IQueryable<Card>;


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
                Card card = new(); 
                //TODO: Change this to DTO later (so that you can only match props I want to expose)
                var orderByInfo = card.GetType().GetProperty(cardResourceParameters.OrderBy);
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
            var collectionToReturn = collection
                .GroupBy(c => c.OracleId)
                .Select(c => c.First())
                .OrderBy(c => c.EdhrecRank);


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


            var collection2 = await collection.Skip(cardResourceParameters.PageSize * (cardResourceParameters.PageNumber - 1))
                .Take(cardResourceParameters.PageSize).ToListAsync();

            return collection2;
        }
    }
}
