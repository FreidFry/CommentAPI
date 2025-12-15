using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread
{
    public interface IThreadService
    {
        Task<IActionResult> GetThreadsThreeAsync(ThreadsThreeDTO dto, CancellationToken cancellationToken);
        Task<IActionResult> GetByIdAsync(ThreadFindDTO dto, HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> CreateAsync(ThreadCreateDTO dto, HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> UpdateAsync(ThreadUpdateDTO dto, HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> DeleteAsync(Guid id, HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> RestoreAsync(Guid id, HttpContext httpContext, CancellationToken cancellationToken);
    }
}