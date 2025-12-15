using Comment.Infrastructure.Services.User.DTOs.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.User
{
    public interface IUserService
    {
        Task<IActionResult> GetByIdAsync(Guid? UserId, HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> GetCurrentAsync(HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> UpdateProfileAvatarAsync(UserUpdateAvatarDTO dto, HttpContext httpContext, CancellationToken cancellationToken);
    }
}