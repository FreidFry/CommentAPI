using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.UpdateThread.Request
{
    public record UpdateThreadRequest([FromRoute]Guid ThreadId, string Title, string Context)
    {
    }
}
