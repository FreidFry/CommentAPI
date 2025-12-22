using Comment.Infrastructure.Services.Auth.Register.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Auth.Register
{
    public interface IRegisterHandler
    {
        Task<IActionResult> Handle(UserRegisterRequest request, HttpContext httpContext, CancellationToken cancellationToken);
    }
}