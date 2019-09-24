using AutoMapper;
using SocialRide.Dtos;
using SocialRide.Models;

namespace SocialRide.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
        }
    }
}
