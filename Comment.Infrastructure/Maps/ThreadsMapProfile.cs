using AutoMapper;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Thread.DTOs.Response;

namespace Comment.Infrastructure.Maps
{
    public class ThreadsMapProfile : Profile
    {
        public ThreadsMapProfile()
        {
            CreateMap<ThreadModel, ThreadsThreeDTOResponce>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Context))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count(c => !c.IsDeleted)));

            CreateMap<ThreadModel, ThreadResponseDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Context, opt => opt.MapFrom(src => src.Context))
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId))
                .ForMember(dest => dest.OwnerUserName, opt => opt.MapFrom(src => src.OwnerUser.UserName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(src => src.LastUpdatedAt))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count(c => !c.IsDeleted)));
        }
    }
}
