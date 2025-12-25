using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.User.GetProfile.Request;
using Comment.Infrastructure.Services.User.GetProfile.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.User.GetProfile
{
    public class GetProfileHandler : HandlerWrapper, IGetProfileHandler
    {
        private readonly IRequestClient<ProfileGetRequestDTO> _client;
        public GetProfileHandler(IRequestClient<ProfileGetRequestDTO> client)
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
            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new ObjectResult(new { statusCode.Message.Message })
            {
                StatusCode = statusCode.Message.StatusCode
            };
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);


            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        });
    }
}
