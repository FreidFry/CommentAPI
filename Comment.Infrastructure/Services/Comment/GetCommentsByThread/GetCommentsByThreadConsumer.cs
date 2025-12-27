using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.Enums;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Request;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread
{
    public class GetCommentsByThreadConsumer : IConsumer<CommentsByThreadRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IDatabase _dataBase;

        public GetCommentsByThreadConsumer(AppDbContext appDbContext, IMapper mapper
            , IConnectionMultiplexer connectionMultiplexer
            )
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _dataBase = connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// Retrieves a paginated list of comments for a specific thread with support for cursor-based navigation 
        /// and deep-linking (focusing) on a specific comment.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="CommentsByThreadRequestDTO"/>.</param>
        /// <returns>
        /// A <see cref="CommentsListResponse"/> containing the list of comments, a cursor for the next page, 
        /// and a boolean indicating if more data is available.
        /// </returns>
        /// <remarks>
        /// Pagination and Sorting Logic:
        /// <list type="bullet">
        /// <item>
        /// <term>Focus Mode:</term>
        /// <description>If <c>FocusCommentId</c> is provided, the method fetches a specific branch of the conversation.</description>
        /// </item>
        /// <item>
        /// <term>Cursor-Based Pagination:</term>
        /// <description>Uses the last item's value (Email, Username, or Date) as a cursor to fetch the next set of results, avoiding "offset skip" performance issues.</description>
        /// </item>
        /// <item>
        /// <term>Look-ahead Buffer:</term>
        /// <description>Fetches <c>Limit + 1</c> items to determine <c>HasMore</c> without an extra count query.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public async Task Consume(ConsumeContext<CommentsByThreadRequestDTO> context)
        {
            var request = context.Message;
            var cancellationToken = context.CancellationToken;
            var finalComments = new List<CommentViewModel>();
            if (request.FocusCommentId != null)
            {
                finalComments = await GetWithFocus(request, cancellationToken);
                await context.RespondAsync(new CommentsListResponse { Items = finalComments, HasMore = true });
            }

            finalComments = await GetWithoutFocus(request, cancellationToken);

            bool HasMore = false;
            if (finalComments.Count > request.Limit)
            {
                HasMore = true;
                finalComments.RemoveAt(request.Limit);
            }

            string? nextCursor = null;
            if (HasMore)
            {
                var last = finalComments.Last();
                nextCursor = request.SortByEnum switch
                {
                    SortByEnum.Email => last.Email,
                    SortByEnum.UserName => last.UserName,
                    SortByEnum.CreateAt => last.CreatedAt.ToString("O"),
                    _ => last.CreatedAt.ToString("O")
                };
            }

            await context.RespondAsync(new CommentsListResponse { Items = finalComments, NextCursor = nextCursor, HasMore = HasMore });
        }

        private async Task<List<CommentViewModel>> GetWithoutFocus(CommentsByThreadRequestDTO request, CancellationToken cancellationToken)
        {
            var paginatedLimit = request.Limit + 1;

            string sortKey = request.SortByEnum.ToString().ToLower();
            string indexKey = $"thread:{request.ThreadId}:comments:sort:{sortKey}";
            var order = request.IsAscending ? Order.Ascending : Order.Descending;

            List<CommentViewModel> finalComments = [];

            var cachedIds = await GetIdsFromRedis(_dataBase, indexKey, request.After, request.SortByEnum, order, paginatedLimit);

            if (cachedIds.Length > 0)
            {
                var tasks = cachedIds.Select(id => _dataBase.StringGetAsync($"comment:{id}"));
                var results = await Task.WhenAll(tasks);
                foreach (var res in results.Where(r => r.HasValue))
                {
                    var comment = JsonSerializer.Deserialize<CommentViewModel>(res!.ToString());
                    if (comment != null && comment.ParentCommentId == null)
                        finalComments.Add(comment);
                }
            }

            if (finalComments.Count < paginatedLimit)
            {
                int needed = paginatedLimit - finalComments.Count;

                string? effectiveAfter = finalComments.Count > 0
                    ? GetCursorFromItem(finalComments.Last(), request.SortByEnum)
                    : request.After;

                var dbComments = await GetFromDb(request, effectiveAfter, needed, cancellationToken);

                finalComments.AddRange(dbComments);
            }

            return finalComments;
        }

        private async Task<List<CommentViewModel>> GetWithFocus(CommentsByThreadRequestDTO request, CancellationToken cancellationToken)
        {
            var focusEntity = await _appDbContext.Comments
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.FocusCommentId, cancellationToken);

            if (focusEntity == null) return new List<CommentViewModel>();

            var rootId = focusEntity.ParentCommentId ?? focusEntity.Id;

            var rootComment = await _appDbContext.Comments
                .AsNoTracking()
                .Where(c => c.Id == rootId)
                .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (rootComment == null) return new List<CommentViewModel>();

            var replies = await _appDbContext.Comments
                .AsNoTracking()
                .Where(c => c.ParentCommentId == rootId && c.CreatedAt >= focusEntity.CreatedAt)
                .OrderBy(c => c.CreatedAt)
                .Take(5)
                .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            rootComment.Replies = replies;

            return [rootComment];
        }

        private async Task<List<CommentViewModel>> GetFromDb(CommentsByThreadRequestDTO request, string after, int need, CancellationToken cancellationToken)
        {
            var query = _appDbContext.Threads
                .Where(t => t.Id == request.ThreadId && !t.OwnerUser.IsDeleted && !t.OwnerUser.IsBanned && !t.IsBanned && !t.IsDeleted);

            var rootList = query
               .SelectMany(t => t.Comments);

            rootList = rootList
               .Where(c => c.ParentDepth == 0 && !c.IsDeleted && !c.IsBaned);


            if (!string.IsNullOrEmpty(after))
            {
                rootList = request.SortByEnum switch
                {
                    SortByEnum.Email => request.IsAscending
                        ? rootList.Where(c => string.Compare(c.User.Email, after) > 0)
                        : rootList.Where(c => string.Compare(c.User.Email, after) < 0),
                    SortByEnum.UserName => request.IsAscending
                        ? rootList.Where(c => string.Compare(c.User.UserName, after) > 0)
                        : rootList.Where(c => string.Compare(c.User.UserName, after) < 0),
                    SortByEnum.CreateAt => request.IsAscending
                        ? rootList.Where(c => c.CreatedAt > DateTime.Parse(after).ToUniversalTime())
                        : rootList.Where(c => c.CreatedAt < DateTime.Parse(after).ToUniversalTime()),
                    _ => request.IsAscending
                        ? rootList.Where(c => c.CreatedAt > DateTime.Parse(after).ToUniversalTime())
                        : rootList.Where(c => c.CreatedAt < DateTime.Parse(after).ToUniversalTime())
                };
            }

            rootList = request.SortByEnum switch
            {
                SortByEnum.Email => request.IsAscending
                    ? rootList.OrderBy(c => c.User.Email)
                    : rootList.OrderByDescending(c => c.User.Email),
                SortByEnum.UserName => request.IsAscending
                    ? rootList.OrderBy(c => c.User.UserName)
                    : rootList.OrderByDescending(c => c.User.UserName),
                SortByEnum.CreateAt => request.IsAscending
                    ? rootList.OrderBy(c => c.CreatedAt)
                    : rootList.OrderByDescending(c => c.CreatedAt),
                _ => request.IsAscending
                    ? rootList.OrderBy(c => c.CreatedAt)
                    : rootList.OrderByDescending(c => c.CreatedAt)
            };

            var comments = await rootList
                .Take(need)
                .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return comments ?? [];
        }

        private async Task<RedisValue[]> GetIdsFromRedis(IDatabase db, string key, string? after, SortByEnum sortBy, Order order, int limit)
        {
            if (sortBy == SortByEnum.CreateAt)
            {
                double score = string.IsNullOrEmpty(after)
                    ? (order == Order.Descending ? double.PositiveInfinity : double.NegativeInfinity)
                    : DateTime.Parse(after).ToUniversalTime().Ticks;

                return await db.SortedSetRangeByScoreAsync(key, score,
                    order == Order.Descending ? double.NegativeInfinity : double.PositiveInfinity,
                    Exclude.Start, order, 0, limit);
            }

            return await db.SortedSetRangeByValueAsync(key, after, default, Exclude.Start, order, limit);
        }

        private static string GetCursorFromItem(CommentViewModel item, SortByEnum sortBy) => sortBy switch
        {
            SortByEnum.Email => item.Email,
            SortByEnum.UserName => item.UserName,
            SortByEnum.CreateAt => item.CreatedAt.ToString("O"),
            _ => item.CreatedAt.ToString("O")
        };
    }
}
