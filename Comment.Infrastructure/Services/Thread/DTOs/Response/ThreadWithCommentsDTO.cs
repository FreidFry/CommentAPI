using Comment.Infrastructure.Services.Comment.DTOs.Response;

namespace Comment.Infrastructure.Services.Thread.DTOs.Response
{
    public class ThreadWithCommentsDTO : ThreadResponseDTO
    {
        public List<CommentResponseDTO> Comments { get; set; } = [];
    }
}

