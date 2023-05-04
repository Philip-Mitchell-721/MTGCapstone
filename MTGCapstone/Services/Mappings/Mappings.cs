using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Responses;

namespace MTGCapstone.API.Services.Mappings
{
    public static class Mappings
    {
        //This is done using the MappingGenerator
        public static Deck MapToDeck(this DeckForCreationDto deckDTOForCreation)
        {
            return new Deck
            {
                Name = deckDTOForCreation.Name,
                IsPrivate = deckDTOForCreation.IsPrivate,
                Format = deckDTOForCreation.Format,
                Primer = deckDTOForCreation.Primer
            };
        }
        public static Response<DeckForUpdateDto> MapToResponseOfDeckForUpDateDto(this Response<Deck> response)
        {
            return new Response<DeckForUpdateDto>
            {
                StatusCode = response.StatusCode,
                Message = response.Message,
                Errors = response.Errors,
                Value = response.Value != null ? new DeckForUpdateDto
                {
                    Name = response.Value.Name,
                    IsPrivate = response.Value.IsPrivate,
                    Format = response.Value.Format,
                    Primer = response.Value.Primer
                } : null,
                Success = response.Success
            };
        }
    }
}
