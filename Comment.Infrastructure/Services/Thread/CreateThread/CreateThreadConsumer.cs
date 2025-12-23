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



