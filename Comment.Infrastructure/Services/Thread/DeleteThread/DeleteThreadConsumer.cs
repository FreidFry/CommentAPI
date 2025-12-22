using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Thread.DeleteThread.Request;
using FluentValidation;

using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Thread.DeleteThread
{
    public class DeleteThreadConsumer : IConsumer<DeleteThreadRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<DeleteThreadRequestDTO> _validator;
        public DeleteThreadConsumer(AppDbContext appDbContext, IValidator<DeleteThreadRequestDTO> validator)
        {
            _appDbContext = appDbContext;
            _validator = validator;
        }

        public async Task Consume(ConsumeContext<DeleteThreadRequestDTO> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;

            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                await context.RespondAsync(new ValidatorErrorResponse(validationResult.Errors));
                return;
            }

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted, cancellationToken);

            if (thread == null)
            {
                await context.RespondAsync(new StatusCodeResponse("Thread not found.", 404));
                return;
            }
            if (thread.OwnerId != dto.CallerId)
            {
                await context.RespondAsync(new StatusCodeResponse("Forbind.", 403));
                return;
            }
            _appDbContext.Entry(thread).Property("IsDeleted").CurrentValue = true;
            thread.LastUpdatedAt = DateTime.UtcNow;

            _appDbContext.Threads.Update(thread);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            await context.RespondAsync(new StatusCodeResponse("Delete success.", 204));
        }
    }
}
