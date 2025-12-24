using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    public class NotificationMigrationWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _services;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public NotificationMigrationWorker(IConnectionMultiplexer redis, IServiceProvider services, IMapper mapper, IHubContext<NotificationHub> notificationHub)
        {
            _redis = redis;
            _services = services;
            _mapper = mapper;
            _notificationHub = notificationHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var redisDb = _redis.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var notificationsJson = await redisDb.ListLeftPopAsync("notification_queue", 30);

                    if (notificationsJson == null || !notificationsJson.Any())
                    {
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }
                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var newNotification = new List<NotificationModel>();

                        foreach (var json in notificationsJson)
                        {
                            var notification = JsonSerializer.Deserialize<NotificationModel>(json.ToString());
                            if (notification != null)
                            {
                                newNotification.Add(notification);
                            }
                        }
                        if (newNotification.Any())
                        {
                            await db.Notification.AddRangeAsync(newNotification);
                            await db.SaveChangesAsync(stoppingToken);

                            foreach (var notification in newNotification)
                            {
                                var viewModel = _mapper.Map<NotificationViewModel>(notification);
                                await _notificationHub.Clients.User(notification.RecipientId.ToString())
                                    .SendAsync("ReceiveNotification", viewModel, stoppingToken);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

    }
}
