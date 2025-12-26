using Comment.Core.Data;
using Comment.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Sends several threads to the database 
    /// </summary>
    public class ThreadMigrationWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _services;
        private readonly ILogger<ThreadMigrationWorker> _logger;
        private readonly string queueName = "threads_queue";

        public ThreadMigrationWorker(IConnectionMultiplexer redis, IServiceProvider services, ILogger<ThreadMigrationWorker> logger)
        {
            _redis = redis;
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ThreadMigrationWorker started. Watching queue: {QueueName}", queueName);
            var redisDb = _redis.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var threadJson = await redisDb.ListLeftPopAsync(queueName, 30);

                    if (threadJson == null || !threadJson.Any())
                    {
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }

                    var sw = Stopwatch.StartNew();
                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var newThreads = new List<ThreadModel>();

                        foreach (var json in threadJson)
                        {
                            try
                            {
                                var comment = JsonSerializer.Deserialize<ThreadModel>(json.ToString());
                                if (comment != null) newThreads.Add(comment);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to deserialize thread JSON. Skipping item.");
                            }
                        }
                        if (newThreads.Any())
                        {
                            await db.Threads.AddRangeAsync(newThreads);
                            await db.SaveChangesAsync(stoppingToken);
                        }
                        sw.Stop();
                        _logger.LogInformation("Batch of {Count} Threads migrated in {Elapsed}ms",
                            threadJson.Length, sw.ElapsedMilliseconds);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error in ThreadMigrationWorker. Restarting loop in 5s...");
                    await Task.Delay(5000, stoppingToken);
                }

            }

        }
    }
}

