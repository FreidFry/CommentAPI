using Microsoft.AspNetCore.Http;
using static Comment.Infrastructure.Extensions.CookieExtensions;

namespace Comment.Infrastructure.Services.Auth.Logout
{
    public class LogoutHandler : ILogoutHandler
    {
        public void Handle(HttpContext httpContext)
        {
            RemoveJwtCookie(httpContext);
        }
    }
}
