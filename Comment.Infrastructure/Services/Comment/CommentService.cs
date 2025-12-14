using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Comment.Infrastructure.Services.Comment
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<CommentCreateDTO> _createValidator;
        private readonly IValidator<CommentUpdateDTO> _updateValidator;
        private readonly IValidator<CommentFindDTO> _findValidator;

        public CommentService(
            AppDbContext appDbContext,
            IMapper mapper,
            IValidator<CommentCreateDTO> createValidator,
            IValidator<CommentUpdateDTO> updateValidator,
            IValidator<CommentFindDTO> findValidator)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _findValidator = findValidator;
        }

        public async Task<IActionResult> GetByThreadAsync(CommentsByThreadDTO dto, CancellationToken cancellationToken)
        {
            var threadExists = await _appDbContext.Threads
                .AnyAsync(t => t.Id == dto.ThreadId && !t.IsDeleted, cancellationToken);

            if (!threadExists)
                return new NotFoundObjectResult("Thread not found");

            var query = _appDbContext.Comments
                .Where(c => c.ThreadId == dto.ThreadId && !c.IsDeleted)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt);

            if (dto.After.HasValue)
                query = (IOrderedQueryable<CommentModel>)query.Where(c => c.CreatedAt > dto.After);

            var comments = await query
                .Take(dto.Limit)
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
                .ToListAsync(cancellationToken);

            var commentTree = BuildCommentTree(comments);
            DateTime? nextCursor = comments.LastOrDefault()?.CreatedAt;

            return new OkObjectResult(new { items = commentTree, nextCursor });
        }

        public async Task<IActionResult> GetByIdAsync(CommentFindDTO dto, CancellationToken cancellationToken)
        {
            var validationResult = await _findValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var comment = await _appDbContext.Comments
                .Where(c => c.Id == dto.CommentId && !c.IsDeleted)
                .Include(c => c.User)
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
                .FirstOrDefaultAsync(cancellationToken);

            if (comment == null)
                return new NotFoundObjectResult("Comment not found");

            return new OkObjectResult(comment);
        }

        public async Task<IActionResult> CreateAsync(CommentCreateDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
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

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted, cancellationToken);

            if (thread == null)
                return new NotFoundObjectResult("Thread not found");

            if (dto.ParentCommentId.HasValue)
            {
                var parentComment = await _appDbContext.Comments
                    .FirstOrDefaultAsync(c => c.Id == dto.ParentCommentId.Value && 
                                             c.ThreadId == dto.ThreadId && 
                                             !c.IsDeleted, cancellationToken);

                if (parentComment == null)
                    return new BadRequestObjectResult("Parent comment not found or belongs to different thread");
            }

            var comment = new CommentModel(dto.Content, dto.ThreadId, user, dto.ParentCommentId);

            await _appDbContext.Comments.AddAsync(comment, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var commentDto = await _appDbContext.Comments
                .Where(c => c.Id == comment.Id)
                .Include(c => c.User)
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
                .FirstOrDefaultAsync(cancellationToken);

            return new CreatedAtActionResult(
                nameof(GetByIdAsync),
                "Comment",
                new { id = comment.Id },
                commentDto);
        }

        public async Task<IActionResult> UpdateAsync(CommentUpdateDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var userIdClaim = httpContext.User.FindFirst("uid");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return new UnauthorizedResult();

            var comment = await _appDbContext.Comments
                .FirstOrDefaultAsync(c => c.Id == dto.CommentId && !c.IsDeleted, cancellationToken);

            if (comment == null)
                return new NotFoundObjectResult("Comment not found");

            if (comment.UserId != userId)
                return new ForbidResult();

            comment.UpdateContent(dto.Content);
            _appDbContext.Comments.Update(comment);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var commentDto = await _appDbContext.Comments
                .Where(c => c.Id == comment.Id)
                .Include(c => c.User)
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
                .FirstOrDefaultAsync(cancellationToken);

            return new OkObjectResult(commentDto);
        }

        public async Task<IActionResult> DeleteAsync(CommentFindDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _findValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var userIdClaim = httpContext.User.FindFirst("uid");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return new UnauthorizedResult();

            var comment = await _appDbContext.Comments
                .FirstOrDefaultAsync(c => c.Id == dto.CommentId && !c.IsDeleted, cancellationToken);

            if (comment == null)
                return new NotFoundObjectResult("Comment not found");

            if (comment.UserId != userId)
                return new ForbidResult();

            comment.MarkAsDeleted();
            _appDbContext.Comments.Update(comment);
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
                {
                    commentDict[comment.ParentCommentId.Value].Replies.Add(comment);
                }
                else
                {
                    rootComments.Add(comment);
                }
            }

            return rootComments;
        }
    }
}

