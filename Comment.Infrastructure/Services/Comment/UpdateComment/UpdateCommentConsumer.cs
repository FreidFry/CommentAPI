using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Comment.Infrastructure.Utils;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Comment.UpdateComment
{
    public class UpdateCommentConsumer : IConsumer<CommentUpdateRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHtmlSanitize _htmlSanitizer;
        public UpdateCommentConsumer(AppDbContext appDbContext, IHtmlSanitize htmlSanitize)
        {
            _appDbContext = appDbContext;
            _htmlSanitizer = htmlSanitize;
        }

        public async Task Consume(ConsumeContext<CommentUpdateRequestDTO> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;

            var comment = await _appDbContext.Comments
                .FirstOrDefaultAsync(c => c.Id == dto.CommentId && !c.IsDeleted, cancellationToken);

            if (comment == null)
            {
                await context.RespondAsync(new StatusCodeResponse("Comment not found", 404));
                return;
            }

            if (comment.UserId != dto.callerId)
            {
                await context.RespondAsync(new StatusCodeResponse("Forbid", 403));
                return;
            }

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

            await context.RespondAsync<CommentViewModel>(commentDto);
        }
    }
}
