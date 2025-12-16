namespace Comment.Infrastructure.Services.Comment.DTOs.Response
{
    public class CommentResponseDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid ThreadId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? AvatarTumbnailUrl { get; set; }
        public string? imageTumbnailUrl { get; set; }
        public string? imageUrl { get; set; }
        public List<CommentResponseDTO> Replies { get; set; } = [];
    }

}