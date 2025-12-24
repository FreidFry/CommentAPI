using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Notification.GetHistory
{
    public interface IGetNotificationHistoryHandler
    {
        Task<IActionResult> Handle(HttpContext httpContext, CancellationToken cancellationToken);
    }
}