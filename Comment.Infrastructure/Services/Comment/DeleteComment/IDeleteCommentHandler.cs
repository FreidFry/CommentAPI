using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment.DeleteComment
{
    public interface IDeleteCommentHandler
    {
        Task<IActionResult> Handle(DeleteCommentRequest request, HttpContext http, CancellationToken cancellationToken);
    }
}