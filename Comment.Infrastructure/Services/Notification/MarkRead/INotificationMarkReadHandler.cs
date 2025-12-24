using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Notification.MarkRead
{
    public interface INotificationMarkReadHandler
    {
        Task<IActionResult> Handle(Guid id, HttpContext httpContext, CancellationToken cancellationToken);
    }
}