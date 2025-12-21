using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Events;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.CreateComment.Response;
using Comment.Infrastructure.Utils;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Comment.Infrastructure.Services.Comment.CreateComment
{
    public class CreateCommentConsumer : IConsumer<CommentCreateRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IImageTransform _imageTransform;
        private readonly IHtmlSanitize _htmlSanitizer;
        private readonly IFileProvider _fileProvider;
        private readonly IRedisDatabase _redisDatabase;

        public CreateCommentConsumer(AppDbContext appDbContext, IImageTransform imageTransform, IHtmlSanitize htmlSanitizer, IFileProvider fileProvider, IRedisDatabase redisDatabase)
        {
            _appDbContext = appDbContext;
            _imageTransform = imageTransform;
            _htmlSanitizer = htmlSanitizer;
            _fileProvider = fileProvider;
            _redisDatabase = redisDatabase;
        }

        public async Task Consume(ConsumeContext<CommentCreateRequestDTO> context)
        {
            var dto = context.Message;

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted && !t.IsBanned, context.CancellationToken);

            if (thread == null)
            {
                await context.RespondAsync(new MessageResponse("Thread not found"));
                return;
            }

            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Id == dto.CallerId && !u.IsBanned && !u.IsDeleted, context.CancellationToken);

            if (user == null)
            {
                await context.RespondAsync(new MessageResponse("User not found"));
                return;
            }

            var comment = new CommentModel(_htmlSanitizer.Sanitize(dto.Content), user, thread);

            Guid? parentAuthorId = null;

            if (dto.ParentCommentId.HasValue)
            {
                var parentComment = await _appDbContext.Comments
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == dto.ParentCommentId && c.ThreadId == dto.ThreadId, context.CancellationToken);

                if (parentComment == null)
                {
                    await context.RespondAsync(new MessageResponse("Parent comment not found"));
                    return;
                }

                comment.AddParent(parentComment);
                parentAuthorId = parentComment.UserId;
            }
            if (dto.FileKey != null)
            {

                var fileBytes = await _redisDatabase.GetAsync<byte[]?>(dto.FileKey);

                if (fileBytes != null)
                {
                    using Stream fileStream = new MemoryStream(fileBytes);
                    switch (dto.ContentType)
                    {
                        case "image/jpeg":
                        case "image/png":
                            (string imageUrl, string tumbnailUrl) = await _imageTransform.ProcessAndUploadImageAsync(fileStream, context.CancellationToken);
                            comment.SetImageUrls(imageUrl, tumbnailUrl);
                            break;

                        case "image/gif":
                            (string gifUrl, string tumbnailGif) = await _imageTransform.ProcessAndUploadGifAsync(fileStream, context.CancellationToken);
                            comment.SetImageUrls(gifUrl, tumbnailGif);
                            break;

                        case "text/plain":
                            var fileUrl = await _fileProvider.SaveFileAsync(fileStream, context.CancellationToken);
                            comment.SetFileUrl(fileUrl);
                            break;
                    }
                }
            }

            await _appDbContext.Comments.AddAsync(comment, context.CancellationToken);
            await _appDbContext.SaveChangesAsync(context.CancellationToken);

            if (parentAuthorId.HasValue && parentAuthorId.Value != user.Id)
            {
                await context.Publish(new CommentRepliedEvent(
                    ParentAuthorId: parentAuthorId.Value,
                    ReplyAuthorId: user.Id,
                    ThreadId: thread.Id,
                    CreatedAt: DateTime.UtcNow
                ), context.CancellationToken);
            }

            await context.RespondAsync(new CreateCommentSuccesResponse());
        }
    }
}
