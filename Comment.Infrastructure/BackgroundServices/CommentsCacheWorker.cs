using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.Enums;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Sends comments from posts on the front page to Redis at regular intervals for quick access.
    /// </summary>
    public class CommentsCacheWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _services;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentsCacheWorker> _logger;
        private readonly int _commentsToCache = 75;

        public CommentsCacheWorker(IConnectionMultiplexer redis, IServiceProvider services, IMapper mapper, ILogger<CommentsCacheWorker> logger)
        {
            _redis = redis;
            _services = services;
            _mapper = mapper;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CommentsCacheWorker started. Interval: 30 min.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    using (var scope = _services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var activeThreadIds = await dbContext.Threads
                            .AsNoTracking()
                            .Where(t => !t.IsDeleted && !t.IsBanned)
                            .OrderByDescending(t => t.Comments.Max(c => c.CreatedAt))
                            .Select(t => t.Id)
                            .Take(100)
                            .ToListAsync(stoppingToken);

                        _logger.LogInformation("Found {Count} active threads for cache warmup", activeThreadIds.Count);

                        int successCount = 0;
                        foreach (var threadId in activeThreadIds)
                        {
                            try
                            {
                                var redisDb = _redis.GetDatabase();
                                await WarmUpCommentCache(threadId, dbContext, SortByEnum.CreateAt, redisDb, stoppingToken, true);
                                await WarmUpCommentCache(threadId, dbContext, SortByEnum.UserName, redisDb, stoppingToken);
                                await WarmUpCommentCache(threadId, dbContext, SortByEnum.Email, redisDb, stoppingToken);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to warmup cache for thread {ThreadId}", threadId);
                            }
                        }
                        sw.Stop();
                        _logger.LogInformation("Cache warmup cycle completed. Processed {Success}/{Total} threads in {Elapsed}s",
                            successCount, activeThreadIds.Count, sw.Elapsed.TotalSeconds);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Fatal error in CommentsCacheWorker main loop");
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }

        }


        private async Task WarmUpCommentCache(Guid threadId, AppDbContext dbContext, SortByEnum sortBy, IDatabase redisDb, CancellationToken cancellatinToken, bool batchComment = false)
        {
            var query = dbContext.Threads
                .AsNoTracking()
                .Where(t => t.Id == threadId)
                .SelectMany(t => t.Comments)
                    .Where(c => c.ParentDepth == 0 && !c.IsDeleted && !c.IsBaned);

            List<CommentViewModel> comments = [];
            switch (sortBy)
            {
                case SortByEnum.CreateAt:
                    comments = await query.OrderByDescending(c => c.CreatedAt)
                        .Take(_commentsToCache)
                        .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                        .ToListAsync(cancellatinToken);
                    break;
                case SortByEnum.UserName:
                    comments = await query.OrderByDescending(c => c.User.UserName)
                        .Take(_commentsToCache)
                        .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                        .ToListAsync(cancellatinToken);
                    break;
                case SortByEnum.Email:
                    comments = await query.OrderByDescending(c => c.User.Email)
                        .Take(_commentsToCache)
                        .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                        .ToListAsync(cancellatinToken);
                    break;
                default:
                    comments = await query.OrderByDescending(c => c.CreatedAt)
                        .Take(_commentsToCache)
                        .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
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

                if (batchComment) batch.StringSetAsync($"comment:{comment.Id}", json, TimeSpan.FromHours(1));
                switch (sortBy)
                {
                    case SortByEnum.CreateAt:
                        batch.SortedSetAddAsync(key, comment.Id.ToString(), comment.CreatedAt.Ticks);
                        break;
                    case SortByEnum.UserName:
                        batch.SortedSetAddAsync(key, comment.Id.ToString(), GetNameScore(comment.UserName));
                        break;
                    case SortByEnum.Email:
                        batch.SortedSetAddAsync(key, comment.Id.ToString(), GetNameScore(comment.Email));
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
            var score = BitConverter.ToDouble(bytes, 0);
            return score;
        }
    }
}
