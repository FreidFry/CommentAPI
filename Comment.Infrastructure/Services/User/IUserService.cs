using Comment.Infrastructure.Services.User.DTOs.Request;
using Comment.Infrastructure.Services.User.DTOs.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.User
{
    public interface IUserService
    {
        Task<CommonUserDataDTO?> GetByIdAsync(UserFindDto dto, CancellationToken cancellationToken);
        Task<CommonUserDataDTO?> GetCurrentAsync(HttpContext httpContext, CancellationToken cancellationToken);
        Task<IActionResult> UpdateProfileAvatarAsync(UserUpdateAvatarDTO dto, HttpContext httpContext, CancellationToken cancellationToken);
    }
}