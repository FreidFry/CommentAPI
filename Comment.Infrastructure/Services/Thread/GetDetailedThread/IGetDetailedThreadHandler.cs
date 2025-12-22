using Comment.Infrastructure.Services.Thread.GetDetailedThread.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.GetDetailedThread
{
    public interface IGetDetailedThreadHandler
    {
        Task<IActionResult> Handle(ThreadDetaliRequest dto, HttpContext httpContext, CancellationToken cancellationToken);
    }
}