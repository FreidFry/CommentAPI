using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Request;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread
{
    public interface IGetCommentsByThreadHandler
    {
        Task<IActionResult> Handle(Guid threadId, CommentsByThreadRequest request, CancellationToken cancellationToken);
    }
}