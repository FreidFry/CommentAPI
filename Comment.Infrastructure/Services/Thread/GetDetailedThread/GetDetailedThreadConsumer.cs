using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Thread.GetDetailedThread
{
    public class GetDetailedThreadConsumer : IConsumer<ThreadDetaliRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public GetDetailedThreadConsumer(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<ThreadDetaliRequestDTO> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;

            var query = _appDbContext.Threads
                .Where(t => t.Id == dto.ThreadId && (t.OwnerId == dto.callerId || !t.IsDeleted));

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
