using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.ViewModels;
using MTGCapstone.API.Services.DomainServiceInterfaces;
using System.Security.Claims;

namespace MTGCapstone.API.Controllers
{
    [Route("api/Decks")]
    [Authorize]
    [ApiController]
    public class DecksController : ControllerBase
    {
        private readonly IDeckService _deckService;
        private readonly ILogger<DecksController> _logger;

        public DecksController(IDeckService deckService, ILogger<DecksController> logger, IMapper mapper)
        {
            _deckService = deckService
                ?? throw new ArgumentNullException(nameof(deckService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        //Decks
        //TODO: Get Users Decks once figure out userId/Authentication
        //TODO: Add Authentication to Create Deck.
        //TODO: Add Authorization checks for manipulating decks.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeckVM>>> GetDecks(DeckSearchFilterParameters deckSearchFilterParameters)
        {
            var decksVM = await _deckService.GetDecksAsync(deckSearchFilterParameters);

            if (decksVM is null)
                return NotFound();

            return Ok(decksVM);
        }

        [HttpGet("{deckId}", Name = "GetDeckById")]
        public async Task<ActionResult<DeckVM>> GetDeck(int deckId)
        {
            if (deckId is 0)
            {
                return BadRequest("No deckId sent in request.");
            }
            var deckVM = await _deckService.GetDeckVMAsync(deckId);

            if (deckVM == null)
                return NotFound();

            return Ok(deckVM);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeck(DeckDTOForCreation deckDTOForCreation)
        {
            if (deckDTOForCreation is null)
                return BadRequest("DeckDTO sent from client is null.");
            //if (_deckService.) //TODO: Figure out how to authenticate user id


            if (!ModelState.IsValid || deckDTOForCreation.UserId == 0)
                return UnprocessableEntity(ModelState);

            var deckVM = await _deckService.CreateDeckAsync(deckDTOForCreation);

            return CreatedAtRoute("GetDeckById", new { id = deckVM.Id }, deckVM);
        }

        [HttpPut("{deckId}")]
        public async Task<IActionResult> UpdateDeck(int deckId, DeckForUpdateDTO deckForUpdateDTO)
        {
            //TODO: Add authorization to edit this deck.
            if (deckForUpdateDTO is null)
                return BadRequest("DeckForUpdateDTO is null.");

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            await _deckService.UpdateDeck(deckId, deckForUpdateDTO);

            return NoContent();
        }

        [HttpPatch("{deckId}")]
        public async Task<IActionResult> PatchDeck(int deckId, [FromBody] JsonPatchDocument<DeckForUpdateDTO> patchDoc)
        {
            //TODO: Add authorization to edit this deck.

            if (patchDoc is null)
                return BadRequest("PatchDoc from client was null");

            var deckForUpdateDTO = await _deckService.GetDeckForUpdateDTOAsync(deckId);

            if (deckForUpdateDTO is null)
                return NotFound($"No deck found with Id:{deckId}.");

            patchDoc.ApplyTo(deckForUpdateDTO, ModelState);

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            await _deckService.UpdateDeck(deckId, deckForUpdateDTO);

            return NoContent();
        }

        [HttpDelete("{deckId}")]
        public async Task<IActionResult> DeleteDeck(int deckId)
        {
            //TODO: Add authorization to edit this deck.

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound();

            await _deckService.DeleteDeck(deckId);

            return NoContent();
        }



        //DeckCards 
        //TODO: Consider how to change route so that front end doesn't have to know about implementation of data structure.
        //GetCardInDeckById instead of GetDeckCardById.  UI passes in CardId becuase they don't know about DeckCards or DeckCardIds.
        //My service method will take CardId and return the proper info.

        [HttpGet("{deckId}/Cards")]
        public async Task<ActionResult<List<DeckCard>?>> GetCardsForDeck(int deckId)
        {
            if (deckId is 0)
                return BadRequest("No deckId sent in request.");

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"Deck with id {deckId} not found in Database.");

            var deckCards = await _deckService.GetDeckCardsForDeck(deckId);

            return Ok(deckCards);

        }

        [HttpGet("{deckId}/Cards/{deckCardId}", Name = "GetDeckCardById")]
        public async Task<ActionResult<DeckCard>> GetDeckCardById(int deckId, int deckCardId)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            var deckCard = await _deckService.GetDeckCardByIdAsync(deckCardId);
            if (deckCard is null)
                return NotFound();

            return Ok(deckCard);
        }

        [HttpPost("{deckId}/Cards")]
        public async Task<IActionResult> AddCardToDeck(int deckId, int cardId)
        {
            //TODO: Add authorization to edit this deck.

            if (cardId == 0)
                return BadRequest("No cardId in request.");

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            if (!await _deckService.CardExistsAsync(cardId))
                return NotFound($"No card found with Id:{cardId}.");

            var deckCard = await _deckService.AddCardToDeckAsync(deckId, cardId);

            return CreatedAtRoute("GetDeckCardById", new { deckId = deckCard.DeckId, deckCardId = deckCard.Id }, deckCard);
        }

        [HttpPut("{deckId}/Cards/{deckCardId}")]
        public async Task<IActionResult> ChangePrintingForDeckCard(int deckId, int deckCardId, int cardId)
        {
            if (cardId == 0)
                return BadRequest("No cardId in request.");

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            if (!await _deckService.CardExistsAsync(cardId))
                return NotFound($"No card found with Id:{cardId}.");

            var deckCard = await _deckService.GetDeckCardByIdAsync(deckCardId);
            if (deckCard is null)
                return NotFound($"No deckCard found with Id:{deckCardId}.");

            await _deckService.UpdateDeckCardPrintingAsync(deckCardId, cardId);

            return NoContent();
        }

        [HttpDelete("{deckId}/Cards/{deckCardId}")]
        public async Task<IActionResult> RemoveCardFromDeck(int deckId, int deckCardId)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            var deckCard = await _deckService.GetDeckCardByIdAsync(deckCardId);
            if (deckCard is null)
                return NotFound($"No deckCard found with Id:{deckCardId}.");

            await _deckService.DeleteDeckCardAsync(deckCardId);

            return NoContent();
        }



        //DeckCategories

        [HttpGet("{deckId}/DeckCategories")]
        public async Task<ActionResult<List<DeckCategory>?>> GetDeckCategoriesForDeck(int deckId)
        {
            if (deckId is 0)
                return BadRequest("No deckId sent in request.");

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"Deck with id {deckId} not found in Database.");

            var deckCards = await _deckService.GetDeckCategoriesForDeck(deckId);

            return Ok(deckCards);
        }

