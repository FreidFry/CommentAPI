using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    public class NotificationMigrationWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _services;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ILogger<NotificationMigrationWorker> _logger;

        public NotificationMigrationWorker(IConnectionMultiplexer redis, IServiceProvider services, IMapper mapper, IHubContext<NotificationHub> notificationHub, ILogger<NotificationMigrationWorker> logger)
        {
            _redis = redis;
            _services = services;
            _mapper = mapper;
            _notificationHub = notificationHub;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationMigrationWorker started.");
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
                    var sw = Stopwatch.StartNew();
                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var newNotification = new List<NotificationModel>();

                        foreach (var json in notificationsJson)
                        {
                            try
                            {
                                var notification = JsonSerializer.Deserialize<NotificationModel>(json.ToString());
                                if (notification != null)
                                    newNotification.Add(notification);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to deserialize notification. Raw data: {Json}", json.ToString());
                            }
                        }
                        if (newNotification.Any())
                        {
                            await db.Notification.AddRangeAsync(newNotification);
                            await db.SaveChangesAsync(stoppingToken);

                            int sentCount = 0;
                            foreach (var notification in newNotification)
                            {
                                try
                                {
                                    var viewModel = _mapper.Map<NotificationViewModel>(notification);
                                    await _notificationHub.Clients.User(notification.RecipientId.ToString())
                                        .SendAsync("ReceiveNotification", viewModel, stoppingToken);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to send SignalR notification to user {UserId}", notification.RecipientId);
                                }
                            }
                            sw.Stop();
                            _logger.LogInformation("Processed {Count} notifications. Database saved. SignalR sent: {SentCount}. Time: {Elapsed}ms",
                                newNotification.Count, sentCount, sw.ElapsedMilliseconds);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error in NotificationMigrationWorker loop");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

    }
}
