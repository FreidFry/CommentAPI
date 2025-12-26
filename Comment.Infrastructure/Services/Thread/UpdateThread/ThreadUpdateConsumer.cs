using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Interfaces;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using Comment.Infrastructure.Services.Thread.UpdateThread.Request;
using Comment.Infrastructure.Services.Thread.UpdateThread.Response;
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

        /// <summary>
        /// Processes a request to update an existing thread's title and content.
        /// Validates ownership, cleanses input to prevent XSS, and persists changes to the database.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="UpdateThreadRequestDTO"/>.</param>
        /// <returns>
        /// An <see cref="UpdateThreadSucces"/> response on success, 
        /// or error codes (404 for missing threads, 403 for unauthorized access).
        /// </returns>
        /// <remarks>
        /// Implementation Logic:
        /// <list type="bullet">
        /// <item>
        /// <term>Validation:</term>
        /// <description>Uses a specific <c>UpdateValidator</c> to ensure the new data meets length and format requirements.</description>
        /// </item>
        /// <item>
        /// <term>Security Sanitization:</term>
        /// <description>Applies <see cref="IHtmlSanitizer"/> to both Title and Context to ensure no malicious scripts are injected.</description>
        /// </item>
        /// <item>
        /// <term>Optimistic Concurrency:</term>
        /// <description>Updates <c>LastUpdatedAt</c> to maintain an accurate audit trail of changes.</description>
        /// </item>
        /// <item>
        /// <term>Shadow Property Access:</term>
        /// <description>Directly manipulates properties via <c>Entry().Property()</c> to ensure precise change tracking.</description>
        /// </item>
        /// </list>
        /// </remarks>
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
