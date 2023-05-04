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
            CreateMap<Deck, DeckVM>()
                .ForMember(dest => dest.OwnerId,
                            opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Owner,
                            opt => opt.MapFrom(src => src.User));
            CreateMap<DeckForCreationDto, Deck>();
            CreateMap<DeckForUpdateDto, Deck>().ReverseMap();
            CreateMap<DeckVM, DeckForUpdateDto>();
        }
    }

    public class CardProfile : Profile
    {
        public CardProfile()
        {
            CreateMap<Card, CardVMForDeck>();
                
            CreateMap<ImageUris, ImageUrisVM>();
            CreateMap<FormatLegalities, FormatLegalitiesVM>();
            CreateMap<Prices, PricesVM>();
            CreateMap<RelatedUris, RelatedUrisVM>();
            CreateMap<PurchaseUris, PurchaseUrisVM>();
            CreateMap<CardFace, CardFaceVM>();
            
        }
    }
}
