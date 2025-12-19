using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Auth.DTOs;
using Comment.Infrastructure.Services.Auth.Login.Response;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Auth.Login
{
    public class AuthHandler : IAuthHandler
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IJwtOptions _jwtOptions;
        private readonly IRequestClient<UserLoginRequest> _client;
        public AuthHandler(IRequestClient<UserLoginRequest> client)
        {
            _client = client;
        }

        public async Task<IActionResult> HandleLoginAsync(UserLoginRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var response = await _client.GetResponse<LoginResponse, NotFoundResult, UnauthorizedResult>(request);

            if (response.Is(out Response<LoginResponse> succes))
            {
                var data = succes.Message;

                SetJwtCookie(httpContext, data.UserModel);

                return new OkObjectResult(new { data.Id, data.UserName, data.Roles });
            }

            if (response.Is(out Response<LoginNotFound> notFount)) return new NotFoundResult();
            if (response.Is(out Response<LoginUnauthorized> notUnauthorized)) return new UnauthorizedResult();

            return new StatusCodeResult(500);
        }

        void SetJwtCookie(HttpContext http, UserModel user)
        {
            var token = _jwtProvider.GenerateToken(user);
            var expiration = DateTimeOffset.UtcNow.AddDays(_jwtOptions.ExpiresDays);

            AppendSecureCookie(http, "jwt", token, expiration);
            SetPartitionedCookie(http);

        }

        private void AppendCookie(HttpContext http, string[] cookieArray)
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

        void AppendSecureCookie(HttpContext http, string id, string value, DateTimeOffset? expiration)
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
        private void SetPartitionedCookie(HttpContext http)
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
