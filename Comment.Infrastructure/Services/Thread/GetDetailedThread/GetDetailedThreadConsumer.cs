using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Comment.Infrastructure.Services.Thread.GetDetailedThread
{
    public class GetDetailedThreadConsumer : IConsumer<ThreadDetaliRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IDatabase _database;

        public GetDetailedThreadConsumer(AppDbContext appDbContext, IMapper mapper, IConnectionMultiplexer multiplexer)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _database = multiplexer.GetDatabase();
        }

        /// <summary>
        /// Retrieves detailed information about a specific thread, prioritizing the Redis cache 
        /// and falling back to the database if necessary.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="ThreadDetaliRequestDTO"/>.</param>
        /// <returns>
        /// A <see cref="DetailedThreadResponse"/> containing thread metadata, or a <see cref="StatusCodeResponse"/> (404) if not found.
        /// </returns>
        /// <remarks>
        /// Data Retrieval Strategy:
        /// <list type="number">
        /// <item>
        /// <term>Cache-First:</term>
        /// <description>Attempts to fetch the thread JSON directly from Redis for sub-millisecond latency.</description>
        /// </item>
        /// <item>
        /// <term>Security-Aware Fallback:</term>
        /// <description>If cache misses, queries the database. Includes a special check allowing owners to view their own deleted threads.</description>
        /// </item>
        /// <item>
        /// <term>Efficient Mapping:</term>
        /// <description>Uses <c>ProjectTo</c> for optimal SQL generation during database fallback.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public async Task Consume(ConsumeContext<ThreadDetaliRequestDTO> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;

            var key = $"thread:{dto.ThreadId}:details";
            var json = await _database.StringGetAsync(key);
            if (json.HasValue) await context.RespondAsync(new JsonResponse(json.ToString()));

            var query = _appDbContext.Threads
                .Where(t => t.Id == dto.ThreadId && (t.OwnerId == dto.СallerId || !t.IsDeleted));

            var thread = await query
                .ProjectTo<DetailedThreadResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (thread == null)
            {
                await context.RespondAsync(new StatusCodeResponse("Thread not found.", 404));
                return;
            }

            await context.RespondAsync(thread);
        }
    }
}
