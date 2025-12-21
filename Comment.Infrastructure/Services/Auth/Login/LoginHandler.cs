using Comment.Core.Interfaces;
using Comment.Infrastructure.Services.Auth.DTOs;
using Comment.Infrastructure.Services.Auth.Login.Response;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Comment.Infrastructure.Extensions.CookieExtensions;

namespace Comment.Infrastructure.Services.Auth.Login
{
    public class LoginHandler : ILoginHandler
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IJwtOptions _jwtOptions;
        private readonly IRequestClient<UserLoginRequest> _client;
        public LoginHandler(IRequestClient<UserLoginRequest> client, IJwtOptions jwtOptions, IJwtProvider jwtProvider)
        {
            _client = client;
            _jwtProvider = jwtProvider;
            _jwtOptions = jwtOptions;
        }

        public async Task<IActionResult> HandleLoginAsync(UserLoginRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var response = await _client.GetResponse<LoginSuccesResponse, NotFoundResult, UnauthorizedResult>(request, cancellationToken);

            if (response.Is(out Response<LoginSuccesResponse> succes))
            {
                var data = succes.Message;

                SetJwtCookie(httpContext, data.UserModel, _jwtProvider, _jwtOptions);

                return new OkObjectResult(new { data.Id, data.UserName, data.Roles });
            }

            if (response.Is(out Response<LoginNotFound> notFount)) return new NotFoundObjectResult(notFount.Message.msg);
            if (response.Is(out Response<LoginUnauthorized> notUnauthorized)) return new UnauthorizedResult();

            return new StatusCodeResult(500);
        }
    }
}
