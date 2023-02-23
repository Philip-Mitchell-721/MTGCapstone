using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Services;

namespace MTGCapstone.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly ICardService _cardService;
        private readonly ILogger<CardController> _logger;

        public CardController(ICardService cardService, ILogger<CardController> logger)
        {
            _cardService = cardService
                ?? throw new ArgumentNullException(nameof(cardService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/Cards
        [HttpGet]
        public async Task<ActionResult<List<Card>>> GetCardsAsync([FromQuery] CardResourceParameters cardResourceParameters)
        {
            var cards = await _cardService.GetCardsAsync(cardResourceParameters);
            //TODO: Figure out why GetCardsAsync isn't happy now.  
            if (cards == null || cards.Count == 0)
                return NotFound();

            return Ok(cards);//TODO: Create CardVM and change to that anywhere I return a card.
        }

        // GET: api/Cards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Card>> GetCard(int id)
        {

            var card = await _cardService.GetCardAsync(id);

            if (card is null)
            {
                _logger.LogError("Card with id {Id} not found in Db.", id);
                return NotFound();
            }

            return Ok(card);
        }
    }
}
