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

        /// <summary>
        /// Marks a single specific notification as read.
        /// Validates existence and ensures the requester is the legitimate recipient.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="NotificationMarkReadRequest"/> with the notification ID.</param>
        /// <returns>
        /// A <see cref="StatusCodeResponse"/> with 204 (No Content) on success, 
        /// or 404 (Not Found) if the notification does not exist or belongs to another user.
        /// </returns>
        /// <remarks>
        /// Security and Logic:
        /// <list type="bullet">
        /// <item>
        /// <term>Ownership Validation:</term>
        /// <description>The query filters by both <c>Id</c> and <c>RecipientId</c> to prevent users from marking other people's notifications as read.</description>
        /// </item>
        /// <item>
        /// <term>State Management:</term>
        /// <description>Uses standard Entity Framework change tracking to update the <c>IsRead</c> property.</description>
        /// </item>
        /// </list>
        /// </remarks>
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
