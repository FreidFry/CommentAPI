using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.User.GetProfile.Request;
using Comment.Infrastructure.Services.User.GetProfile.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.User.GetProfile
{
    public class GetProfileHandler : HandlerWrapper, IGetProfileHandler
    {
        private readonly IRequestClient<ProfileGetRequestDTO> _client;
        public GetProfileHandler(IRequestClient<ProfileGetRequestDTO> client, ILogger<GetProfileHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(ProfileGetRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var callerId = GetCallerId(httpContext);
            var dto = new ProfileGetRequestDTO(request.Id, callerId);

            var response = await _client.GetResponse<ProfileDetailedResponse, StatusCodeResponse, ValidatorErrorResponse>(dto);

            if (response.Is(out Response<ProfileDetailedResponse> profileDetail)) return new OkObjectResult(profileDetail.Message);
            if (response.Is(out Response<StatusCodeResponse> statusCode))
            {
                var code = statusCode.Message.StatusCode;
                _logger.LogWarning("Profile access for {TargetUserId} failed. Status: {Code}, Reason: {Message}",
                request.Id, code, statusCode.Message.Message);
                return new ObjectResult(new { statusCode.Message.Message })
                {
                    StatusCode = code
                };
            }
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);


            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "GetProfile", new { TargetUserId = request.Id, CallerId = GetCallerId(httpContext) });
    }
}
