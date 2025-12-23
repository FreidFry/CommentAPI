using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.Services.Thread.DTOs;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Data;
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



        public ThreadCacheWorker(IConnectionMultiplexer redis, IServiceProvider services, IMapper mapper)
        {
            _redis = redis;
            _services = services;
            _mapper = mapper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
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
                }

                await Task.Delay(TimeSpan.FromMinutes(25), stoppingToken);
            }
        }

        private async Task WarmUpThreadCache(List<DetailedThreadResponse> threads, AppDbContext dbContext, IDatabase redisDb, CancellationToken cancellatinToken)
        {
            if (!threads.Any()) return;

            var batch = redisDb.CreateBatch();
            foreach (var thread in threads)
            {
                var key = $"thread:{thread.Id}:details";
                string json = JsonSerializer.Serialize(thread, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNameCaseInsensitive = true
                });
                batch.StringSetAsync(key, json, TimeSpan.FromHours(1));
            }
            batch.Execute();
        }


        private async Task WarmUpThreadPreviewCache(List<DetailedThreadResponse> threads, AppDbContext dbContext, IDatabase redisDb, CancellationToken cancellatinToken)
        {
            if (!threads.Any()) return;

            var batch = redisDb.CreateBatch();
            foreach (var thread in threads)
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
            batch.Execute();
        }
    }
}

