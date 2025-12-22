using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Thread.RestoreThread
{

    public class RestoreThreadConsumer : IConsumer<RestoreThreadRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<RestoreThreadRequestDTO> _validator;

        public RestoreThreadConsumer(AppDbContext appDbContext, IValidator<RestoreThreadRequestDTO> validator)
        {
            _appDbContext = appDbContext;
            _validator = validator;
        }

        public async Task Consume(ConsumeContext<RestoreThreadRequestDTO> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;

            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                await context.RespondAsync(new ValidatorErrorResponse(validationResult.Errors));
                return;
            }

            var thread = await _appDbContext.Threads.FirstOrDefaultAsync(t => t.Id == dto.ThreadId, cancellationToken);
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

            if (!thread.IsDeleted)
            {
                await context.RespondAsync(new StatusCodeResponse("Thread not deleted.", 204));
                return;
            }
            _appDbContext.Entry(thread).Property("IsDeleted").CurrentValue = false;
            thread.LastUpdatedAt = DateTime.UtcNow;
            _appDbContext.Threads.Update(thread);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            await context.RespondAsync(new StatusCodeResponse("Thread restore.", 204));
        }
    }
}
