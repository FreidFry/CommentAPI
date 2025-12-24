namespace Comment.Infrastructure.Services.Notification.MarkRead.Request
{
    public record NotificationMarkReadRequest(Guid Id, Guid? CallerId)
    {
    }
}
