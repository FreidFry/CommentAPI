using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Interfaces;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.CreateComment.Response;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using MassTransit;
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
        private readonly IMapper _mapper;

        public CreateCommentConsumer(AppDbContext appDbContext, IImageTransform imageTransform, IHtmlSanitize htmlSanitizer, IFileProvider fileProvider, IRedisDatabase redisCache, IConnectionMultiplexer connectionMultiplexer, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _imageTransform = imageTransform;
            _htmlSanitizer = htmlSanitizer;
            _fileProvider = fileProvider;
            _redisCache = redisCache;
            _redisDatabase = connectionMultiplexer.GetDatabase();
            _mapper = mapper;
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

            if (_htmlSanitizer.HasTextContent(dto.Content))
            {
                await context.RespondAsync(new StatusCodeResponse("The comment must contain plain text.", 400));
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


            await context.RespondAsync(new CreateCommentSuccesResponse());

            //send to redis

            var commentView = _mapper.Map<CommentViewModel>(comment);

            string json = JsonSerializer.Serialize(commentView);

            string dateIndexKey = $"thread:{comment.ThreadId}:comments:sort:createat";
            string nameIndexKey = $"thread:{comment.ThreadId}:comments:sort:username";

            var batch = _redisDatabase.CreateBatch();

            if (parentAuthorId.HasValue && parentAuthorId.Value != user.Id)
            {
                var parent = await _redisDatabase.StringGetAsync($"comment:{comment.ParentCommentId}");
                var parentComment = JsonSerializer.Deserialize<CommentViewModel>(parent.ToString());
                parentComment.CommentCount += 1;
                batch.StringSetAsync($"comment:{comment.ParentCommentId}", JsonSerializer.Serialize(parentComment), TimeSpan.FromHours(1));
                var notification = new NotificationModel
                {
                    Title = "Ответ на ваш комментарий!",
                    Message = comment.Content.Length > 50 ? comment.Content.Substring(0, 50) : comment.Content,
                    CommentId = (Guid)comment.ParentCommentId!,
                    Type = "Comment",
                    CreateAt = comment.CreatedAt,
                    CreatorId = comment.UserId,
                    RecipientId = comment.ParentComment!.UserId,
                    ThreadId = comment.ThreadId
                };

                batch.ListRightPushAsync("notification_queue", JsonSerializer.Serialize(notification));

            }

            batch.ListRightPushAsync("comments_queue", JsonSerializer.Serialize(comment));

            batch.StringSetAsync($"comment:{comment.Id}", json);

            batch.SortedSetAddAsync(dateIndexKey, comment.Id.ToString(), comment.CreatedAt.Ticks);
            double nameScore = GetNameScore(comment.User.UserName);
            batch.SortedSetAddAsync(nameIndexKey, comment.Id.ToString(), nameScore);

            batch.SortedSetRemoveRangeByRankAsync(dateIndexKey, 0, -76);
            batch.SortedSetRemoveRangeByRankAsync(nameIndexKey, 0, -76);

            batch.Execute();
        }

        private static double GetNameScore(string name)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(name.ToLower().PadRight(8));
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}
