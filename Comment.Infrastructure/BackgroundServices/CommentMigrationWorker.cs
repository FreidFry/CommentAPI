using Comment.Core.Data;
using Comment.Core.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    public class CommentMigrationWorker : BackgroundService
    {
        private readonly IDatabase _redis;
        private readonly IServiceProvider _services;

        public CommentMigrationWorker(IConnectionMultiplexer redis, IServiceProvider services)
        {
            _redis = redis.GetDatabase();
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var commentJson = await _redis.ListRightPopAsync("comments_queue");

                if (!commentJson.IsNull)
                    using (var scope = _services.CreateScope())
                    {
                        try
                        {
                            var comment = JsonSerializer.Deserialize<CommentModel>(commentJson.ToString());

                            var db = _services.GetRequiredService<AppDbContext>();
                            db.Comments.Add(comment);
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка сохранения в БД: {ex.Message}");
                            await _redis.ListLeftPushAsync("comments_queue", commentJson);
                        }
                    }
                else
                    await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
