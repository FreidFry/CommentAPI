namespace Comment.Infrastructure.Services.Thread.DeleteThread.Request
{
    public record DeleteThreadRequestDTO(Guid ThreadId, Guid? CallerId)
    {
    }
}
