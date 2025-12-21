using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Comment.DeleteComment
{
    public class DeleteCommentConsumer : IConsumer<DeleteCommentRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<DeleteCommentRequestDTO> _validator;

        public DeleteCommentConsumer(AppDbContext appDbContext, IValidator<DeleteCommentRequestDTO> validator)
        {
            _appDbContext = appDbContext;
            _validator = validator;
        }

        public async Task Consume(ConsumeContext<DeleteCommentRequestDTO> context)
        {
            var cancellationToken = context.CancellationToken;
            var dto = context.Message;
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                await context.RespondAsync(new ValidatorErrorResponse(validationResult.Errors));
                return;
            }

            var comment = await _appDbContext.Comments
                .FirstOrDefaultAsync(c => c.Id == dto.CommentId && !c.IsDeleted, cancellationToken);

            if (comment == null)
            {
                await context.RespondAsync(new StatusCodeResponse("Comment not found or deleted.", 404));
                return;
            }

            if (comment.UserId != dto.CallerId)
            {
                await context.RespondAsync(new StatusCodeResponse("Unauthorized.", 403));
                return;
            }

            comment.MarkAsDeleted();
            _appDbContext.Comments.Update(comment);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            await context.RespondAsync(new StatusCodeResponse(string.Empty, 204));
        }
    }
}
