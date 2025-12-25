using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.UpdateThread.Request;
using Comment.Infrastructure.Services.Thread.UpdateThread.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services.Thread.UpdateThread
{
    public class UpdateThreadHandler : HandlerWrapper, IUpdateThreadHandler
    {
        private readonly IRequestClient<UpdateThreadRequestDTO> _client;
        public UpdateThreadHandler(IRequestClient<UpdateThreadRequestDTO> client, ILogger<UpdateThreadHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(UpdateThreadRequest request, HttpContext http, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var callerId = ClaimsExtensions.GetCallerId(http);
            var dto = new UpdateThreadRequestDTO(request.ThreadId, request.Title, request.Context, callerId);
            _logger.LogDebug("User {UserId} is updating thread {ThreadId}. New Title: {Title}",
                callerId, request.ThreadId, request.Title);
            var response = await _client.GetResponse<UpdateThreadSucces, StatusCodeResponse, ValidatorErrorResponse>(dto, cancellationToken);

            if (response.Is(out Response<UpdateThreadSucces> updateResponse))
            {
                _logger.LogInformation("Thread {ThreadId} successfully updated by user {UserId}.",
                    request.ThreadId, callerId);
                return new OkObjectResult(updateResponse.Message);
            }
            if (response.Is(out Response<StatusCodeResponse> stasusCode))
            {
                var code = stasusCode.Message.StatusCode;
                _logger.LogWarning("Thread update refused for {ThreadId}. Status: {Code}, Reason: {Message}",
                    request.ThreadId, code, stasusCode.Message.Message);
                return new ObjectResult(new { stasusCode.Message.Message })
                {
                    StatusCode = stasusCode.Message.StatusCode
                };
            }
            if (response.Is(out Response<ValidatorErrorResponse> e))
            {
                _logger.LogWarning("Validation failed for updating thread {ThreadId}: {Error}",
                    request.ThreadId, e.Message.msg);
                return new BadRequestObjectResult(e.Message.msg);
            }

            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "UpdateThread", new { request.ThreadId, request.Title });
    }
}
