using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment
{
    public interface ICommentService
    {
        Task<IActionResult> GetByThreadAsync(CommentsByThreadDTO dto, CancellationToken cancellationToken);
        Task<IActionResult> GetByIdAsync(CommentFindDTO dto, CancellationToken cancellationToken);
        Task<IActionResult> CreateAsync(CommentCreateDTO dto, HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> UpdateAsync(CommentUpdateDTO dto, HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> DeleteAsync(CommentFindDTO dto, HttpContext httpContext, CancellationToken cancellationToken);
    }
}

