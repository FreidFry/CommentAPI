
using AutoMapper;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.User.DTOs.Request;
using Comment.Infrastructure.Services.User.DTOs.Response;

namespace Comment.Infrastructure.Maps.UserMaps
{
    public class UserMapProfile : Profile
    {
        public UserMapProfile()
        {
            CreateMap<UserModel, CommonUserDataDTO>()
                .ForMember(dest => dest.AvatarTumbnailUrl, opt => opt.MapFrom(src => src.AvatarTumbnailUrl))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastActive, opt => opt.MapFrom(src => src.LastActive));

            CreateMap<UserModel, UserUpdateAvatarDTO>()
                .ForMember(dest => dest.AvatarId, opt => opt.Ignore());
        }
    }
}
