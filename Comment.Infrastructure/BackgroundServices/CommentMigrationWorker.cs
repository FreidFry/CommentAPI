using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Thread.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Responsible for periodically transferring comments from Redis to the database and updating the count in Redis.
    /// </summary>
    public class CommentMigrationWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _services;
        private readonly ILogger<CommentMigrationWorker> _logger;


        public CommentMigrationWorker(IConnectionMultiplexer redis, IServiceProvider services, ILogger<CommentMigrationWorker> logger)
        {
            _redis = redis;
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CommentMigrationWorker started.");
            var redisDb = _redis.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    var commentsJson = await redisDb.ListRightPopAsync("comments_queue", 30);

                    if (commentsJson == null || !commentsJson.Any())
                    {
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }

                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var newComments = new List<CommentModel>();

                        foreach (var json in commentsJson)
                        {
                            try
                            {
                                var comment = JsonSerializer.Deserialize<CommentModel>(json.ToString());
                                if (comment != null)
                                    newComments.Add(comment);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to deserialize comment JSON: {Json}", json.ToString());
                            }
                            if (newComments.Any())
                            {
                                await db.Comments.AddRangeAsync(newComments);
                                await db.SaveChangesAsync(stoppingToken);

                                foreach (var threadId in newComments.Select(c => c.ThreadId).Distinct())
                                {
                                    var countInBatch = newComments.Count(c => c.ThreadId == threadId);
                                    await IncrementThreadCommentsCount(redisDb, threadId, countInBatch);
                                }

                                sw.Stop();
                                _logger.LogInformation("Successfully migrated {Count} comments in {Elapsed}ms",
                                newComments.Count, sw.ElapsedMilliseconds);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error in CommentMigrationWorker during execution");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private async Task IncrementCommentsCount(IDatabase db, Guid threadId, int count)
        {
            var key = $"thread:{threadId}:details";
            var json = await db.StringGetAsync(key);
            if (!json.IsNull)
            {
                var preview = JsonSerializer.Deserialize<ThreadsResponseViewModel>(json.ToString());
                preview.CommentCount += count;
                await db.StringSetAsync(key, JsonSerializer.Serialize(preview), TimeSpan.FromHours(1));
            }
        }

        private async Task IncrementThreadCommentsCount(IDatabase db, Guid threadId, int count)
        {
            var key = $"thread:{threadId}:preview";
            var json = await db.StringGetAsync(key);
            if (!json.IsNull)
            {
                var preview = JsonSerializer.Deserialize<ThreadsResponseViewModel>(json.ToString());
                preview.CommentCount += count;
                await db.StringSetAsync(key, JsonSerializer.Serialize(preview), TimeSpan.FromHours(1));
            }
        }
    }
}
