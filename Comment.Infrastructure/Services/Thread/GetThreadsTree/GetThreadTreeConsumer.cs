using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Thread.DTOs;
using Comment.Infrastructure.Services.Thread.GetThreadsTree.Request;
using Comment.Infrastructure.Services.Thread.GetThreadsTree.Response;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace Comment.Infrastructure.Services.Thread.GetThreadsTree
{
    public class GetThreadTreeConsumer : IConsumer<ThreadsThreeRequest>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IDatabase _dataBase;

        public GetThreadTreeConsumer(AppDbContext context, IMapper mapper, IConnectionMultiplexer connectionMultiplexer)
        {
            _appDbContext = context;
            _mapper = mapper;
            _dataBase = connectionMultiplexer.GetDatabase();
        }

        public async Task Consume(ConsumeContext<ThreadsThreeRequest> context)
        {
            var request = context.Message;
            var cancellationToken = context.CancellationToken;

            var paginatedLimit = request.Limit + 1;

            List<ThreadsResponseViewModel> finalThreads = [];

            var cachedIds = await GetIdsFromRedis("all_active_previews", _dataBase, request.After, paginatedLimit, cancellationToken);

            if (cachedIds.Length > 0)
            {
                var tasks = cachedIds.Select(id => _dataBase.StringGetAsync($"thread:{id}:preview"));
                var results = await Task.WhenAll(tasks);
                foreach (var res in results.Where(r => r.HasValue))
                {
                    var comment = JsonSerializer.Deserialize<ThreadsResponseViewModel>(res!.ToString());
                    finalThreads.Add(_mapper.Map<ThreadsResponseViewModel>(comment));
                }
            }

            if (finalThreads.Count < paginatedLimit)
            {
                int needed = paginatedLimit - finalThreads.Count;

                DateTime? effectiveAfter = finalThreads.Count > 0
                    ? finalThreads.Last().CreatedAt : request.After;

                var dbThreads = await GetFromDb(effectiveAfter, needed, context.CancellationToken);

                finalThreads.AddRange(dbThreads);
            }

            bool HasMore = false;
            if (finalThreads.Count > request.Limit)
            {
                HasMore = true;
                finalThreads.RemoveAt(request.Limit);
            }

            DateTime? nextCursor = null;
            if (HasMore)
            {
                var last = finalThreads.Last();
                nextCursor = last.CreatedAt;
            }

            await context.RespondAsync(new ThreadsTreeResponse(finalThreads, nextCursor, HasMore));
        }


        private async Task<List<ThreadsResponseViewModel>> GetFromDb(DateTime? after, int need, CancellationToken cancellationToken)
        {
            var query = _appDbContext.Threads
                .Where(t => !t.IsDeleted && !t.IsBanned && !t.OwnerUser.IsDeleted && !t.OwnerUser.IsBanned)
                .OrderByDescending(t => t.CreatedAt);

            if (after.HasValue)
                query = (IOrderedQueryable<ThreadModel>)query.Where(t => t.CreatedAt < after);

            var ThreadsThree = await query
                .Take(need)
                .ProjectTo<ThreadsResponseViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return ThreadsThree;
        }

        private static async Task<RedisValue[]> GetIdsFromRedis(string keys, IDatabase db, DateTime? after, int limit, CancellationToken cancellationToken)
        {
            return await db.SortedSetRangeByScoreAsync(keys, double.NegativeInfinity, after?.ToUniversalTime().Ticks ?? double.PositiveInfinity, Exclude.Both, Order.Descending, 0, limit);
        }
    }
}
