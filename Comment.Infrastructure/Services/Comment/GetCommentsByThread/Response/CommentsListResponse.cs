using Comment.Infrastructure.Services.Comment.DTOs.Response;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response
{
    public record CommentsListResponse
    {
        public List<CommentViewModel>? Items { get; set; }
        public string? NextCursor { get; set; }
        public bool HasMore { get; set; }
    }
}
