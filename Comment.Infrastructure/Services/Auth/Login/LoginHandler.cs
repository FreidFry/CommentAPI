using Comment.Core.Interfaces;
using Comment.Infrastructure.CommonDTOs;
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

        public async Task<IActionResult> Handle(UserLoginRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var response = await _client.GetResponse<LoginSuccesResponse, StatusCodeResponse>(request, cancellationToken);

            if (response.Is(out Response<LoginSuccesResponse> succes))
            {
                var data = succes.Message;

                SetJwtCookie(httpContext, data.UserModel, _jwtProvider, _jwtOptions);

                return new OkObjectResult(new { data.Id, data.UserName, data.Roles });
            }

            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new StatusCodeResult(statusCode.Message.StatusCode);

            return new StatusCodeResult(500);
        }
    }
}
