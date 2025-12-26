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

        /// <summary>
        /// Retrieves a paginated list of thread previews using a hybrid approach (Redis + SQL Fallback).
        /// Implements cursor-based pagination to provide a smooth "infinite scroll" experience.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="ThreadsThreeRequest"/> with limit and cursor.</param>
        /// <returns>
        /// A <see cref="ThreadsTreeResponse"/> containing the list of threads, the next timestamp cursor, and a "HasMore" flag.
        /// </returns>
        /// <remarks>
        /// Multi-layer Retrieval Strategy:
        /// <list type="number">
        /// <item>
        /// <term>Redis Index Look-up:</term>
        /// <description>Fetches thread IDs from a Sorted Set (<c>all_active_previews</c>) based on the provided timestamp cursor.</description>
        /// </item>
        /// <item>
        /// <term>Bulk Cache Fetch:</term>
        /// <description>Executes parallel <c>GET</c> operations to retrieve JSON previews for all discovered IDs.</description>
        /// </item>
        /// <item>
        /// <term>Database Fallback:</term>
        /// <description>If the cache contains fewer items than the requested limit, the gap is filled by querying the SQL database.</description>
        /// </item>
        /// <item>
        /// <term>Pagination Handling:</term>
        /// <description>Uses a "Limit + 1" approach to determine the <c>HasMore</c> status without extra count queries.</description>
        /// </item>
        /// </list>
        /// </remarks>
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
