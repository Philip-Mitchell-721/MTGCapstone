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
                .ForMember(dest => dest.Likes,
                            opt => opt.MapFrom(src => src.Likes.Count));
            CreateMap<DeckForCreationDto, Deck>();
            CreateMap<DeckForUpdateDto, Deck>().ReverseMap();
            CreateMap<DeckVM, DeckForUpdateDto>();
        }
    }

    public class CardProfile : Profile
    {
        public CardProfile()
        {
            CreateMap<Card, CardVMForDeck>()
                .ForMember(dest => dest.Colors,
                            opt => opt.Ignore())
                .ForMember(dest => dest.ColorIdentity,
                            opt => opt.Ignore())
                .ForMember(dest => dest.Keywords,
                            opt => opt.Ignore())
                .ForMember(dest => dest.CardFaces,
                            opt => opt.Ignore());
                
            CreateMap<ImageUris, ImageUrisVM>();
            CreateMap<FormatLegalities, FormatLegalitiesVM>();
            CreateMap<Prices, PricesVM>();
            CreateMap<RelatedUris, RelatedUrisVM>();
            CreateMap<PurchaseUris, PurchaseUrisVM>();
            CreateMap<CardFace, CardFaceVM>()
                .ForMember(dest => dest.Colors,
                            opt => opt.Ignore());
            
        }
    }
}
