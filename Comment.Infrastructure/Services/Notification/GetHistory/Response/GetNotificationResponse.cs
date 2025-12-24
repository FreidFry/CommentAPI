using Comment.Infrastructure.CommonDTOs;

namespace Comment.Infrastructure.Services.Notification.GetHistory.Response
{
    public record GetNotificationResponse(List<NotificationViewModel> Notifications)
    {
    }
}
