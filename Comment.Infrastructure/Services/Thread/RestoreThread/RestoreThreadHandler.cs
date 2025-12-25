using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.RestoreThread.Request;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services.Thread.RestoreThread
{
    public class RestoreThreadHandler : HandlerWrapper, IRestoreThreadHandler
    {
        private readonly IRequestClient<RestoreThreadRequestDTO> _client;
        public RestoreThreadHandler(IRequestClient<RestoreThreadRequestDTO> client, ILogger<RestoreThreadHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(RestoreThreadRequest request, HttpContext http, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var callerId = ClaimsExtensions.GetCallerId(http);
            var dto = new RestoreThreadRequestDTO(request.ThreadId, callerId);
            _logger.LogInformation("User {UserId} is attempting to restore Thread {ThreadId}", callerId, request.ThreadId);
            var response = await _client.GetResponse<StatusCodeResponse, ValidatorErrorResponse>(dto, cancellationToken);

            if (response.Is(out Response<StatusCodeResponse> statusCode))
            {
                var code = statusCode.Message.StatusCode;

                if (code >= 200 && code < 300)
                    _logger.LogInformation("Thread {ThreadId} successfully restored by User {UserId}", request.ThreadId, callerId);
                else
                    _logger.LogWarning("Restore failed for Thread {ThreadId}. Service returned {Code}: {Message}",
                        request.ThreadId, code, statusCode.Message.Message);
                return new ObjectResult(new { statusCode.Message.Message })
                {
                    StatusCode = statusCode.Message.StatusCode
                };
            }
            if (response.Is(out Response<ValidatorErrorResponse> e))
            {
                _logger.LogWarning("Validation failed while restoring Thread {ThreadId}: {Error}", request.ThreadId, e.Message.msg);
                return new BadRequestObjectResult(e.Message.msg);
            }

            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "RestoreThread", new { request.ThreadId, UserId = ClaimsExtensions.GetCallerId(http) });
    }
}
