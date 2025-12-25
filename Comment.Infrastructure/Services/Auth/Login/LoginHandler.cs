using Comment.Core.Interfaces;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Auth.DTOs;
using Comment.Infrastructure.Services.Auth.Login.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Comment.Infrastructure.Extensions.CookieExtensions;

namespace Comment.Infrastructure.Services.Auth.Login
{
    public class LoginHandler : HandlerWrapper, ILoginHandler
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IJwtOptions _jwtOptions;
        private readonly IRequestClient<UserLoginRequest> _client;
        public LoginHandler(IRequestClient<UserLoginRequest> client, IJwtOptions jwtOptions, IJwtProvider jwtProvider, ILogger<LoginHandler> _logger) : base(_logger)
        {
            _client = client;
            _jwtProvider = jwtProvider;
            _jwtOptions = jwtOptions;
        }

        public async Task<IActionResult> Handle(UserLoginRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
           {
               var response = await _client.GetResponse<LoginSuccesResponse, StatusCodeResponse>(request, cancellationToken);

               if (response.Is(out Response<LoginSuccesResponse> succes))
               {
                   var data = succes.Message;

                   SetJwtCookie(httpContext, data.UserModel, _jwtProvider, _jwtOptions);
                   _logger.LogInformation("User {UserName} successfully logged in", data.UserName);

                   return new OkObjectResult(new { data.Id, data.UserName, data.Roles });
               }

               if (response.Is(out Response<StatusCodeResponse> statusCode))
               {
                   _logger.LogWarning("Failed login attempt for user {Email}: {Reason}", request.Email, statusCode.Message.Message);
                   return new ObjectResult(new { statusCode.Message.Message })
                   {
                       StatusCode = statusCode.Message.StatusCode
                   };
               }
               return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
           }, "Login", new { Email = request.Email });
    }
}
