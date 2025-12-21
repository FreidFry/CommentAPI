using AutoMapper;
using AutoMapper.QueryableExtensions;
using Comment.Core.Data;
using Comment.Infrastructure.Enums;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Request;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread
{
    public class GetCommentsByThreadConsumer : IConsumer<CommentsByThreadRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        public GetCommentsByThreadConsumer(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<CommentsByThreadRequestDTO> context)
        {
            var query = _appDbContext.Threads
                .Where(t => t.Id == context.Message.ThreadId && !t.OwnerUser.IsDeleted && !t.OwnerUser.IsBanned && !t.IsBanned && !t.IsDeleted);

            var rootList = query
               .SelectMany(t => t.Comments);

            rootList = context.Message.SortByEnum switch
            {
                SortByEnum.Email => context.Message.IsAscending
                    ? rootList.OrderBy(c => c.User.Email)
                    : rootList.OrderByDescending(c => c.User.Email),
                SortByEnum.UserName => context.Message.IsAscending
                    ? rootList.OrderBy(c => c.User.UserName)
                    : rootList.OrderByDescending(c => c.User.UserName),
                SortByEnum.CreateAt => context.Message.IsAscending
                    ? rootList.OrderBy(c => c.CreatedAt)
                    : rootList.OrderByDescending(c => c.CreatedAt),
                _ => context.Message.IsAscending
                    ? rootList.OrderBy(c => c.CreatedAt)
                    : rootList.OrderByDescending(c => c.CreatedAt)
            };

            rootList = rootList.Where(c => c.ParentDepth == 0 && !c.IsDeleted && !c.IsBaned);

            if (!string.IsNullOrEmpty(context.Message.After))
            {
                rootList = context.Message.SortByEnum switch
                {
                    SortByEnum.Email => context.Message.IsAscending
                        ? rootList.Where(c => string.Compare(c.User.Email, context.Message.After) > 0)
                        : rootList.Where(c => string.Compare(c.User.Email, context.Message.After) < 0),
                    SortByEnum.UserName => context.Message.IsAscending
                        ? rootList.Where(c => string.Compare(c.User.UserName, context.Message.After) > 0)
                        : rootList.Where(c => string.Compare(c.User.UserName, context.Message.After) < 0),
                    SortByEnum.CreateAt => context.Message.IsAscending
                        ? rootList.Where(c => c.CreatedAt > DateTime.Parse(context.Message.After).ToUniversalTime())
                        : rootList.Where(c => c.CreatedAt < DateTime.Parse(context.Message.After).ToUniversalTime()),
                    _ => context.Message.IsAscending
                        ? rootList.Where(c => c.CreatedAt > DateTime.Parse(context.Message.After).ToUniversalTime())
                        : rootList.Where(c => c.CreatedAt < DateTime.Parse(context.Message.After).ToUniversalTime())
                };
            }

            var comments = await rootList
                .Take(context.Message.Limit + 1)
                .ProjectTo<CommentViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(context.CancellationToken);

            bool HasMore = false;
            if (comments.Count > context.Message.Limit)
            {
                HasMore = true;
                comments.RemoveAt(context.Message.Limit);
            }

            string? nextCursor = null;
            if (HasMore)
            {
                var last = comments.Last();
                nextCursor = context.Message.SortByEnum switch
                {
                    SortByEnum.Email => last.Email,
                    SortByEnum.UserName => last.UserName,
                    SortByEnum.CreateAt => last.CreatedAt.ToString("O"),
                    _ => last.CreatedAt.ToString("O")
                };
            }

            await context.RespondAsync(new CommentsListResponse { Items = comments, NextCursor = nextCursor, HasMore = HasMore });
        }
    }
}
