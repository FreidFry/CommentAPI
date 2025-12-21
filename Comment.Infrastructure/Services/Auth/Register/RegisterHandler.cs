using Comment.Core.Interfaces;
using Comment.Infrastructure.Services.Auth.Register.Request;
using Comment.Infrastructure.Services.Auth.Register.Response;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Comment.Infrastructure.Extensions.CookieExtensions;

namespace Comment.Infrastructure.Services.Auth.Register
{
    public class RegisterHandler : IRegisterHandler
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IJwtOptions _jwtOptions;
        private readonly IRequestClient<UserRegisterRequest> _client;

        public RegisterHandler(IJwtProvider jwtProvider, IJwtOptions jwtOptions, IRequestClient<UserRegisterRequest> client)
        {
            _jwtProvider = jwtProvider;
            _jwtOptions = jwtOptions;
            _client = client;
        }


        public async Task<IActionResult> RegisterHandleAsync(UserRegisterRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var response = await _client.GetResponse<RegisterSuccesResult, ConflictRegisterResponse>(request, cancellationToken);

            if (response.Is(out Response<RegisterSuccesResult> sucess))
            {
                var data = sucess.Message;
                SetJwtCookie(httpContext, data.UserModel, _jwtProvider, _jwtOptions);

                return new OkObjectResult(new { data.Id, data.UserName, data.Roles });
            }
            if (response.Is(out Response<ConflictRegisterResponse> error)) return new ConflictObjectResult(error.Message.msg);

            return new StatusCodeResult(500);
        }


    }
}
