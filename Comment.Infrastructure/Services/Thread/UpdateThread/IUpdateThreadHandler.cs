using Comment.Infrastructure.Services.Thread.UpdateThread.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.UpdateThread
{
    public interface IUpdateThreadHandler
    {
        Task<IActionResult> Handle(UpdateThreadRequest request, HttpContext http, CancellationToken cancellationToken);
    }
}