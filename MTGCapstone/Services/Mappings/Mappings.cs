using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;

namespace MTGCapstone.API.Services.Mappings
{
    public static class Mappings
    {
        //This is done using the MappingGenerator
        public static Deck MapToDeck(this DeckDTOForCreation deckDTOForCreation)
        {
            return new Deck
            {
                UserId = deckDTOForCreation.UserId,
                Name = deckDTOForCreation.Name,
                IsPrivate = deckDTOForCreation.IsPrivate,
                Format = deckDTOForCreation.Format,
                Primer = deckDTOForCreation.Primer
            };
        }
    }
}
