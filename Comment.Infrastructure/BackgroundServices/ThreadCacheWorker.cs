using Amazon.Runtime.Internal.Util;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.Services.Thread.DTOs;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Data;
using System.Diagnostics;
using System.Text.Json;

namespace Comment.Infrastructure.BackgroundServices
{
    public class ThreadCacheWorker : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceProvider _services;
        private readonly IMapper _mapper;
        private readonly int _threadsToCache = 100;
        private readonly string _allPreviewsKeys = "all_active_previews";
        private readonly ILogger<ThreadCacheWorker> _logger;


        public ThreadCacheWorker(IConnectionMultiplexer redis, IServiceProvider services, IMapper mapper, ILogger<ThreadCacheWorker> logger)
        {
            _redis = redis;
            _services = services;
            _mapper = mapper;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ThreadCacheWorker started (Interval: 25 min)");

            while (!stoppingToken.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var redisDb = _redis.GetDatabase();

                        var lastThreads = await dbContext.Threads
                            .AsNoTracking()
                            .Where(t => !t.IsBanned && !t.IsDeleted && !t.OwnerUser.IsDeleted && !t.OwnerUser.IsBanned)
                            .OrderByDescending(t => t.CreatedAt)
                            .Take(100)
                            .ProjectTo<DetailedThreadResponse>(_mapper.ConfigurationProvider)
                            .ToListAsync(stoppingToken);

                        await WarmUpThreadCache(lastThreads, dbContext, redisDb, stoppingToken);
                        await WarmUpThreadPreviewCache(lastThreads, dbContext, redisDb, stoppingToken);
                        await redisDb.SortedSetRemoveRangeByRankAsync(_allPreviewsKeys, 0, -(_threadsToCache + 1));

                        sw.Stop();
                        if (!lastThreads.Any())
                            _logger.LogWarning("No active threads found to cache.");
                        else
                            _logger.LogInformation("Cache warmup successful: {Count} threads processed in {Elapsed}ms",
                                lastThreads.Count, sw.ElapsedMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError(ex, "Critical error during thread cache warmup after {Elapsed}ms", sw.ElapsedMilliseconds);
                }

                await Task.Delay(TimeSpan.FromMinutes(25), stoppingToken);
            }
        }

        private async Task WarmUpThreadCache(List<DetailedThreadResponse> threads, AppDbContext dbContext, IDatabase redisDb, CancellationToken cancellatinToken)
        {
            if (!threads.Any()) return;

            var batch = redisDb.CreateBatch();
            int count = 0;
            foreach (var thread in threads)
            {
                try
                {
                    var key = $"thread:{thread.Id}:details";
                    string json = JsonSerializer.Serialize(thread, new JsonSerializerOptions
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        PropertyNameCaseInsensitive = true
                    });
                    batch.StringSetAsync(key, json, TimeSpan.FromHours(1));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to map/serialize details for thread {ThreadId}", thread.Id);
                }
            }
            batch.Execute();
            _logger.LogDebug("Batch execution sent for {Count} thread details", count);
        }


        private async Task WarmUpThreadPreviewCache(List<DetailedThreadResponse> threads, AppDbContext dbContext, IDatabase redisDb, CancellationToken cancellatinToken)
        {
            if (!threads.Any()) return;

            var batch = redisDb.CreateBatch();
            int count = 0;
            foreach (var thread in threads)
            {
                try
                {
                    var threadPreview = _mapper.Map<ThreadsResponseViewModel>(thread);
                var key = $"thread:{threadPreview.Id}:preview";

                string json = JsonSerializer.Serialize(threadPreview, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true
                });
                batch.StringSetAsync(key, json, TimeSpan.FromHours(1));
                batch.SortedSetAddAsync(_allPreviewsKeys, thread.Id.ToString(), thread.CreatedAt.Ticks);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to map/serialize details for thread {ThreadId}", thread.Id);
                }
            }
            batch.Execute();
            _logger.LogDebug("Batch execution sent for {Count} thread previews", count);
        }
    }
}

