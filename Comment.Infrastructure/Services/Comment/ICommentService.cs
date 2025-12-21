using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.UpdateComment.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment
{
    public interface ICommentService
    {
        Task<IActionResult> GetByThreadAsync(Guid threadId, CommentsByThreadDTO dto, CancellationToken cancellationToken);
        Task<IActionResult> GetByIdAsync(CommentFindDTO dto, CancellationToken cancellationToken);
        Task<IActionResult> CreateAsync(CommentCreateRequest dto, HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> UpdateAsync(CommentUpdateRequest dto, HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> DeleteAsync(CommentFindDTO dto, HttpContext httpContext, CancellationToken cancellationToken);
    }
}

