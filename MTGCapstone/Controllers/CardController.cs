using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Responses;
using MTGCapstone.API.Data.ViewModels;
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
            Response<List<CardVMForDeck>> response = await _cardService.GetCardsAsync(cardResourceParameters);

            return Ok(response.Value);
            //TODO: Create CardVM that isn't for the deck,
            //probably don't need to return a full CareVMForDeck.
        }

        // GET: api/Cards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Card>> GetCard(int id)
        {

            Card? card = await _cardService.GetCardAsync(id);

            if (card is null)
            {
                _logger.LogError("Card with id {Id} not found in Db.", id);
                return NotFound();
            }

            return Ok(card);
        }
    }
}
