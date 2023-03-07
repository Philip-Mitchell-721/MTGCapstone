using AutoMapper;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;

namespace MTGCapstone.API.Services.AutoMapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRegistrationModel, User>();
        }
    }
}
