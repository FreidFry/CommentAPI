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

        /// <summary>
        /// Processes a request to restore a previously soft-deleted thread.
        /// Validates ownership and toggles the deletion flag back to an active state.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="RestoreThreadRequestDTO"/>.</param>
        /// <returns>
        /// A <see cref="StatusCodeResponse"/> with 204 (No Content) on success or if already active,
        /// 404 (Not Found) if the thread is missing, or 403 (Forbidden) if the requester is not the owner.
        /// </returns>
        /// <remarks>
        /// Restoration Workflow:
        /// <list type="bullet">
        /// <item>
        /// <term>Ownership Verification:</term>
        /// <description>Ensures that only the <c>OwnerId</c> associated with the thread can perform restoration.</description>
        /// </item>
        /// <item>
        /// <term>State Validation:</term>
        /// <description>Checks if the thread is actually deleted. If it's already active, returns 204 to maintain idempotency.</description>
        /// </item>
        /// <item>
        /// <term>Flag Reset:</term>
        /// <description>Reverts the <c>IsDeleted</c> shadow property to <c>false</c> and updates the <c>LastUpdatedAt</c> timestamp.</description>
        /// </item>
        /// </list>
        /// </remarks>
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
