using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Notification.MarkRead.Request;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Notification.MarkRead
{
    public class NotificationMarkReadConsumer : IConsumer<NotificationMarkReadRequest>
    {
        private readonly AppDbContext _appDbContext;
        public NotificationMarkReadConsumer(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;

        }
        public async Task Consume(ConsumeContext<NotificationMarkReadRequest> context)
        {
            var dto = context.Message;

            var notification = await _appDbContext.Notification
                .FirstOrDefaultAsync(n => n.Id == dto.Id && n.RecipientId == dto.CallerId);

            if (notification == null)
            {
                await context.RespondAsync(new StatusCodeResponse("not found", 404));
                return;
            }

            notification.IsRead = true;
            await _appDbContext.SaveChangesAsync();

            await context.RespondAsync(new StatusCodeResponse("marked", 204));
        }
    }
}
