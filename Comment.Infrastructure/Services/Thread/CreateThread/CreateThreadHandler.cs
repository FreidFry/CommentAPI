using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.CreateThread.Request;
using Comment.Infrastructure.Services.Thread.CreateThread.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services.Thread.CreateThread
{
    public class CreateThreadHandler : HandlerWrapper, ICreateThreadHandler
    {
        private readonly IRequestClient<ThreadCreateRequestDTO> _client;
        public CreateThreadHandler(IRequestClient<ThreadCreateRequestDTO> client, ILogger<CreateThreadHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(ThreadCreateRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var callerId = ClaimsExtensions.GetCallerId(httpContext);
            var dto = new ThreadCreateRequestDTO(request.Title, request.Context, callerId);
            _logger.LogInformation("User {UserId} is creating a new thread: {Title}", callerId, request.Title);
            var response = await _client.GetResponse<ThreadCreateSuccess, StatusCodeResponse>(dto, cancellationToken);

            if (response.Is(out Response<ThreadCreateSuccess> thread))
            {
                var newThreadId = thread.Message.threadId;
                _logger.LogInformation("Thread successfully created. ID: {ThreadId}, Owner: {UserId}", newThreadId, callerId);
                return new RedirectResult($"/threads/{newThreadId}");
            }
            if (response.Is(out Response<StatusCodeResponse> statusCode))
            {
                _logger.LogWarning("Failed to create thread. Service returned {Code}: {Message}. User: {UserId}",
                    statusCode.Message.StatusCode, statusCode.Message.Message, callerId);
                return new ObjectResult(new { statusCode.Message.Message })
                {
                    StatusCode = statusCode.Message.StatusCode
                };
            }
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "CreateThread", new { Title = request.Title, UserId = ClaimsExtensions.GetCallerId(httpContext) });
    }
}
