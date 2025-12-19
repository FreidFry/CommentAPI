using Comment.Infrastructure.Services.Auth.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Auth
{
    public interface IAuthService
    {
        Task<IActionResult> Init(HttpContext httpContext);
        Task<IActionResult> LoginAsync(UserLoginRequest UserDto, HttpContext httpContext, CancellationToken cancellationToken);
        void Logout(HttpContext httpContext);
        Task<IActionResult> RegisterAsync(UserRegisterDto UserDto, HttpContext httpContext, CancellationToken cancellationToken);
    }
}