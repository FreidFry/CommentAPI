using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Notification.MarkAllRead
{
    public interface IAllNotificationmarkReadHandler
    {
        Task<IActionResult> Handle(HttpContext httpContext, CancellationToken cancellationToken);
    }
}