using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Comment.Infrastructure.Services.Thread.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    public class CommentMigrationWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _services;

        public CommentMigrationWorker(IConnectionMultiplexer redis, IServiceProvider services)
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
                            var comment = JsonSerializer.Deserialize<CommentModel>(json.ToString());
                            if (comment != null)
                                newComments.Add(comment);
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
                        }
                    }
                }
                catch (Exception ex)
                {
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
