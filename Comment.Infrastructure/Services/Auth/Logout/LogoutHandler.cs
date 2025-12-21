using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Services.Auth.Logout
{
    public class LogoutHandler : ILogoutHandler
    {
        public void Logout(HttpContext httpContext)
        {
            httpContext.Response.Cookies.Delete("jwt");
        }
    }
}
