using Comment.Infrastructure.Services.Comment.DTOs.Response;

namespace Comment.Infrastructure.Services.Thread.DTOs.Response
{
    public class ThreadWithCommentsDTO : ThreadResponseDTO
    {
        public List<CommentTreeDTO> Comments { get; set; } = new();
    }
}

