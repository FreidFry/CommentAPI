using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Interfaces;
using Comment.Infrastructure.Services.Thread.CreateThread.Request;
using Comment.Infrastructure.Services.Thread.CreateThread.Response;
using Comment.Infrastructure.Services.Thread.DTOs;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace Comment.Infrastructure.Services.Thread.CreateThread
{
    public class CreateThreadConsumer : IConsumer<ThreadCreateRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<ThreadCreateRequestDTO> _validator;
        private readonly IHtmlSanitize _htmlSanitizer;
        private readonly IDatabase _database;


        public CreateThreadConsumer(AppDbContext appDbContext, IMapper mapper, IValidator<ThreadCreateRequestDTO> validator, IHtmlSanitize htmlSanitizer, IConnectionMultiplexer multiplexer)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _validator = validator;
            _htmlSanitizer = htmlSanitizer;
            _database = multiplexer.GetDatabase();
        }

        /// <summary>
        /// Processes a request to create a new discussion thread. 
        /// Validates input, checks user status, and populates Redis caches for both detailed and preview views.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="ThreadCreateRequestDTO"/>.</param>
        /// <returns>
        /// A <see cref="ThreadCreateSuccess"/> with the new thread ID on success, 
        /// or error responses (404 for missing users, 403 for banned/deleted accounts, or validation errors).
        /// </returns>
        /// <remarks>
        /// Implementation details:
        /// <list type="number">
        /// <item>
        /// <term>Validation:</term>
        /// <description>Executes FluentValidation to ensure title and content meet business rules.</description>
        /// </item>
        /// <item>
        /// <term>Security:</term>
        /// <description>Sanitizes both Title and Context using <see cref="IHtmlSanitizer"/> to prevent XSS.</description>
        /// </item>
        /// <item>
        /// <term>Cache Priming:</term>
        /// <description>Immediately warms up Redis by storing the detailed thread view and a preview object.</description>
        /// </item>
        /// <item>
        /// <term>Indexing:</term>
        /// <description>Adds the thread ID to a Redis Sorted Set (<c>all_active_previews</c>) using <c>CreatedAt.Ticks</c> as the score for chronological sorting.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public async Task Consume(ConsumeContext<ThreadCreateRequestDTO> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                await context.RespondAsync(new ValidatorErrorResponse(validationResult.Errors));
                return;
            }

            var user = await _appDbContext.Users
                .Where(u => u.Id == dto.callerId)
                .FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                await context.RespondAsync(new StatusCodeResponse("User not found.", 404));
                return;
            }
            else if (user.IsBanned || user.IsDeleted)
            {
                await context.RespondAsync(new StatusCodeResponse("Forbind.", 403));
                return;
            }

            var thread = new ThreadModel(_htmlSanitizer.Sanitize(dto.Title), _htmlSanitizer.Sanitize(dto.Context), user);

            var threadDetail = _mapper.Map<DetailedThreadResponse>(thread);
            string detailJson = JsonSerializer.Serialize(threadDetail);
            var detailedThreadKey = $"thread:{threadDetail.Id}:details";

            await _database.StringSetAsync(detailedThreadKey, detailJson, TimeSpan.FromHours(1));

            await context.RespondAsync(new ThreadCreateSuccess(thread.Id));

            //send to redis detail

            var batch = _database.CreateBatch();
            batch.ListRightPushAsync("threads_queue", JsonSerializer.Serialize(thread));

            //send to redis preview
            var threadPreview = _mapper.Map<ThreadsResponseViewModel>(thread);
            var key = $"thread:{threadPreview.Id}:preview";
            string previewJson = JsonSerializer.Serialize(threadPreview);
            batch.StringSetAsync(key, detailJson, TimeSpan.FromHours(1));
            batch.SortedSetAddAsync("all_active_previews", thread.Id.ToString(), thread.CreatedAt.Ticks);

            batch.Execute();
        }
    }
}



