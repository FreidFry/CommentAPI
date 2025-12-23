using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Hubs;
using Comment.Infrastructure.Interfaces;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.CreateComment.Response;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System.Text;
using System.Text.Json;

namespace Comment.Infrastructure.Services.Comment.CreateComment
{
    public class CreateCommentConsumer : IConsumer<CommentCreateRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IImageTransform _imageTransform;
        private readonly IHtmlSanitize _htmlSanitizer;
        private readonly IFileProvider _fileProvider;
        private readonly IDatabase _redisDatabase;
        private readonly IRedisDatabase _redisCache;
        private readonly IHubContext<CommentHub> _hubContext;

        public CreateCommentConsumer(AppDbContext appDbContext, IImageTransform imageTransform, IHtmlSanitize htmlSanitizer, IFileProvider fileProvider, IRedisDatabase redisCache, IConnectionMultiplexer connectionMultiplexer, IHubContext<CommentHub> hubContext)
        {
            _appDbContext = appDbContext;
            _imageTransform = imageTransform;
            _htmlSanitizer = htmlSanitizer;
            _fileProvider = fileProvider;
            _redisCache = redisCache;
            _redisDatabase = connectionMultiplexer.GetDatabase();
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<CommentCreateRequestDTO> context)
        {
            var dto = context.Message;

            var thread = await _appDbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && !t.IsDeleted && !t.IsBanned, context.CancellationToken);

            if (thread == null)
            {
                await context.RespondAsync(new StatusCodeResponse("Thread not found", 404));
                return;
            }

            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Id == dto.CallerId && !u.IsBanned && !u.IsDeleted, context.CancellationToken);

            if (user == null)
            {
                await context.RespondAsync(new StatusCodeResponse("User not found", 404));
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
                    await context.RespondAsync(new StatusCodeResponse("Parent comment not found", 404));
                    return;
                }

                comment.AddParent(parentComment);
                parentAuthorId = parentComment.UserId;
            }
            if (dto.FileKey != null)
            {

                var fileBytes = await _redisCache.GetAsync<byte[]?>(dto.FileKey);

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

            if (parentAuthorId.HasValue && parentAuthorId.Value != user.Id)
                await NotifyAboutComment(comment);


            await context.RespondAsync(new CreateCommentSuccesResponse());

            //send to redis

            string json = JsonSerializer.Serialize(comment);

            string dateIndexKey = $"thread:{comment.ThreadId}:comments:sort:createat";
            string nameIndexKey = $"thread:{comment.ThreadId}:comments:sort:username";

            await _redisDatabase.StringSetAsync($"comment:{comment.Id}", json);

            await _redisDatabase.SortedSetAddAsync(dateIndexKey, comment.Id.ToString(), comment.CreatedAt.Ticks);
            double nameScore = GetNameScore(comment.User.UserName);
            await _redisDatabase.SortedSetAddAsync(nameIndexKey, comment.Id.ToString(), nameScore);

            await _redisDatabase.SortedSetRemoveRangeByRankAsync(dateIndexKey, 0, -76);
            await _redisDatabase.SortedSetRemoveRangeByRankAsync(nameIndexKey, 0, -76);
        }

        public async Task NotifyAboutComment(CommentModel comment)
        {
            await _hubContext.Clients.Group($"Post_{comment.Id}")
                .SendAsync("ReceiveComment", comment);

            if (comment.ParentComment?.UserId != null)
            {
                await _hubContext.Clients.User(comment.ParentComment.UserId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        title = "Новый ответ",
                        body = $"{comment.User.UserName} ответил на ваш комментарий",
                        link = $"/post/{comment.Id}#comment-{comment.Id}"
                    });
            }
        }

        private static double GetNameScore(string name)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(name.ToLower().PadRight(8));
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}
