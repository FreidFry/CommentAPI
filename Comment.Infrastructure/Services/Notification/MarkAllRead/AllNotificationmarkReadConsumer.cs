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

        /// <summary>
        /// Marks all unread notifications for a specific user as read in a single atomic operation.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="AllNotificationsMarkReadRequest"/>.</param>
        /// <returns>
        /// A <see cref="StatusCodeResponse"/> with 204 (No Content) indicating successful update.
        /// </returns>
        /// <remarks>
        /// Performance and Security:
        /// <list type="bullet">
        /// <item>
        /// <term>Direct Execution:</term>
        /// <description>Uses <c>ExecuteUpdateAsync</c> to perform a bulk update directly on the database level, avoiding the overhead of change tracking and memory allocation.</description>
        /// </item>
        /// <item>
        /// <term>Scope Isolation:</term>
        /// <description>Ensures only notifications belonging to the <c>CallerId</c> are affected, preventing cross-user data modification.</description>
        /// </item>
        /// <item>
        /// <term>State Filtering:</term>
        /// <description>Optimizes the query by targeting only records where <c>IsRead</c> is currently <c>false</c>.</description>
        /// </item>
        /// </list>
        /// </remarks>
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
