namespace Comment.Infrastructure.Events
{
    public record CommentRepliedEvent(
    Guid ParentAuthorId,    
    Guid ReplyAuthorId,     
    Guid ThreadId,
    DateTime CreatedAt
);
}
