namespace Comment.Infrastructure.Services.Thread.DTOs.Request
{
    public record ThreadUpdateDTO(Guid ThreadId, string Title, string Context);
}

