using Comment.Infrastructure.Services.Thread.CreateThread.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.CreateThread
{
    public interface ICreateThreadHandler
    {
        Task<IActionResult> Handle(ThreadCreateRequest dto, HttpContext httpContext, CancellationToken cancellationToken);
    }
}