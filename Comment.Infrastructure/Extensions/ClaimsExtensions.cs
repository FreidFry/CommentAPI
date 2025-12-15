using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Comment.Infrastructure.Extensions
{
    public static class ClaimsExtensions
    {
        public static Guid? GetCallerId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim?.Value, out Guid callerId))
                return callerId;
            return null;
        }

        public static List<string> GetCallerRoles(HttpContext httpContext)
        {
            var roleClaims = httpContext.User.FindAll(ClaimTypes.Role);
            return [.. roleClaims.Select(c => c.Value)];
        }

        public static string? GetCallerUserName(HttpContext httpContext)
        {
            var userNameClaim = httpContext.User.FindFirst(ClaimTypes.Name);
            return userNameClaim?.Value;
        }
    }
}
