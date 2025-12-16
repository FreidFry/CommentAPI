namespace Comment.Infrastructure.Services.Comment.DTOs.Response
{
    public class CommentTreeDTO
    {
        public ICollection<CommentResponseDTO> Comments { get; set; } = [];
    }
}

