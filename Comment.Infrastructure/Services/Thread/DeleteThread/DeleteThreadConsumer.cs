using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Thread.DeleteThread.Request;
using FluentValidation;

using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Comment.Infrastructure.Services.Thread.DeleteThread
{
    public class DeleteThreadConsumer : IConsumer<DeleteThreadRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<DeleteThreadRequestDTO> _validator;
        private readonly IDatabase _redisDb;
        public DeleteThreadConsumer(AppDbContext appDbContext, IValidator<DeleteThreadRequestDTO> validator, IConnectionMultiplexer connectionMultiplexer)
        {
            _appDbContext = appDbContext;
            _validator = validator;
            _redisDb = connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// Processes a request to soft-delete an entire discussion thread.
        /// Validates the request, verifies ownership, and updates the thread's deletion status in the database.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="DeleteThreadRequestDTO"/>.</param>
        /// <returns>
        /// A <see cref="StatusCodeResponse"/> with 204 (No Content) on success, 
        /// 404 (Not Found) if the thread is missing, or 403 (Forbidden) if the requester is not the owner.
        /// </returns>
        /// <remarks>
        /// Logic and Safety:
        /// <list type="bullet">
        /// <item>
        /// <term>Validation:</term>
        /// <description>Ensures the request DTO adheres to schema rules before hitting the database.</description>
        /// </item>
        /// <item>
        /// <term>Authorization:</term>
        /// <description>Strictly compares <c>OwnerId</c> with <c>CallerId</c> to prevent unauthorized deletions.</description>
        /// </item>
        /// <item>
        /// <term>Soft Delete Implementation:</term>
        /// <description>Uses shadow property access or explicit flag updates to mark <c>IsDeleted</c> as true without physical row removal.</description>
        /// </item>
        /// <item>
        /// <term>Audit Trail:</term>
        /// <description>Updates <c>LastUpdatedAt</c> to reflect the time of the deletion event.</description>
        /// </item>
        /// </list>
        /// </remarks>
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

            var key = $"thread:{thread.Id}:preview";
            var detailedThreadKey = $"thread:{thread.Id}:details";


            _appDbContext.Threads.Update(thread);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var bath = _redisDb.CreateBatch();
            bath.KeyDeleteAsync(key);
            bath.KeyDeleteAsync(detailedThreadKey);
            bath.Execute();

            await context.RespondAsync(new StatusCodeResponse("Delete success.", 204));
        }
    }
}
