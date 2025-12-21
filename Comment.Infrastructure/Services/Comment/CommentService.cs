using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.Enums;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Comment.Infrastructure.Services.Comment.UpdateComment.Request;
using Comment.Infrastructure.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Comment
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        //private readonly IValidator<CommentCreateRequest> _createValidator;
        private readonly IValidator<CommentUpdateRequest> _updateValidator;
        private readonly IValidator<CommentFindDTO> _findValidator;
        private readonly IImageTransform _imageTransform;
        private readonly IHtmlSanitize _htmlSanitizer;
        private readonly IFileProvider _fileProvider;

        public CommentService(
            AppDbContext appDbContext,
            IMapper mapper,
            //IValidator<CommentCreateRequest> createValidator,
            IValidator<CommentUpdateRequest> updateValidator,
            IValidator<CommentFindDTO> findValidator,
            IImageTransform imageTransform,
            IHtmlSanitize htmlSanitizer,
            IFileProvider fileProvider)

        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            //_createValidator = createValidator;
            _updateValidator = updateValidator;
            _findValidator = findValidator;
            _imageTransform = imageTransform;
            _htmlSanitizer = htmlSanitizer;
            _fileProvider = fileProvider;
        }

        public async Task<IActionResult> GetByThreadAsync(Guid threadId, CommentsByThreadDTO dto, CancellationToken cancellationToken)
        {
            var query = _appDbContext.Threads
                .Where(t => t.Id == threadId && !t.OwnerUser.IsDeleted && !t.OwnerUser.IsBanned && !t.IsBanned && !t.IsDeleted);

            var rootList = query
               .SelectMany(t => t.Comments);

            rootList = dto.SortByEnum switch
            {
                SortByEnum.Email => dto.IsAscending
                    ? rootList.OrderBy(c => c.User.Email)
                    : rootList.OrderByDescending(c => c.User.Email),
                SortByEnum.UserName => dto.IsAscending
                    ? rootList.OrderBy(c => c.User.UserName)
                    : rootList.OrderByDescending(c => c.User.UserName),
                SortByEnum.CreateAt => dto.IsAscending
                    ? rootList.OrderBy(c => c.CreatedAt)
                    : rootList.OrderByDescending(c => c.CreatedAt),
                _ => dto.IsAscending
                    ? rootList.OrderBy(c => c.CreatedAt)
                    : rootList.OrderByDescending(c => c.CreatedAt)
            };

            rootList = rootList.Where(c => c.ParentDepth == 0 && !c.IsDeleted && !c.IsBaned);

            if (!string.IsNullOrEmpty(dto.After))
            {
                rootList = dto.SortByEnum switch
                {
                    SortByEnum.Email => dto.IsAscending
                        ? rootList.Where(c => string.Compare(c.User.Email, dto.After) > 0)
                        : rootList.Where(c => string.Compare(c.User.Email, dto.After) < 0),
                    SortByEnum.UserName => dto.IsAscending
                        ? rootList.Where(c => string.Compare(c.User.UserName, dto.After) > 0)
                        : rootList.Where(c => string.Compare(c.User.UserName, dto.After) < 0),
                    SortByEnum.CreateAt => dto.IsAscending
                        ? rootList.Where(c => c.CreatedAt > DateTime.Parse(dto.After).ToUniversalTime())
                        : rootList.Where(c => c.CreatedAt < DateTime.Parse(dto.After).ToUniversalTime()),
                    _ => dto.IsAscending
                        ? rootList.Where(c => c.CreatedAt > DateTime.Parse(dto.After).ToUniversalTime())
                        : rootList.Where(c => c.CreatedAt < DateTime.Parse(dto.After).ToUniversalTime())
                };
            }

            var comments = await rootList
                .Take(dto.Limit+1)
                .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            bool HasMore = false;
            if (comments.Count > dto.Limit)
            {
                HasMore = true;
                comments.RemoveAt(dto.Limit);
            }

            string? nextCursor = null;
            if (HasMore)
            {
                var last = comments.Last();
                nextCursor = dto.SortByEnum switch
                {
                    SortByEnum.Email => last.Email,
                    SortByEnum.UserName => last.UserName,
                    SortByEnum.CreateAt => last.CreatedAt.ToString("O"),
                    _ => last.CreatedAt.ToString("O")
                };
            }

            return new OkObjectResult(new { items = comments, nextCursor, HasMore });
        }

        public async Task<IActionResult> GetByIdAsync(CommentFindDTO dto, CancellationToken cancellationToken)
        {
            var validationResult = await _findValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var query = _appDbContext.Comments.Where(c => !c.IsDeleted && !c.User.IsBanned && !c.User.IsDeleted);

            if (dto.after.HasValue)
                query = _appDbContext.Comments.Where(c => c.CreatedAt > dto.after);

            var comment = await query
                .Where(c => c.Id == dto.CommentId)
                .OrderByDescending(c => c.CreatedAt)
                .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (comment == null)
                return new NotFoundObjectResult("Comment not found");
            return new OkObjectResult(comment);
        }

        public async Task<IActionResult> UpdateAsync(CommentUpdateRequest dto, HttpContext httpContext, CancellationToken cancellationToken)
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

            comment.UpdateContent(_htmlSanitizer.Sanitize(dto.Content));
            _appDbContext.Comments.Update(comment);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            var commentDto = await _appDbContext.Comments
                .Where(c => c.Id == comment.Id)
                .Include(c => c.User)
                .Select(c => new CommentViewModel
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

        public Task<IActionResult> CreateAsync(CommentCreateRequest dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

