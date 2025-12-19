using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
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
        private readonly IValidator<CommentCreateDTO> _createValidator;
        private readonly IValidator<CommentUpdateDTO> _updateValidator;
        private readonly IValidator<CommentFindDTO> _findValidator;
        private readonly IImageTransform _imageTransform;
        private readonly IHtmlSanitize _htmlSanitizer;
        private readonly IFileProvider _fileProvider;

        public CommentService(
            AppDbContext appDbContext,
            IMapper mapper,
            IValidator<CommentCreateDTO> createValidator,
            IValidator<CommentUpdateDTO> updateValidator,
            IValidator<CommentFindDTO> findValidator,
            IImageTransform imageTransform,
            IHtmlSanitize htmlSanitizer,
            IFileProvider fileProvider)

        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _findValidator = findValidator;
            _imageTransform = imageTransform;
            _htmlSanitizer = htmlSanitizer;
            _fileProvider = fileProvider;
        }

        public async Task<IActionResult> GetByThreadAsync(CommentsByThreadDTO dto, CancellationToken cancellationToken)
        {
            var threadExists = await _appDbContext.Threads
                .AnyAsync(t => t.Id == dto.ThreadId && !t.IsDeleted, cancellationToken);

            if (!threadExists)
                return new NotFoundResult();

            var query = _appDbContext.Comments
                .Where(c => c.ThreadId == dto.ThreadId && !c.IsDeleted)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt);

            if (dto.After.HasValue)
                query = (IOrderedQueryable<CommentModel>)query.Where(c => c.CreatedAt > dto.After);

            var comments = await query
                .Where(c => c.ParentDepth == 0)
                .OrderByDescending(c => c.CreatedAt)
                .ProjectTo<CommentResponseDTO>(_mapper.ConfigurationProvider)
                .Take(dto.Limit > 0 ? dto.Limit : 10)
                .ToListAsync(cancellationToken);

            DateTime? nextCursor = comments.LastOrDefault()?.CreatedAt;
            bool HasMore = await query.Skip(dto.Limit).AnyAsync(cancellationToken);

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
                .ProjectTo<CommentResponseDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (comment == null)
                return new NotFoundObjectResult("Comment not found");

            return new OkObjectResult(comment);
        }

        public async Task<IActionResult> CreateAsync([FromForm] CommentCreateDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var callerId = GetCallerId(httpContext);
            if (callerId == null)
                return new UnauthorizedResult();

            var user = await _appDbContext.Users.FindAsync(new object[] { callerId }, cancellationToken);
            if (user == null || user.IsDeleted || user.IsBanned)
                return new NotFoundObjectResult("User not found or banned");

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted, cancellationToken);

            if (thread == null)
                return new NotFoundObjectResult("Thread not found");
            var comment = new CommentModel(_htmlSanitizer.Sanitize(dto.Content), user, thread);
            if (dto.ParentCommentId.HasValue)
            {
                var parentComment = await _appDbContext.Comments
                    .FirstOrDefaultAsync(c => c.Id == dto.ParentCommentId.Value &&
                                             c.ThreadId == dto.ThreadId &&
                                             !c.IsDeleted, cancellationToken);

                if (parentComment == null)
                    return new BadRequestObjectResult("Parent comment not found or belongs to different thread");

                comment.AddParent(parentComment);
            }
            switch (dto.FormFile?.ContentType)
            {
                case "image/jpeg":
                case "image/png":
                    (string imageUrl, string tumbnailUrl) = await _imageTransform.ProcessAndUploadImageAsync(dto.FormFile, cancellationToken);
                    comment.SetImageUrls(imageUrl, tumbnailUrl);
                    break;
                case "image/gif":
                    (string gifUrl, string tumbnailGif) = await _imageTransform.ProcessAndUploadGifAsync(dto.FormFile, cancellationToken);
                    comment.SetImageUrls(gifUrl, tumbnailGif);
                    break;
                case "text/plain":
                    if (dto.FormFile.Length > 100 * 1024) // 100 KB
                        return new BadRequestObjectResult("Text file size exceeds the limit of 100 KB");
                    var fileUrl = await _fileProvider.SaveFileAsync(dto.FormFile, cancellationToken);
                    comment.SetFileUrl(fileUrl);
                    break;
                default:
                    break;
            }

            await _appDbContext.Comments.AddAsync(comment, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            return new OkResult();
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

            comment.UpdateContent(_htmlSanitizer.Sanitize(dto.Content));
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
    }
}

