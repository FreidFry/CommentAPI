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
        public async Task Consume(ConsumeContext<GetNotificationRequest> context)
        {
            var notifications = await _appDbContext.Notification
                .Where(n => n.RecipientId == context.Message.UserId && !n.IsRead)
                .OrderByDescending(n => n.CreateAt)
                .ProjectTo<NotificationViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(context.CancellationToken);
            notifications.Add(new NotificationViewModel(Guid.NewGuid(), "testc", "testc", "testc", DateTime.Now, "test", "test", Guid.NewGuid(), Guid.NewGuid(), false));

            await context.RespondAsync(new GetNotificationResponse(notifications));
        }
    }
}
