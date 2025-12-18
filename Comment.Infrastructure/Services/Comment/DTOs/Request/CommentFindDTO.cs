namespace Comment.Infrastructure.Services.Comment.DTOs.Request
{
    public record CommentFindDTO(Guid CommentId, DateTime? after);
}

