namespace Comment.Infrastructure.Services.Thread.CreateThread.Request
{
    public record ThreadCreateRequestDTO(string Title, string Context, Guid? callerId);
}

