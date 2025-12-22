namespace Comment.Infrastructure.Services.Comment.GetReply.Request
{
    public record GetReplyRequest(Guid CommentId, DateTime? After)
    {
    }
}
