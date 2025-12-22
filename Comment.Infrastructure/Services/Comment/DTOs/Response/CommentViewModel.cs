namespace Comment.Infrastructure.Services.Comment.DTOs.Response
{
    public class CommentViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid ThreadId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public Guid UserId { get; set; }
        public string? AvatarTumbnailUrl { get; set; }
        public string? ImageTumbnailUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? FileUrl { get; set; }
        public List<CommentViewModel> Replies { get; set; } = [];
        public int CommentCount { get; set; }
    }

}