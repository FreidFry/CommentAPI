using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.DeleteThread.Request;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services.Thread.DeleteThread
{
    public class DeleteThreadHandler : HandlerWrapper, IDeleteThreadHandler
    {
        private readonly IRequestClient<DeleteThreadRequestDTO> _client;
        public DeleteThreadHandler(IRequestClient<DeleteThreadRequestDTO> client, ILogger<DeleteThreadHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(DeleteThreadRequest request, HttpContext http, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var callerId = ClaimsExtensions.GetCallerId(http);
            var dto = new DeleteThreadRequestDTO(request.ThreadId, callerId);
            _logger.LogInformation("User {UserId} is requesting deletion of thread {ThreadId}.", callerId, request.ThreadId);
            var response = await _client.GetResponse<StatusCodeResponse, ValidatorErrorResponse>(dto, cancellationToken);

            if (response.Is(out Response<StatusCodeResponse> statusCode))
            {
                var code = statusCode.Message.StatusCode;

                if (code >= 200 && code < 300)
                    _logger.LogInformation("Thread {ThreadId} successfully deleted by user {UserId}.", request.ThreadId, callerId);
                else
                    _logger.LogWarning("Service refused to delete thread {ThreadId}. Status: {Status}, Message: {Msg}",
                        request.ThreadId, code, statusCode.Message.Message);

                return new ObjectResult(new { statusCode.Message.Message })
                {
                    StatusCode = code
                };
            }
            if (response.Is(out Response<ValidatorErrorResponse> e))
            {
                _logger.LogWarning("Validation failed for deleting thread {ThreadId}: {Errors}", request.ThreadId, e.Message.msg);
                return new BadRequestObjectResult(e.Message.msg);
            }

            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "DeleteThread", new { request.ThreadId, UserId = ClaimsExtensions.GetCallerId(http) });
    }
}