        [HttpGet("{deckId}/DeckCategories/{deckCategoryId}", Name = "GetDeckCategoryById")]
        public async Task<ActionResult<DeckCategory>> GetDeckCategoryByIdAsync(int deckId, int deckCategoryId)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            var deckCategory = await _deckService.GetDeckCategoryByIdAsync(deckCategoryId);
            if (deckCategory is null)
                return NotFound();

            return Ok(deckCategory);
        }

        [HttpPost("{deckId}/DeckCategories")]
        public async Task<IActionResult> AddCategoryToDeckAsync(int deckId, string name)
        {
            //TODO: Add authorization to edit this deck.

            if (String.IsNullOrWhiteSpace(name))
                return BadRequest($"Must provide name for deck category.");

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            var deckCategory = await _deckService.AddCategoryToDeckAsync(deckId, name);

            return CreatedAtRoute("GetDeckCategoryById", new { deckId = deckCategory.DeckId, deckCategoryId = deckCategory.Id }, deckCategory);
        }

        [HttpPut("{deckId}/DeckCategories/{deckCategoryId}")]
        public async Task<IActionResult> UpdateDeckCategoryNameAsync(int deckId, int deckCategoryId, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return BadRequest("No deckCategoryName in request.");

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            var deckCategory = await _deckService.GetDeckCategoryByIdAsync(deckCategoryId);
            if (deckCategory is null)
                return NotFound($"No deckCategory found with Id:{deckCategoryId}.");

            await _deckService.UpdateDeckCategoryAsync(deckCategoryId, name);

            return NoContent();
        }

        [HttpDelete("{deckId}/DeckCategories/{deckCategoryId}")]
        public async Task<IActionResult> RemoveDeckCategoryFromDeckAsync(int deckId, int deckCategoryId)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            var deckCategory = await _deckService.GetDeckCategoryByIdAsync(deckCategoryId);
            if (deckCategory is null)
                return NotFound($"No deckCard found with Id:{deckCategoryId}.");

            await _deckService.DeleteDeckCategoryAsync(deckCategoryId);

            return NoContent();
        }

        //TODO: Get User Categories once authentication is figured out


        //Likes

        [HttpGet("{deckId}/Likes/{likeId}", Name = "GetLikeById")]
        public async Task<ActionResult<Like>> GetLikeById(int deckId, int likeId)
        {
            //Not sure GetLikeById is needed, but oh well.

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            var like = await _deckService.GetLikeByIdAsync(likeId);

            if (like is null)
                return NotFound();

            return Ok(like);
        }

        [HttpPut("{deckId}/Likes")]
        public async Task<IActionResult> LikeDeckAsync(int deckId)
        {
            //TODO: Make sure user is Authenticated.  Add Authentication attribute.



            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            var likingUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (likingUserIdString is null || !Int32.TryParse(likingUserIdString, out int userId))
                return BadRequest();


            var like = await _deckService.LikeDeckAsync(deckId, userId);

            return NoContent();

        }

        [HttpDelete("{deckId}/Likes")]
        public async Task<IActionResult> UnLikeDeckAsync(int deckId)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            var likingUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (likingUserIdString is null || !Int32.TryParse(likingUserIdString, out int userId))
                return BadRequest();

            //ASK: How do I know when the UI will have access to IDs?
            //It makes the difference for my service search
            //on whether I use Find(byId) or FirstOrDefault(byCondition)
            //Leaving this here to confirm, but UI will send bearer token on every request,
            //which has the userId(string) as one of the claims.

            await _deckService.UnLikeDeckAsync(deckId, userId);

            return NoContent();
        }

        //Comments

        [HttpPut("{deckId}/Comments")]
        public async Task<IActionResult> CommentOnDeckAsync(int deckId, CommentDTO commentDTO)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            var commentingUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (commentingUser is null || !Int32.TryParse(commentingUser, out int userId))
                return BadRequest();

            await _deckService.CommentOnDeckAsync(deckId, userId, commentDTO);

            return NoContent();

        }

    }
}
