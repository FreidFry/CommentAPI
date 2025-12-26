using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Notification.GetHistory.Request;
using Comment.Infrastructure.Services.Notification.GetHistory.Response;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Notification.GetHistory
{
    public class GetNotificationHistoryConsumer : IConsumer<GetNotificationRequest>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public GetNotificationHistoryConsumer(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a list of unread notifications for a specific user, ordered by creation date descending.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="GetNotificationRequest"/> with the target <c>UserId</c>.</param>
        /// <returns>
        /// A <see cref="GetNotificationResponse"/> containing a collection of <see cref="NotificationViewModel"/>.
        /// </returns>
        /// <remarks>
        /// This method implements a "pull" model for notifications:
        /// <list type="bullet">
        /// <item>
        /// <term>Recipient Filtering:</term>
        /// <description>Strictly filters records where <c>RecipientId</c> matches the caller's ID.</description>
        /// </item>
        /// <item>
        /// <term>Status Filtering:</term>
        /// <description>Fetches only active (unread) notifications (<c>IsRead == false</c>).</description>
        /// </item>
        /// <item>
        /// <term>Projection:</term>
        /// <description>Uses AutoMapper's <c>ProjectTo</c> for efficient SQL generation, fetching only required columns.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public async Task Consume(ConsumeContext<GetNotificationRequest> context)
        {
            var notifications = await _appDbContext.Notification
                .Where(n => n.RecipientId == context.Message.UserId && !n.IsRead)
                .OrderByDescending(n => n.CreateAt)
                .ProjectTo<NotificationViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(context.CancellationToken);

            await context.RespondAsync(new GetNotificationResponse(notifications));
        }
    }
}
