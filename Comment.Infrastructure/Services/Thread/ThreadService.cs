using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Comment.Infrastructure.Services.Thread.DTOs.Response;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Thread
{
    public class ThreadService : IThreadService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<ThreadCreateDTO> _createValidator;
        private readonly IValidator<ThreadUpdateDTO> _updateValidator;
        private readonly IValidator<ThreadFindDTO> _findValidator;

        public ThreadService(
            AppDbContext context,
            IMapper mapper,
            IValidator<ThreadCreateDTO> createValidator,
            IValidator<ThreadUpdateDTO> updateValidator,
            IValidator<ThreadFindDTO> findValidator)
        {
            _appDbContext = context;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _findValidator = findValidator;
        }

        public async Task<IActionResult> GetThreadsThreeAsync(ThreadsThreeDTO dto, CancellationToken cancellationToken)
        {
            var query = _appDbContext.Threads
                .Where(t => !t.IsDeleted && !t.IsBanned)
                .OrderByDescending(t => t.CreatedAt);

            if (dto.After.HasValue)
                query = (IOrderedQueryable<ThreadModel>)query.Where(t => t.CreatedAt < dto.After);

            var ThreadsThree = await query
                .Take(dto.Limit)
                .Select(t => new ThreadsThreeDTOResponce(
                    t.Id,
                    t.Title,
                    t.Context,
                    t.CreatedAt,
                    t.Comments.Count(c => !c.IsDeleted && !c.IsBaned)
                ))
                .ToListAsync(cancellationToken);

            DateTime? nextCursor = ThreadsThree.LastOrDefault()?.CreatedAt;

            return new OkObjectResult(new { items = ThreadsThree, nextCursor });
        }

        public async Task<IActionResult> GetByIdAsync(ThreadFindDTO dto, CancellationToken cancellationToken)
        {
            var validationResult = await _findValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var thread = await _appDbContext.Threads
                .Where(t => t.Id == dto.ThreadId && !t.IsDeleted)
                .Include(t => t.OwnerUser)
                .Select(t => new ThreadResponseDTO
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

            if (thread == null)
                return new NotFoundObjectResult("Thread not found");

            return new OkObjectResult(thread);
        }

        public async Task<IActionResult> GetByIdWithCommentsAsync(ThreadFindDTO dto, CancellationToken cancellationToken)
        {
            var validationResult = await _findValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var thread = await _appDbContext.Threads
                .Where(t => t.Id == dto.ThreadId && !t.IsDeleted)
                .Include(t => t.OwnerUser)
                .Include(t => t.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(cancellationToken);

            if (thread == null)
                return new NotFoundObjectResult("Thread not found");

            var comments = thread.Comments
                .Select(c => new CommentResponseDTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ThreadId = c.ThreadId,
                    ParentCommentId = c.ParentCommentId,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    AvatarTumbnailUrl = c.User.AvatarTumbnailUrl
                })
                .ToList();

            var commentTree = BuildCommentTree(comments);

            var threadDto = new ThreadWithCommentsDTO
            {
                Id = thread.Id,
                Title = thread.Title,
                Context = thread.Context,
                OwnerId = thread.OwnerId,
                OwnerUserName = thread.OwnerUser.UserName,
                CreatedAt = thread.CreatedAt,
                LastUpdatedAt = thread.LastUpdatedAt,
                CommentCount = comments.Count,
                Comments = commentTree
            };

            return new OkObjectResult(threadDto);
        }

        public async Task<IActionResult> CreateAsync(ThreadCreateDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var userIdClaim = httpContext.User.FindFirst("uid");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return new UnauthorizedResult();

            var user = await _appDbContext.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null || user.IsDeleted || user.IsBanned)
                return new NotFoundObjectResult("User not found or banned");

            var thread = new ThreadModel(dto.Title, dto.Context, user);

            await _appDbContext.Threads.AddAsync(thread, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var threadDto = await _appDbContext.Threads
                .Where(t => t.Id == thread.Id)
                .Include(t => t.OwnerUser)
                .Select(t => new ThreadResponseDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Context = t.Context,
                    OwnerId = t.OwnerId,
                    OwnerUserName = t.OwnerUser.UserName,
                    CreatedAt = t.CreatedAt,
                    LastUpdatedAt = t.LastUpdatedAt,
                    CommentCount = 0
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new OkResult();
        }

        public async Task<IActionResult> UpdateAsync(ThreadUpdateDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var userIdClaim = httpContext.User.FindFirst("uid");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return new UnauthorizedResult();

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted, cancellationToken);

            if (thread == null)
                return new NotFoundObjectResult("Thread not found");

            if (thread.OwnerId != userId)
                return new ForbidResult();

            // Update thread properties using EF Core's property access for private setters
            _appDbContext.Entry(thread).Property("Title").CurrentValue = dto.Title;
            _appDbContext.Entry(thread).Property("Context").CurrentValue = dto.Context;
            thread.LastUpdatedAt = DateTime.UtcNow;
            
            _appDbContext.Threads.Update(thread);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var threadDto = await _appDbContext.Threads
                .Where(t => t.Id == thread.Id)
                .Include(t => t.OwnerUser)
                .Select(t => new ThreadResponseDTO
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

        public async Task<IActionResult> DeleteAsync(ThreadFindDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _findValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var userIdClaim = httpContext.User.FindFirst("uid");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return new UnauthorizedResult();

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted, cancellationToken);

            if (thread == null)
                return new NotFoundObjectResult("Thread not found");

            if (thread.OwnerId != userId)
                return new ForbidResult();

            // Use EF Core's property access for private setters
            _appDbContext.Entry(thread).Property("IsDeleted").CurrentValue = true;
            thread.LastUpdatedAt = DateTime.UtcNow;
            
            _appDbContext.Threads.Update(thread);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            return new NoContentResult();
        }

        private List<CommentTreeDTO> BuildCommentTree(List<CommentResponseDTO> comments)
        {
            var commentDict = comments.ToDictionary(c => c.Id, c => new CommentTreeDTO
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ThreadId = c.ThreadId,
                ParentCommentId = c.ParentCommentId,
                UserId = c.UserId,
                UserName = c.UserName,
                AvatarTumbnailUrl = c.AvatarTumbnailUrl,
                Replies = new List<CommentTreeDTO>()
            });

            var rootComments = new List<CommentTreeDTO>();

            foreach (var comment in commentDict.Values)
            {
                if (comment.ParentCommentId.HasValue && commentDict.ContainsKey(comment.ParentCommentId.Value))
                    commentDict[comment.ParentCommentId.Value].Replies.Add(comment);
                else
                    rootComments.Add(comment);
            }

            return rootComments;
        }
    }
}
