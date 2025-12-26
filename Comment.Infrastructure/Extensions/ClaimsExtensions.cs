using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Comment.Infrastructure.Extensions
{
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Retrieves the User ID from the JWT token stored in <see cref="ClaimTypes.NameIdentifier"/>.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>The parsed <see cref="Guid"/> if the claim exists and is valid; otherwise, <c>null</c>.</returns>
        public static Guid? GetCallerId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim?.Value, out Guid callerId))
                return callerId;
            return null;
        }

        /// <summary>
        /// Retrieves the list of user roles from the JWT token stored in <see cref="ClaimTypes.Role"/>.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>A list of roles assigned to the caller.</returns>
        public static List<string> GetCallerRoles(HttpContext httpContext)
        {
            var roleClaims = httpContext.User.FindAll(ClaimTypes.Role);
            return [.. roleClaims.Select(c => c.Value)];
        }

        /// <summary>
        /// Retrieves the username from the JWT token stored in <see cref="ClaimTypes.Name"/>.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>The username string if the claim exists; otherwise, <c>null</c>.</returns>
        public static string? GetCallerUserName(HttpContext httpContext)
        {
            var userNameClaim = httpContext.User.FindFirst(ClaimTypes.Name);
            return userNameClaim?.Value;
        }
    }
}
