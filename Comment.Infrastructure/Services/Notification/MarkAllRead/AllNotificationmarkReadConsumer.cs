using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Notification.MarkAllRead.Request;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Notification.MarkAllRead
{
    public class AllNotificationmarkReadConsumer : IConsumer<AllNotificationsMarkReadRequest>
    {
        private readonly AppDbContext _appDbContext;
        public AllNotificationmarkReadConsumer(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;

        }
        public async Task Consume(ConsumeContext<AllNotificationsMarkReadRequest> context)
        {
            var dto = context.Message;

            await _appDbContext.Notification
            .Where(n => n.RecipientId == dto.CallerId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

            await context.RespondAsync(new StatusCodeResponse("marked", 204));
        }
    }
}
