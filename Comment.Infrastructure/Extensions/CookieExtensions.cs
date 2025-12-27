using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Extensions
{
    /// <summary>
    /// Provides extension methods for managing HTTP cookies within the application.
    /// </summary>
    public static class CookieExtensions
    {
        /// <summary>
        /// Generates a JWT token and sets it as a secure, partitioned cookie in the HTTP response.
        /// </summary>
        /// <param name="http">The current HTTP context.</param>
        /// <param name="user">The user model to generate the token for.</param>
        /// <param name="jwtProvider">The provider used to generate the JWT.</param>
        /// <param name="jwtOptions">The configuration options for JWT expiration and settings.</param>
        public static void SetJwtCookie(HttpContext http, UserModel user, IJwtProvider _jwtProvider, IJwtOptions _jwtOptions)
        {
            var token = _jwtProvider.GenerateToken(user);
            var expiration = DateTimeOffset.UtcNow.AddDays(_jwtOptions.ExpiresDays);

            AppendSecureCookie(http, "jwt", token, expiration);
            SetPartitionedCookie(http);
        }

        public static void RemoveJwtCookie(HttpContext http)
        {
            var expiration = new DateTime(0);

            AppendSecureCookie(http, "jwt", "", expiration);
            SetPartitionedCookie(http);
        }

        /// <summary>
        /// Appends multiple cookies from a string array formatted as "key|value".
        /// </summary>
        /// <param name="http">The current HTTP context.</param>
        /// <param name="cookieArray">An array of strings where each element is a pipe-separated key-value pair.</param>
        public static void AppendCookie(HttpContext http, string[] cookieArray)
        {
            if (cookieArray.Length == 0) return;

            for (int i = 0; i < cookieArray.Length; i++)
            {
                var parts = cookieArray[i].Split('|');
                if (parts.Length != 2) continue;
                var key = parts[0];
                var value = parts[1];
                if (i == cookieArray.Length - 1)
                    http.Response.Cookies.Append(key, value, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Path = "/"
                    });
                else
                    http.Response.Cookies.Append(key, value);
            }

        }

        /// <summary>
        /// Appends a secure, cross-site compatible cookie to the HTTP response.
        /// </summary>
        /// <param name="http">The current HTTP context.</param>
        /// <param name="id">The name (key) of the cookie.</param>
        /// <param name="value">The value to be stored in the cookie.</param>
        /// <param name="expiration">The optional expiration date and time for the cookie.</param>
        public static void AppendSecureCookie(HttpContext http, string id, string value, DateTimeOffset? expiration)
        {
            http.Response.Cookies.Append(id, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = expiration
            });
        }

        /// <summary>
        /// Manually injects the "Partitioned" attribute into the last "Set-Cookie" header.
        /// </summary>
        /// <param name="http">The current HTTP context.</param>
        private static void SetPartitionedCookie(HttpContext http)
        {
            var setCookieHeader = http.Response.Headers["Set-Cookie"];
            if (!string.IsNullOrEmpty(setCookieHeader))
            {
                var lastCookie = setCookieHeader.LastOrDefault();
                if (lastCookie != null && !lastCookie.Contains("Partitioned"))
                {
                    http.Response.Headers["Set-Cookie"] = setCookieHeader.ToString().Replace(lastCookie, lastCookie + "; Partitioned");
                }
            }
        }
    }
}
