using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using Comment.Infrastructure.Services.Thread.UpdateThread.Request;
using Comment.Infrastructure.Services.Thread.UpdateThread.Response;
using Comment.Infrastructure.Utils;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Thread.UpdateThread
{
    public class ThreadUpdateConsumer : IConsumer<UpdateThreadRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<UpdateThreadRequestDTO> _updateValidator;
        private readonly IMapper _mapper;
        private readonly IHtmlSanitize _htmlSanitizer;

        public ThreadUpdateConsumer(AppDbContext context, IMapper mapper, IValidator<UpdateThreadRequestDTO> updateValidator, IHtmlSanitize htmlSanitizer)
        {
            _appDbContext = context;
            _mapper = mapper;
            _updateValidator = updateValidator;
            _htmlSanitizer = htmlSanitizer;
        }

        public async Task Consume(ConsumeContext<UpdateThreadRequestDTO> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;
            var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                await context.RespondAsync(new ValidatorErrorResponse(validationResult.Errors));
                return;
            }
            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted, cancellationToken);

            if (thread == null)
            {
                await context.RespondAsync(new StatusCodeResponse("Thread not found", 404));
                return;
            }
            if (thread.OwnerId != dto.CallerId)
            {
                await context.RespondAsync(new StatusCodeResponse("Forbind", 403));
                return;
            }
            var updatedTitle = _htmlSanitizer.Sanitize(dto.Title);
            var updatedContext = _htmlSanitizer.Sanitize(dto.Context);
            _appDbContext.Entry(thread).Property("Title").CurrentValue = updatedTitle;
            _appDbContext.Entry(thread).Property("Context").CurrentValue = updatedContext;
            thread.LastUpdatedAt = DateTime.UtcNow;

            _appDbContext.Threads.Update(thread);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var threadDto = await _appDbContext.Threads
                .Where(t => t.Id == thread.Id)
                .ProjectTo<DetailedThreadResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            await context.RespondAsync(new UpdateThreadSucces(updatedTitle, updatedContext));
        }
    }
}
