using AutoMapper;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;

namespace Comment.Infrastructure.Maps
{
    public class NotificationMapProfile : Profile
    {
        public NotificationMapProfile()
        {
            CreateMap<NotificationModel, NotificationViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => src.CreateAt))
                .ForMember(dest => dest.ThreadId, opt => opt.MapFrom(src => src.ThreadId))
                .ForMember(dest => dest.CommentId, opt => opt.MapFrom(src => src.CommentId))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.CreatorUser.UserName))
                .ForMember(dest => dest.CreatorAvatarUrl, opt => opt.MapFrom(src => src.CreatorUser.AvatarTumbnailUrl));
        }
    }
}
