using Comment.Core.Interfaces;
using Comment.Infrastructure.Services.Auth.Register.Request;
using Comment.Infrastructure.Services.Auth.Register.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Comment.Infrastructure.Extensions.CookieExtensions;

namespace Comment.Infrastructure.Services.Auth.Register
{
    public class RegisterHandler : HandlerWrapper, IRegisterHandler
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IJwtOptions _jwtOptions;
        private readonly IRequestClient<UserRegisterRequest> _client;

        public RegisterHandler(IJwtProvider jwtProvider, IJwtOptions jwtOptions, IRequestClient<UserRegisterRequest> client, ILogger<RegisterHandler> _logger) : base(_logger)
        {
            _jwtProvider = jwtProvider;
            _jwtOptions = jwtOptions;
            _client = client;
        }


        public async Task<IActionResult> Handle(UserRegisterRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var response = await _client.GetResponse<RegisterSuccesResult, ConflictRegisterResponse>(request, cancellationToken);

            if (response.Is(out Response<RegisterSuccesResult> sucess))
            {
                var data = sucess.Message;
                SetJwtCookie(httpContext, data.UserModel, _jwtProvider, _jwtOptions);

                _logger.LogInformation("New user registered: {Email} (ID: {UserId})", request.Email, data.Id);

                return new OkObjectResult(new { data.Id, data.UserName, data.Roles });
            }
            if (response.Is(out Response<ConflictRegisterResponse> error))
            {
                _logger.LogWarning("Registration conflict for {Email}: {Reason}", request.Email, error.Message.msg);
                return new ConflictObjectResult(error.Message.msg);
            }
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "Register", new { request.Email, request.UserName });
    }
}
