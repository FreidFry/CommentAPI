using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment.CreateComment
{
    public interface ICreateCommentHandler
    {
        Task<IActionResult> CreateCommentHandleAsync(CommentCreateRequest request, HttpContext httpContext, CancellationToken cancellationToken);
    }
}