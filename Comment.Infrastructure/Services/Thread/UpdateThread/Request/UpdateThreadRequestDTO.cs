namespace Comment.Infrastructure.Services.Thread.UpdateThread.Request
{
    public record UpdateThreadRequestDTO(Guid ThreadId, string Title, string Context, Guid? CallerId)
    {
    }
}
