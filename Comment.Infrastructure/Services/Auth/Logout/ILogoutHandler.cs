using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Services.Auth.Logout
{
    public interface ILogoutHandler
    {
        void Handle(HttpContext httpContext);
    }
}