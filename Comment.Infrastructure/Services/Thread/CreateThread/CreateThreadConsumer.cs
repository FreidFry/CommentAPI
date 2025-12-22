using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Interfaces;
using Comment.Infrastructure.Services.Thread.CreateThread.Request;
using Comment.Infrastructure.Services.Thread.CreateThread.Response;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Thread.CreateThread
{
    public class CreateThreadConsumer : IConsumer<ThreadCreateRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<ThreadCreateRequestDTO> _validator;
        private readonly IHtmlSanitize _htmlSanitizer;


        public CreateThreadConsumer(AppDbContext appDbContext, IMapper mapper, IValidator<ThreadCreateRequestDTO> validator, IHtmlSanitize htmlSanitizer)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _validator = validator;
            _htmlSanitizer = htmlSanitizer;
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

            await _appDbContext.Threads.AddAsync(thread, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var threadDto = await _appDbContext.Threads
                .Where(t => t.Id == thread.Id)
                .ProjectTo<DetailedThreadResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            await context.RespondAsync(new ThreadCreateSuccess(thread.Id));
        }
    }
}



