using Comment.Infrastructure.Services.Notification.GetHistory;
using Comment.Infrastructure.Services.Notification.MarkAllRead;
using Comment.Infrastructure.Services.Notification.MarkRead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IAllNotificationmarkReadHandler _allNotificationmarkReadHandler;
        private readonly INotificationMarkReadHandler _notificationMarkReadHandler;
        private readonly IGetNotificationHistoryHandler _getNotificationHistoryHandler;

        public NotificationController(IAllNotificationmarkReadHandler allNotificationmarkReadHandler, INotificationMarkReadHandler notificationMarkReadHandler, IGetNotificationHistoryHandler getNotificationHistoryHandler)
        {
            _allNotificationmarkReadHandler = allNotificationmarkReadHandler;
            _notificationMarkReadHandler = notificationMarkReadHandler;
            _getNotificationHistoryHandler = getNotificationHistoryHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
        {
            return await _getNotificationHistoryHandler.Handle(HttpContext, cancellationToken);
        }

        [HttpPatch("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        {
            return await _notificationMarkReadHandler.Handle(id, HttpContext, cancellationToken);
        }

        [HttpPatch("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        {
            return await _allNotificationmarkReadHandler.Handle(HttpContext, cancellationToken);
        }
    }
}
