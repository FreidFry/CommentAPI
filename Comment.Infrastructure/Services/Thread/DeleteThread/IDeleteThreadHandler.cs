using Comment.Infrastructure.Services.Thread.DeleteThread.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.DeleteThread
{
    public interface IDeleteThreadHandler
    {
        Task<IActionResult> Handle(DeleteThreadRequest request, HttpContext http, CancellationToken cancellationToken);
    }
}