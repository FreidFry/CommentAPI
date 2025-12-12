using Comment.Infrastructure.Services.Auth.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Comment.Infrastructure.Services.Auth
{
    public interface IAuthService
    {
        string Init(ClaimsPrincipal user);
        Task<IActionResult> Login(UserLoginDto UserDto, HttpContext httpContext, CancellationToken cancellationToken);
        void Logout(HttpContext httpContext);
        Task<IActionResult> RegisterAsync(UserRegisterDto UserDto, HttpContext httpContext, CancellationToken cancellationToken);
    }
}