using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Comment.Infrastructure.Services.Thread.DTOs.Response;
using Comment.Infrastructure.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Thread
{
    public class ThreadService : IThreadService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<ThreadCreateDTO> _createValidator;
        private readonly IValidator<ThreadUpdateDTO> _updateValidator;
        private readonly IValidator<ThreadFindDTO> _findValidator;
        private readonly IHtmlSanitize _htmlSanitizer;

        public ThreadService(
            AppDbContext context,
            IMapper mapper,
            IValidator<ThreadCreateDTO> createValidator,
            IValidator<ThreadUpdateDTO> updateValidator,
            IValidator<ThreadFindDTO> findValidator,
            IHtmlSanitize htmlSanitizer)
        {
            _appDbContext = context;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _findValidator = findValidator;
            _htmlSanitizer = htmlSanitizer;
        }

        public async Task<IActionResult> GetThreadsThreeAsync(ThreadsThreeDTO dto, CancellationToken cancellationToken)
        {

            var query = _appDbContext.Threads
                .Where(t => !t.IsDeleted && !t.IsBanned && !t.OwnerUser.IsDeleted && !t.OwnerUser.IsBanned)
                .OrderByDescending(t => t.CreatedAt);

            if (dto.After.HasValue)
                query = (IOrderedQueryable<ThreadModel>)query.Where(t => t.CreatedAt < dto.After);

            switch (dto.Limit)
            {
                case <= 0:
                    dto = dto with { Limit = 10 };
                    break;
                case > 50:
                    dto = dto with { Limit = 50 };
                    break;
            }

            var ThreadsThree = await query
                .Take(dto.Limit)
                .ProjectTo<ThreadsThreeResponce>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            bool HasMore = await query
                .Skip(dto.Limit)
                .AnyAsync(cancellationToken);

            DateTime? nextCursor = ThreadsThree.LastOrDefault()?.CreatedAt;

            return new OkObjectResult(new { items = ThreadsThree, nextCursor, HasMore });
        }

        public async Task<IActionResult> GetByIdAsync(ThreadFindDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _findValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var callerId = GetCallerId(httpContext);

            var query = _appDbContext.Threads
                .Where(t => t.Id == dto.ThreadId && (t.OwnerId == callerId || !t.IsDeleted));

            var thread = await query
                .ProjectTo<DetailedThreadResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (thread == null)
                return new NotFoundObjectResult("Thread not found");

            return new OkObjectResult(thread);
        }

        public async Task<IActionResult> CreateAsync(ThreadCreateDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var callerId = GetCallerId(httpContext);
            if (callerId == null)
                return new UnauthorizedResult();

            var user = await _appDbContext.Users.FindAsync([callerId], cancellationToken);
            if (user == null || user.IsDeleted || user.IsBanned)
                return new NotFoundObjectResult("User not found or banned");

            var thread = new ThreadModel(_htmlSanitizer.Sanitize(dto.Title), _htmlSanitizer.Sanitize(dto.Context), user);

            await _appDbContext.Threads.AddAsync(thread, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var threadDto = await _appDbContext.Threads
                .Where(t => t.Id == thread.Id)
                .Include(t => t.OwnerUser)
                .Select(t => new DetailedThreadResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Context = t.Context,
                    OwnerId = t.OwnerId,
                    OwnerUserName = t.OwnerUser.UserName,
                    CreatedAt = t.CreatedAt,
                    LastUpdatedAt = t.LastUpdatedAt,
                    CommentCount = t.Comments.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new RedirectResult($"/threads/{thread.Id}");
        }

        public async Task<IActionResult> UpdateAsync(ThreadUpdateDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var callerId = GetCallerId(httpContext);
            if (callerId == null)
                return new UnauthorizedResult();

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted, cancellationToken);

            if (thread == null)
                return new NotFoundObjectResult("Thread not found");

            if (thread.OwnerId != callerId)
                return new ForbidResult();

            // Update thread properties using EF Core's property access for private setters
            _appDbContext.Entry(thread).Property("Title").CurrentValue = _htmlSanitizer.Sanitize(dto.Title);
            _appDbContext.Entry(thread).Property("Context").CurrentValue = _htmlSanitizer.Sanitize(dto.Context);
            thread.LastUpdatedAt = DateTime.UtcNow;

            _appDbContext.Threads.Update(thread);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var threadDto = await _appDbContext.Threads
                .Where(t => t.Id == thread.Id)
                .Include(t => t.OwnerUser)
                .Select(t => new DetailedThreadResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Context = t.Context,
                    OwnerId = t.OwnerId,
                    OwnerUserName = t.OwnerUser.UserName,
                    CreatedAt = t.CreatedAt,
                    LastUpdatedAt = t.LastUpdatedAt,
                    CommentCount = t.Comments.Count(c => !c.IsDeleted)
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new OkObjectResult(threadDto);
        }

        public async Task<IActionResult> DeleteAsync(Guid id, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var callerId = GetCallerId(httpContext);
            if (callerId == null)
                return new UnauthorizedResult();

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

            if (thread == null)
                return new NotFoundObjectResult("Thread not found");

            if (thread.OwnerId != callerId)
                return new ForbidResult();

            // Use EF Core's property access for private setters
            _appDbContext.Entry(thread).Property("IsDeleted").CurrentValue = true;
            thread.LastUpdatedAt = DateTime.UtcNow;

            _appDbContext.Threads.Update(thread);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            return new NoContentResult();
        }

        public async Task<IActionResult> RestoreAsync(Guid id, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var callerId = GetCallerId(httpContext);
            if (callerId == null)
                return new UnauthorizedResult();

            var thread = await _appDbContext.Threads.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            if (thread == null)
                return new NotFoundResult();
            if (thread.OwnerId != callerId)
                return new ForbidResult();

            if (!thread.IsDeleted)
                return new OkResult();
            _appDbContext.Entry(thread).Property("IsDeleted").CurrentValue = false;
            thread.LastUpdatedAt = DateTime.UtcNow;
            _appDbContext.Threads.Update(thread);
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return new OkResult();
        }
    }
}
