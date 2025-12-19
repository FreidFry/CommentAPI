using Comment.Infrastructure.Services.Auth.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Auth.Login
{
    public interface IAuthHandler
    {
        Task<IActionResult> HandleLoginAsync(UserLoginRequest request, HttpContext httpContext, CancellationToken cancellationToken);
    }
}