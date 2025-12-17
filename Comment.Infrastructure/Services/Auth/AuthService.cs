using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Auth.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;
using static System.Net.WebRequestMethods;

namespace Comment.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IJwtOptions _envOptions;

        public AuthService(AppDbContext appDbContext, IPasswordHasher passwordHasher, IJwtProvider jwtProvider, IJwtOptions envOptions)
        {
            _appDbContext = appDbContext;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _envOptions = envOptions;
        }

        public async Task<IActionResult> RegisterAsync(UserRegisterDto UserDto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            if (!UserDto.Password.Equals(UserDto.ConfirmPassword))
                return new BadRequestObjectResult("Passwords do not match");
            var existingUser = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.UserName == UserDto.UserName, cancellationToken);

            if (existingUser != null) return new ConflictObjectResult("User already exists");

            var newUser = new UserModel(UserDto.UserName, UserDto.Email, _passwordHasher.HashPassword(UserDto.Password));

            await _appDbContext.Users.AddAsync(newUser, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            SetJwtCookie(httpContext, newUser);

            return new OkResult();
        }

        public async Task<IActionResult> LoginAsync(UserLoginDto UserDto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Email == UserDto.Email, cancellationToken);

            if (user == null) return new NotFoundObjectResult("User not found");
            if (!_passwordHasher.VerifyPassword(UserDto.Password, user.HashPassword))
                return new UnauthorizedResult();

            SetJwtCookie(httpContext, user);

            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            return new OkObjectResult(new { Message = "Login successful" });
        }

        public void Logout(HttpContext httpContext)
        {
            httpContext.Response.Cookies.Delete("jwt");
        }

        public async Task<IActionResult> Init(HttpContext httpContext)
        {
            try
            {

            var id = GetCallerId(httpContext);
            var userName = GetCallerUserName(httpContext);
            var roles = GetCallerRoles(httpContext);
            string[] cookies = {
                $"{id}|{id.ToString() ?? string.Empty}",
                $"{userName}|{userName?.ToString() ?? string.Empty}",
                $"{roles}|{string.Join(",", roles) ?? string.Empty}"
            };
            AppendCookie(httpContext, cookies);
            SetPartitionedCookie(httpContext);
            return new OkResult();
            }
            catch (Exception ex)
            {
                // Это лог в консоль Railway
                Console.WriteLine($"FATAL ERROR in Init: {ex.Message} \n {ex.StackTrace}");
                return new StatusCodeResult(500);
            }
        }

        void SetJwtCookie(HttpContext http, UserModel user)
        {
            var token = _jwtProvider.GenerateToken(user);
            var expiration = DateTimeOffset.UtcNow.AddDays(_envOptions.ExpiresDays);

            AppendSecureCookie(http, "jwt", token, expiration);
            SetPartitionedCookie(http);

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
