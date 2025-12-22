using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.RestoreThread.Request;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.RestoreThread
{
    public class RestoreThreadHandler : IRestoreThreadHandler
    {
        private readonly IRequestClient<RestoreThreadRequestDTO> _client;
        public RestoreThreadHandler(IRequestClient<RestoreThreadRequestDTO> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(RestoreThreadRequest request, HttpContext http, CancellationToken cancellationToken)
        {
            var callerId = ClaimsExtensions.GetCallerId(http);
            var dto = new RestoreThreadRequestDTO(request.ThreadId, callerId);

            var response = await _client.GetResponse<StatusCodeResponse, ValidatorErrorResponse>(dto, cancellationToken);

            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new StatusCodeResult(statusCode.Message.StatusCode);
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);

            return new StatusCodeResult(500);
        }
    }
}
