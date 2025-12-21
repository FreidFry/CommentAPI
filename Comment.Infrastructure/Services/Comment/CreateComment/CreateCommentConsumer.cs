using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.CreateComment.Response;
using Comment.Infrastructure.Utils;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Comment.CreateComment
{
    public class CreateCommentConsumer : IConsumer<CommentCreateRequestDTO>
    {
        private readonly IValidator<CommentCreateRequestDTO> _createValidator;
        private readonly AppDbContext _appDbContext;
        private readonly IImageTransform _imageTransform;
        private readonly IHtmlSanitize _htmlSanitizer;
        private readonly IFileProvider _fileProvider;

        public CreateCommentConsumer()
        {
            
        }

        public async Task Consume(ConsumeContext<CommentCreateRequestDTO> context)
        {
            var dto = context.Message;
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                await context.RespondAsync(new ValidatorErrorResponse(validationResult.Errors));

            

            var user = await _appDbContext.Users.FindAsync(new object[] { dto.CallerId });
            if (user == null || user.IsDeleted || user.IsBanned)
                await context.RespondAsync( new MessageResponse("User not found or banned"));

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted);

            if (thread == null)
                await context.RespondAsync(new MessageResponse("Thread not found"));
            var comment = new CommentModel(_htmlSanitizer.Sanitize(dto.Content), user, thread);
            if (dto.ParentCommentId.HasValue)
            {
                var parentComment = await _appDbContext.Comments
                    .FirstOrDefaultAsync(c => c.Id == dto.ParentCommentId.Value &&
                                             c.ThreadId == dto.ThreadId &&
                                             !c.IsDeleted);

                if (parentComment == null)
                    await context.RespondAsync("Parent comment not found or belongs to different thread");

                comment.AddParent(parentComment);
            }
            switch (dto.FormFile?.ContentType)
            {
                case "image/jpeg":
                case "image/png":
                    (string imageUrl, string tumbnailUrl) = await _imageTransform.ProcessAndUploadImageAsync(dto.FormFile, context.CancellationToken);
                    comment.SetImageUrls(imageUrl, tumbnailUrl);
                    break;
                case "image/gif":
                    (string gifUrl, string tumbnailGif) = await _imageTransform.ProcessAndUploadGifAsync(dto.FormFile, context.CancellationToken);
                    comment.SetImageUrls(gifUrl, tumbnailGif);
                    break;
                case "text/plain":
                    if (dto.FormFile.Length > 100 * 1024) // 100 KB
                        await context.RespondAsync("Text file size exceeds the limit of 100 KB");
                    var fileUrl = await _fileProvider.SaveFileAsync(dto.FormFile, context.CancellationToken);
                    comment.SetFileUrl(fileUrl);
                    break;
                default:
                    break;
            }

            await _appDbContext.Comments.AddAsync(comment, context.CancellationToken);
            await _appDbContext.SaveChangesAsync(context.CancellationToken);

            context.RespondAsync(new CreateCommentSuccesResponse());
        }
    }
}
