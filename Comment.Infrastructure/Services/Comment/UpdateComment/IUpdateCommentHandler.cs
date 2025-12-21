using Comment.Infrastructure.Services.Comment.UpdateComment.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment.UpdateComment
{
    public interface IUpdateCommentHandler
    {
        Task<IActionResult> UpdateCommentHandle(CommentUpdateRequest request, HttpContext http, CancellationToken cancellationToken);
    }
}