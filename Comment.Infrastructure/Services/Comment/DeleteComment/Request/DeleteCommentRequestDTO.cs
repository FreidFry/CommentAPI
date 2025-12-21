namespace Comment.Infrastructure.Services.Comment.DeleteComment.Request
{
    public record DeleteCommentRequestDTO(Guid CommentId, Guid? CallerId)
    {
    }
}
