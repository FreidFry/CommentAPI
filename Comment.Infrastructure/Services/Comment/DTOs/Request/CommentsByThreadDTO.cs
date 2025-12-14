namespace Comment.Infrastructure.Services.Comment.DTOs.Request
{
    public record CommentsByThreadDTO(Guid ThreadId, DateTime? After = null, int Limit = 50);
}

