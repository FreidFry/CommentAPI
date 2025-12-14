namespace Comment.Infrastructure.Services.Comment.DTOs.Response
{
    public class CommentTreeDTO : CommentResponseDTO
    {
        public List<CommentTreeDTO> Replies { get; set; } = new();
    }
}

