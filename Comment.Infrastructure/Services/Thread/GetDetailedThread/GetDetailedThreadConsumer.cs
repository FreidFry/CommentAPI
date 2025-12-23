using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.Json;
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
