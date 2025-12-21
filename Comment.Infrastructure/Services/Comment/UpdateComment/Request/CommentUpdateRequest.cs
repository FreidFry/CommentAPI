namespace Comment.Infrastructure.Services.Comment.UpdateComment.Request
{
    public record CommentUpdateRequest(Guid CommentId, string Content);
}

