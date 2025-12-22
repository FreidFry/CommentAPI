using Comment.Infrastructure.Services.Thread.RestoreThread.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.RestoreThread
{
    public interface IRestoreThreadHandler
    {
        Task<IActionResult> Handle(RestoreThreadRequest request, HttpContext http, CancellationToken cancellationToken);
    }
}