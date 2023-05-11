using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Responses;
using MTGCapstone.API.Data.ViewModels;
using MTGCapstone.API.Extentions;
using MTGCapstone.API.Services.DomainServiceInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MTGCapstone.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DecksController : ControllerBase
    {
        private readonly IDeckService _deckService;
        private readonly ILogger<DecksController> _logger;

        public DecksController(IDeckService deckService, 
            ILogger<DecksController> logger)
        {
            _deckService = deckService
                ?? throw new ArgumentNullException(nameof(deckService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SearchDecks([FromQuery] GetDecksRequest getDecksRequest)
        {
            Response<List<DeckVM>> response = await _deckService.GetDecksAsync(getDecksRequest);

            return Ok(response);
        }

        
        [HttpGet("Personal")]
        public async Task<IActionResult> GetMyDecks([FromQuery] PersonalDecksRequest decksRequest)
        {
            Response<List<DeckVM>> response = await _deckService.GetMyDecksAsync(User.Id(), decksRequest);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("{deckId}", Name = "GetDeckById")]
        public async Task<ActionResult<DeckVM>> GetDeck(int deckId)
        {

            Response<DeckVM> response = await _deckService.GetDeckWithCardsAsync(deckId);
            if (!response.Success)
            {
                return response.StatusCode switch
                {
                    ResponseStatusCodes.BadRequest => BadRequest(response.Errors),
                    ResponseStatusCodes.Forbidden => Forbid(),
                    ResponseStatusCodes.NotFound => NotFound(response.Errors),
                    _ => StatusCode((int)ResponseStatusCodes.Error, response.Errors)
                };
            }
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeck(DeckForCreationDto deckDTOForCreation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<DeckVM> response = await _deckService.CreateDeckAsync(User.Id(), deckDTOForCreation);
            if (!response.Success)
            {
                return BadRequest(response.Errors);
            }
            return CreatedAtRoute("GetDeckById", new { id = response.Value!.Id }, response.Value);
        }

        [HttpPut("{deckId}")]
        public async Task<IActionResult> UpdateDeck(int deckId, DeckForUpdateDto deckForUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _deckService.UpdateDeckAsync(User.Id(), deckId, deckForUpdateDTO);

            if (!response.Success)
            {
                return response.StatusCode switch
                {
                    ResponseStatusCodes.BadRequest => BadRequest(response.Errors),
                    ResponseStatusCodes.Forbidden => Forbid(),
                    ResponseStatusCodes.NotFound => NotFound(response.Errors),
                    _ => StatusCode((int)ResponseStatusCodes.Error, response.Errors)
                };
            }
            return NoContent();
        }

        [HttpPatch("{deckId}")]
        public async Task<IActionResult> PatchDeck(int deckId, [FromBody] JsonPatchDocument<DeckForUpdateDto> patchDoc)
        {
            Response<DeckForUpdateDto> response = await _deckService.GetDeckForPatchDTOAsync(User.Id(), deckId);

            if (!response.Success || response.Value is null)
            {
                return response.StatusCode switch
                {
                    ResponseStatusCodes.BadRequest => BadRequest(response.Errors),
                    ResponseStatusCodes.Forbidden => Forbid(),
                    ResponseStatusCodes.NotFound => NotFound(response.Errors),
                    _ => StatusCode((int)ResponseStatusCodes.Error, response.Errors)
                };
            }

            patchDoc.ApplyTo(response.Value, ModelState);
            //The TryValidateModel will check the passed in model.
            //If invalid, it will add the errors to the ModelState.
            if (!ModelState.IsValid && !TryValidateModel(response.Value))
            {
                return BadRequest(ModelState);
            }

            await _deckService.UpdateDeckAsync(User.Id(), deckId, response.Value);
            return NoContent();
        }

        [HttpDelete("{deckId}")]
        public async Task<IActionResult> DeleteDeck(int deckId)
        {
            var response = await _deckService.DeleteDeckAsync(User.Id(), deckId);

            if (!response.Success)
            {
                return response.StatusCode switch
                {
                    ResponseStatusCodes.BadRequest => BadRequest(response.Errors),
                    ResponseStatusCodes.Forbidden => Forbid(),
                    ResponseStatusCodes.NotFound => NotFound(response.Errors),
                    _ => StatusCode((int)ResponseStatusCodes.Error, response.Errors)
                };
            }
            return NoContent();
        }



        //DeckCards 
        //TODO: Consider how to change route so that front end doesn't have to know about implementation of data structure.
        //GetCardInDeckById instead of GetDeckCardById.  UI passes in CardId becuase they don't know about DeckCards or DeckCardIds.
        //My service method will take CardId and return the proper info.

        //[HttpGet("{deckId}/Cards")]
        //public async Task<ActionResult<List<Card>?>> GetCardsForDeck(int deckId)
        //{
        //    Response<DeckVM> response = await _deckService.GetDeckWithCardsAsync(deckId);
        //    response.Value = response.Value.Cards;

        //    return Ok(response);

        //}

        [HttpGet("{deckId}/Cards/{deckCardId}", Name = "GetDeckCardById")]
        public async Task<ActionResult<DeckCard>> GetDeckCardById(int deckId, int deckCardId)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            DeckCard? deckCard = await _deckService.GetDeckCardByIdAsync(deckCardId);
            if (deckCard is null)
                return NotFound();

            return Ok(deckCard);
        }

        [HttpPost("{deckId}/Cards")]
        public async Task<IActionResult> AddNewCardToDeck(int deckId, [FromBody] int cardId)
        {
            Response<CardVMForDeck> response = await _deckService.AddNewCardToDeckAsync(User.Id(), deckId, cardId);

            if (!response.Success || response.Value is null)
            {
                return response.StatusCode switch
                {
                    ResponseStatusCodes.BadRequest => BadRequest(response.Errors),
                    ResponseStatusCodes.Forbidden => Forbid(),
                    ResponseStatusCodes.NotFound => NotFound(response.Errors),
                    _ => StatusCode((int)ResponseStatusCodes.Error, response.Errors)
                };
            }

            return CreatedAtRoute("GetDeckCardById", new { deckId, deckCardId = response.Value.DeckCardId }, response.Value);
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

            DeckCard? deckCard = await _deckService.GetDeckCardByIdAsync(deckCardId);
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

            DeckCard? deckCard = await _deckService.GetDeckCardByIdAsync(deckCardId);
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

            List<DeckCategory> deckCards = await _deckService.GetDeckCategoriesForDeck(deckId);

            return Ok(deckCards);
        }

        [HttpGet("{deckId}/DeckCategories/{deckCategoryId}", Name = "GetDeckCategoryById")]
        public async Task<ActionResult<DeckCategory>> GetDeckCategoryByIdAsync(int deckId, int deckCategoryId)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            DeckCategory? deckCategory = await _deckService.GetDeckCategoryByIdAsync(deckCategoryId);
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

            DeckCategory deckCategory = await _deckService.AddCategoryToDeckAsync(deckId, name);

            return CreatedAtRoute("GetDeckCategoryById", new { deckId = deckCategory.DeckId, deckCategoryId = deckCategory.Id }, deckCategory);
        }

        [HttpPut("{deckId}/DeckCategories/{deckCategoryId}")]
        public async Task<IActionResult> UpdateDeckCategoryNameAsync(int deckId, int deckCategoryId, string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                return BadRequest("No deckCategoryName in request.");

            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            DeckCategory? deckCategory = await _deckService.GetDeckCategoryByIdAsync(deckCategoryId);
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

            DeckCategory? deckCategory = await _deckService.GetDeckCategoryByIdAsync(deckCategoryId);
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

            Like? like = await _deckService.GetLikeByIdAsync(likeId);

            if (like is null)
                return NotFound();

            return Ok(like);
        }

        [HttpPut("{deckId}/Likes")]
        public async Task<IActionResult> LikeDeckAsync(int deckId)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound();

            Like like = await _deckService.LikeDeckAsync(deckId, User.Id());

            return NoContent();
        }

        [HttpDelete("{deckId}/Likes")]
        public async Task<IActionResult> UnLikeDeckAsync(int deckId)
        {
            if (!await _deckService.DeckExistsAsync(deckId))
                return NotFound($"No deck found with Id:{deckId}.");

            string? likingUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (likingUserIdString is null || int.TryParse(likingUserIdString, out int likingUserId))
                return BadRequest();

            

            await _deckService.UnLikeDeckAsync(deckId, likingUserId);

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

            string? commentingUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (commentingUserIdString is null || int.TryParse(commentingUserIdString, out int commentingUserId))
                return BadRequest();

            await _deckService.CommentOnDeckAsync(deckId, commentingUserId, commentDTO);

            return NoContent();

        }

    }
}
