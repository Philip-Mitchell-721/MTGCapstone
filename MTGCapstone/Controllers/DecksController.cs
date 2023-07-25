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
        private int? UserId => User.Id();

        public DecksController(IDeckService deckService)
        {
            _deckService = deckService
                ?? throw new ArgumentNullException(nameof(deckService));
            
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
            if (!UserId.HasValue)
            {
                return BadRequest();
            }
            Response<List<DeckVM>> response = await _deckService.GetMyDecksAsync(UserId.Value, decksRequest);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("{deckId}", Name = "GetDeckById")]
        public async Task<ActionResult<DeckVM>> GetDeck(int deckId)
        {
            //var userId = UserId.Value;
            Response<DeckVM> response = await _deckService.GetDeckWithCardsAsync(UserId, deckId);
            
            return response.StatusCode switch
            {
                ResponseStatusCodes.Ok => Ok(response),
                ResponseStatusCodes.BadRequest => BadRequest(response),
                ResponseStatusCodes.NotFound => NotFound(response),
                _ => StatusCode((int)ResponseStatusCodes.Error, response)
            };
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeck(DeckForCreationDto deckDTOForCreation)
        {
            if (!ModelState.IsValid || !UserId.HasValue)
            {
                return BadRequest(ModelState);
            }

            Response<DeckVM> response = await _deckService.CreateDeckAsync(UserId.Value, deckDTOForCreation);

            return response.StatusCode switch
            {
                ResponseStatusCodes.BadRequest => BadRequest(response),
                ResponseStatusCodes.Created => CreatedAtRoute("GetDeckById", new { deckId = response.Value!.Id }, response),
                _ => StatusCode((int)ResponseStatusCodes.Error, response)
            };
        }
        

        [HttpPut("{deckId}")]
        public async Task<IActionResult> UpdateDeck(int deckId, DeckForUpdateDto deckForUpdateDTO)
        {
            if (!ModelState.IsValid || !UserId.HasValue)
            {
                return BadRequest(ModelState);
            }

            Response<Deck> response = await _deckService.UpdateDeckAsync(UserId.Value, deckId, deckForUpdateDTO);
            
            return response.StatusCode switch
            {
                ResponseStatusCodes.NoContent => NoContent(),
                ResponseStatusCodes.Forbidden => Forbid(), //For some reason, the Forbid helper doesn't allow an object (errors) to be passed back
                ResponseStatusCodes.NotFound => NotFound(response.Errors),
                _ => StatusCode((int)ResponseStatusCodes.Error, response.Errors)
            };
        }

        [HttpPatch("{deckId}")]
        public async Task<IActionResult> PatchDeck(int deckId, [FromBody] JsonPatchDocument<DeckForUpdateDto> patchDoc)
        {
            if (!UserId.HasValue)
            {
                return BadRequest();
            }

            Response<DeckForUpdateDto> response = await _deckService.GetDeckForPatchDTOAsync(UserId.Value, deckId);

            if (response.Success && response.Value is not null)
            {
                patchDoc.ApplyTo(response.Value, ModelState);
                //The TryValidateModel will check the passed in model.
                //If invalid, it will add the errors to the ModelState.
                if (!TryValidateModel(response.Value))
                {
                    return BadRequest(ModelState);
                }

                Response<Deck> updateResponse = await _deckService.UpdateDeckAsync(UserId.Value, deckId, response.Value);
                response.StatusCode = updateResponse.StatusCode;
            }
            
            return response.StatusCode switch
            {
                ResponseStatusCodes.NoContent => NoContent(),
                ResponseStatusCodes.Forbidden => Forbid(),
                ResponseStatusCodes.NotFound => NotFound(response.Errors),
                _ => StatusCode((int)ResponseStatusCodes.Error, response.Errors)
            };
        }

        [HttpDelete("{deckId}")]
        public async Task<IActionResult> DeleteDeck(int deckId)
        {
            try
            {
                if (!UserId.HasValue)
                {
                    return BadRequest();
                }
                var response = await _deckService.DeleteDeckAsync(UserId.Value, deckId);

                return response.StatusCode switch
                {
                    ResponseStatusCodes.NoContent => NoContent(),
                    ResponseStatusCodes.Forbidden => Forbid(),
                    ResponseStatusCodes.NotFound => NotFound(response.Errors),
                    _ => StatusCode((int)ResponseStatusCodes.Error, response.Errors)
                };

            }
            catch (Exception ex)
            {
                var response = new Response { Errors = { ex.Message, ex.InnerException?.Message ?? "" }, StatusCode = ResponseStatusCodes.Error, Success = false };
                return StatusCode((int)ResponseStatusCodes.Error, response);
            }
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
        public async Task<IActionResult> AddCardToDeck(int deckId, AddCardRequestDto requestDto)
        {
            if (!UserId.HasValue || (requestDto.CardId is null && string.IsNullOrWhiteSpace(requestDto.ScryfallId)))
            {
                return BadRequest();
            }
            Response<CardVMForDeck> response = await _deckService.AddCardToDeckAsync(UserId.Value, deckId, requestDto);

            return response.StatusCode switch
            {
                ResponseStatusCodes.Ok => Ok(response),
                ResponseStatusCodes.BadRequest => BadRequest(response),
                ResponseStatusCodes.Forbidden => StatusCode((int)ResponseStatusCodes.Forbidden, response),
                ResponseStatusCodes.NotFound => NotFound(response),
                _ => StatusCode((int)ResponseStatusCodes.Error, response)
            };
        }

        [HttpPut("{deckId}/Cards/{deckCardId}")]
        public async Task<IActionResult> ChangePrintingForDeckCard(int deckId, int deckCardId, [FromBody] int cardId)
        {
            if (!UserId.HasValue)
            {
                return BadRequest();
            }

            Response<CardVMForDeck> response = await _deckService.UpdateDeckCardPrintingAsync(UserId.Value, deckId, deckCardId, cardId);

            return response.StatusCode switch
            {
                ResponseStatusCodes.Ok => Ok(response),
                ResponseStatusCodes.Forbidden => Forbid(),
                ResponseStatusCodes.NotFound => NotFound(response),
                _ => StatusCode((int)ResponseStatusCodes.Error, response)
            };
        }

        [HttpDelete("{deckId}/Cards/{deckCardId}")]
        public async Task<IActionResult> RemoveCardFromDeck(int deckId, int deckCardId)
        {
            try
            {
                if (!UserId.HasValue)
                {
                    return BadRequest();
                }

                var response = await _deckService.RemoveCardFromDeckAsync(UserId.Value, deckId, deckCardId);

                return response.StatusCode switch
                {
                    ResponseStatusCodes.NoContent => NoContent(),
                    ResponseStatusCodes.Forbidden => Forbid(),
                    ResponseStatusCodes.NotFound => NotFound(response),
                    _ => StatusCode((int)ResponseStatusCodes.Error, response)
                };

            }
            catch (Exception ex)
            {
                var response = new Response { Errors = { ex.Message }, StatusCode = ResponseStatusCodes.Error, Success = false };
                return StatusCode((int)ResponseStatusCodes.Error, response);
            }
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
            if (!UserId.HasValue)
            {
                return BadRequest(ModelState);
            }
            if (!await _deckService.DeckExistsAsync(deckId))
            {
                return NotFound();
            }

            await _deckService.LikeDeckAsync(deckId, UserId.Value);

            return NoContent();
        }

        [HttpDelete("{deckId}/Likes")]
        public async Task<IActionResult> UnLikeDeckAsync(int deckId)
        {
            if (!UserId.HasValue)
            {
                return BadRequest();
            }

            Response response = await _deckService.UnLikeDeckAsync(deckId, UserId.Value);

            return response.StatusCode switch
            {
                ResponseStatusCodes.NoContent => NoContent(),
                ResponseStatusCodes.BadRequest => BadRequest(response.Errors),
                ResponseStatusCodes.Forbidden => Forbid(),
                ResponseStatusCodes.NotFound => NotFound(response.Errors),
                _ => StatusCode((int)ResponseStatusCodes.Error, response.Errors)
            };
        }

        //Comments

    //    [HttpPut("{deckId}/Comments")]
    //    public async Task<IActionResult> CommentOnDeckAsync(int deckId, CommentDTO commentDTO)
    //    {
    //        if (!await _deckService.DeckExistsAsync(deckId))
    //            return NotFound($"No deck found with Id:{deckId}.");

    //        if (!ModelState.IsValid)
    //            return UnprocessableEntity(ModelState);

    //        string? commentingUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //        if (commentingUserIdString is null || int.TryParse(commentingUserIdString, out int commentingUserId))
    //            return BadRequest();

    //        await _deckService.CommentOnDeckAsync(deckId, commentingUserId, commentDTO);

    //        return NoContent();

    //    }

    }
}
