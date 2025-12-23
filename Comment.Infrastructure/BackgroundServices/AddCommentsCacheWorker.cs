using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    public class AddCommentsCacheWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _services;
        private readonly int _commentsToCache = 75;
        private bool BatchState = true;

        public AddCommentsCacheWorker(IConnectionMultiplexer redis, IServiceProvider services)
        {
            _redis = redis;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var redisDb = _redis.GetDatabase();

                    var activeThreadIds = await dbContext.Threads
                        .Where(t => !t.IsDeleted && !t.IsBanned)
                        .OrderByDescending(t => t.Comments.Max(c => c.CreatedAt))
                        .Select(t => t.Id)
                        .Take(100)
                        .ToListAsync(stoppingToken);

                    foreach (var threadId in activeThreadIds)
                    {
                        await WarmUpThreadCache(threadId, dbContext, SortByEnum.CreateAt, redisDb, stoppingToken);
                        await WarmUpThreadCache(threadId, dbContext, SortByEnum.UserName, redisDb, stoppingToken);
                        await WarmUpThreadCache(threadId, dbContext, SortByEnum.Email, redisDb, stoppingToken);
                        BatchState = true;
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }


        private async Task WarmUpThreadCache(Guid threadId, AppDbContext dbContext, SortByEnum sortBy, IDatabase redisDb, CancellationToken cancellatinToken)
        {
            var query = dbContext.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.ThreadId == threadId && c.ParentDepth == 0 && !c.IsDeleted && !c.IsBaned);

            List<CommentModel> comments = [];
            switch (sortBy)
            {
                case SortByEnum.CreateAt:
                    comments = await query.OrderByDescending(c => c.CreatedAt)
                        .Take(_commentsToCache)
                        .ToListAsync(cancellatinToken);
                    break;
                case SortByEnum.UserName:
                    comments = await query.OrderByDescending(c => c.User.UserName)
                        .Take(_commentsToCache)
                        .ToListAsync(cancellatinToken);
                    break;
                case SortByEnum.Email:
                    comments = await query.OrderByDescending(c => c.User.Email)
                        .Take(_commentsToCache)
                        .ToListAsync(cancellatinToken);
                    break;
                default:
                    comments = await query.OrderByDescending(c => c.CreatedAt)
                        .Take(_commentsToCache)
                        .ToListAsync(cancellatinToken);
                    break;
            }


            if (!comments.Any()) return;

            var key = sortBy switch
            {
                SortByEnum.CreateAt => $"thread:{threadId}:comments:sort:createat",
                SortByEnum.UserName => $"thread:{threadId}:comments:sort:username",
                SortByEnum.Email => $"thread:{threadId}:comments:sort:email",
                _ => $"thread:{threadId}:comments:sort:createat"
            };

            var batch = redisDb.CreateBatch();

            foreach (var comment in comments)
            {
                string json = JsonSerializer.Serialize(comment);

                if (BatchState)
                {
                    batch.StringSetAsync($"comment:{comment.Id}", json, TimeSpan.FromHours(1));
                    BatchState = false;
                }

                switch (sortBy)
                {
                    case SortByEnum.CreateAt:
                        batch.SortedSetAddAsync(key, comment.Id.ToString(), comment.CreatedAt.Ticks);
                        break;
                    case SortByEnum.UserName:
                        batch.SortedSetAddAsync(key, comment.Id.ToString(), GetNameScore(comment.User.UserName));
                        break;
                    case SortByEnum.Email:
                        batch.SortedSetAddAsync(key, comment.Id.ToString(), GetNameScore(comment.User.Email));
                        break;
                    default:
                        batch.SortedSetAddAsync(key, comment.Id.ToString(), comment.CreatedAt.Ticks);
                        break;
                }
            }
            batch.Execute();

            await redisDb.SortedSetRemoveRangeByRankAsync(key, 0, -(_commentsToCache + 1));
        }

        private static double GetNameScore(string name)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(name.ToLower().PadRight(8));
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}
