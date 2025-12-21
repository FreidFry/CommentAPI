namespace Comment.Infrastructure.Services.Comment.DTOs.Request
{
    public record CommentUpdateRequestDTO(Guid? callerId, Guid CommentId, string Content);
}

