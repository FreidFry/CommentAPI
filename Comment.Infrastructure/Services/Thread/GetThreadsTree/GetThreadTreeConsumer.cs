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

namespace Comment.Infrastructure.Services.Thread.GetThreadsTree
{
    public class GetThreadTreeConsumer : IConsumer<ThreadsThreeRequest>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public GetThreadTreeConsumer(AppDbContext context, IMapper mapper)
        {
            _appDbContext = context;
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<ThreadsThreeRequest> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;
            var query = _appDbContext.Threads
    .Where(t => !t.IsDeleted && !t.IsBanned && !t.OwnerUser.IsDeleted && !t.OwnerUser.IsBanned)
    .OrderByDescending(t => t.CreatedAt);

            if (dto.After.HasValue)
                query = (IOrderedQueryable<ThreadModel>)query.Where(t => t.CreatedAt < dto.After);

            switch (dto.Limit)
            {
                case <= 0:
                    dto = dto with { Limit = 10 };
                    break;
                case > 50:
                    dto = dto with { Limit = 50 };
                    break;
            }

            var ThreadsThree = await query
                .Take(dto.Limit)
                .ProjectTo<ThreadsResponseViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            bool HasMore = await query
                .Skip(dto.Limit)
                .AnyAsync(cancellationToken);

            DateTime? nextCursor = ThreadsThree.LastOrDefault()?.CreatedAt;

            await context.RespondAsync(new ThreadsTreeResponse(ThreadsThree, nextCursor, HasMore));
        }
    }
}
