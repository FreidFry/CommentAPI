namespace Comment.Infrastructure.Services.Comment.DTOs.Request
{
    public record CommentCreateDTO(string Content, Guid ThreadId, Guid? ParentCommentId = null);
}

