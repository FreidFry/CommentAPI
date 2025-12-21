using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Microsoft.AspNetCore.Http;

namespace Comment.Infrastructure.Extensions
{
    public static class CookieExtensions
    {
        public static void SetJwtCookie(HttpContext http, UserModel user, IJwtProvider _jwtProvider, IJwtOptions _jwtOptions)
        {
            var token = _jwtProvider.GenerateToken(user);
            var expiration = DateTimeOffset.UtcNow.AddDays(_jwtOptions.ExpiresDays);

            AppendSecureCookie(http, "jwt", token, expiration);
            SetPartitionedCookie(http);
        }

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
