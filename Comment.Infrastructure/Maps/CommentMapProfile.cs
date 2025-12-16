using AutoMapper;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Comment.DTOs.Response;

namespace Comment.Infrastructure.Maps
{
    public class CommentMapProfile : Profile
    {
        public CommentMapProfile()
        {
            CreateMap<CommentModel, CommentResponseDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.ThreadId, opt => opt.MapFrom(src => src.ThreadId))
                .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.ParentCommentId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.AvatarTumbnailUrl, opt => opt.MapFrom(src => src.User.AvatarTumbnailUrl))
                .ForMember(dest => dest.imageTumbnailUrl, opt => opt.MapFrom(src => src.ImageTumbnailUrl))
                .ForMember(dest => dest.imageUrl, opt => opt.MapFrom(src => src.ImageUrl));

        }
    }
}

