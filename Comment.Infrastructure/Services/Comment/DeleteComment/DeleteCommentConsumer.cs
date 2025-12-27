using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Comment.Infrastructure.Services.Comment.DeleteComment
{
    public class DeleteCommentConsumer : IConsumer<DeleteCommentRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<DeleteCommentRequestDTO> _validator;
        private readonly IDatabase _RedisDb;

        public DeleteCommentConsumer(AppDbContext appDbContext, IValidator<DeleteCommentRequestDTO> validator, IConnectionMultiplexer connectionMultiplexer)
        {
            _appDbContext = appDbContext;
            _validator = validator;
            _RedisDb = connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// Processes a request to soft-delete a comment. 
        /// Validates request parameters, checks ownership, and updates the comment status in the database.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="DeleteCommentRequestDTO"/>.</param>
        /// <returns>
        /// A <see cref="StatusCodeResponse"/> with 204 (No Content) on success, 
        /// 404 (Not Found) if the comment doesn't exist, or 403 (Forbidden) if the caller is not the author.
        /// Returns <see cref="ValidatorErrorResponse"/> if the input data is malformed.
        /// </returns>
        /// <remarks>
        /// Security and Logic:
        /// <list type="bullet">
        /// <item>Uses FluentValidation to ensure the request DTO is structurally sound.</item>
        /// <item>Implements Ownership Enforcement: Only the user who created the comment can delete it.</item>
        /// <item>Performs Soft Delete: Invokes <c>MarkAsDeleted()</c> which usually toggles a boolean flag instead of removing the row.</item>
        /// <item>Supports idempotency: If the comment is already marked as deleted, it returns 404.</item>
        /// </list>
        /// </remarks>
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

            await _RedisDb.KeyDeleteAsync($"comment:{comment.Id}");

            comment.MarkAsDeleted();
            _appDbContext.Comments.Update(comment);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            await context.RespondAsync(new StatusCodeResponse(string.Empty, 204));
        }
    }
}
