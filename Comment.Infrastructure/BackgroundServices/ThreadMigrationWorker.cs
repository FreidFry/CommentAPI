using Comment.Core.Data;
using Comment.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    public class ThreadMigrationWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _services;

        public ThreadMigrationWorker(IConnectionMultiplexer redis, IServiceProvider services)
        {
            _redis = redis;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var redisDb = _redis.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var threadJson = await redisDb.ListRightPopAsync("threads_queue", 30);

                    if (threadJson == null || !threadJson.Any())
                    {
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }

                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var newThreads = new List<ThreadModel>();

                        foreach (var json in threadJson)
                        {
                            var comment = JsonSerializer.Deserialize<ThreadModel>(json.ToString());
                            if (comment != null) newThreads.Add(comment);
                        }
                        if (newThreads.Any())
                        {
                            await db.Threads.AddRangeAsync(newThreads);
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch
                {
                    await Task.Delay(5000, stoppingToken);
                }

            }
        }

    }
}

