using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response;
using Comment.Infrastructure.Services.Comment.GetReply.Request;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Comment.GetReply
{
    public class GetReplyConsumer : IConsumer<GetReplyRequest>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly int _CommentCount = 3;

        public GetReplyConsumer(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated list of direct replies (child comments) for a specific parent comment.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="GetReplyRequest"/>.</param>
        /// <returns>
        /// A <see cref="CommentsListResponse"/> containing the collection of replies, 
        /// a cursor for the next batch, and a flag indicating if more replies exist.
        /// </returns>
        /// <remarks>
        /// Retrieval Logic:
        /// <list type="bullet">
        /// <item>
        /// <term>Security Filtering:</term>
        /// <description>Automatically excludes replies from banned or deleted users, as well as comments marked as deleted.</description>
        /// </item>
        /// <item>
        /// <term>Depth Control:</term>
        /// <description>Filters replies to ensure they are exactly one level deeper than the parent (<c>ParentDepth + 1</c>).</description>
        /// </item>
        /// <item>
        /// <term>Cursor Pagination:</term>
        /// <description>Uses the <c>After</c> timestamp to fetch the next set of chronological results.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public async Task Consume(ConsumeContext<GetReplyRequest> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;

            var query = _appDbContext.Comments
                .OrderByDescending (c => c.CreatedAt)
                .Where(c => c.Id == dto.CommentId && !c.IsDeleted && !c.User.IsBanned && !c.User.IsDeleted)
                .SelectMany(c => c.Replyes)
                    .OrderByDescending(c => c.CreatedAt)
                    .Where(c => c.ParentDepth == c.ParentComment!.ParentDepth + 1 && !c.IsDeleted && !c.User.IsBanned && !c.User.IsDeleted);

            if (dto.After.HasValue)
                query = query.Where(c => c.CreatedAt < dto.After);

            var comments = await query
                .Take(_CommentCount + 1)
                .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            if (comments == null)
            {
                await context.RespondAsync(new StatusCodeResponse("Comment not found.", 404));
                return;
            }
            bool HasMore = false;
            string? nextCursor = null;
            if(comments.Count > _CommentCount)
            {
                HasMore = true;
                comments.RemoveAt(3);
                nextCursor = comments.Last().CreatedAt.ToString("O");
            }

            await context.RespondAsync(new CommentsListResponse { Items = comments, NextCursor = nextCursor, HasMore = HasMore });
        }
    }
}
