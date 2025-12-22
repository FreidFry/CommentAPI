using Comment.Infrastructure.Services.Comment.UpdateComment.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment.UpdateComment
{
    public interface IUpdateCommentHandler
    {
        Task<IActionResult> Handle(CommentUpdateRequest request, HttpContext http, CancellationToken cancellationToken);
    }
}