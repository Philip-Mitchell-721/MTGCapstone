using AutoMapper;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.ViewModels;

namespace MTGCapstone.API.Services.AutoMapperProfiles
{
    public class DeckProfile : Profile
    {
        public DeckProfile()
        {
            CreateMap<Deck, DeckVM>();
            CreateMap<DeckDTOForCreation, Deck>();
            CreateMap<DeckForUpdateDTO, Deck>().ReverseMap();
            CreateMap<DeckVM, DeckForUpdateDTO>();
        }
    }
}
