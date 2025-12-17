using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Core.Persistence;
using Comment.Infrastructure.Services.Auth.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

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
            var id = GetCallerId(httpContext);
            var userName = GetCallerUserName(httpContext);
            var roles = GetCallerRoles(httpContext);

            AppendCookies(httpContext, "id", id.ToString() ?? string.Empty);
            AppendCookies(httpContext, "userName", userName?.ToString() ?? string.Empty);
            AppendCookies(httpContext, "roles", string.Join(",", roles) ?? string.Empty);


            return new OkResult();
        }

        void SetJwtCookie(HttpContext http, UserModel user)
        {
            var token = _jwtProvider.GenerateToken(user);
            var expiration = DateTimeOffset.UtcNow.AddDays(_envOptions.ExpiresDays);

            AppendSecureCookie(http, "jwt", token, expiration);
        }

        void AppendSecureCookie(HttpContext http, string key, string value, DateTimeOffset? expiration)
        {
            if (string.IsNullOrEmpty(value)) return;

            var cookieHeader = $"{key}={value}; Secure; HttpOnly; SameSite=None; Partitioned; Path=/;";
            http.Response.Headers.Append("Set-Cookie", cookieHeader);
        }
        private void AppendCookies(HttpContext httpContext, string key, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            var cookieHeader = $"{key}={value}; Secure; HttpOnly; SameSite=None; Partitioned; Path=/;";
            httpContext.Response.Headers.Append("Set-Cookie", cookieHeader);
        }
    }
}
