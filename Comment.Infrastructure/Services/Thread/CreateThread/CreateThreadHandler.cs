using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.CreateThread.Request;
using Comment.Infrastructure.Services.Thread.CreateThread.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.CreateThread
{
    public class CreateThreadHandler : HandlerWrapper, ICreateThreadHandler
    {
        private readonly IRequestClient<ThreadCreateRequestDTO> _client;
        public CreateThreadHandler(IRequestClient<ThreadCreateRequestDTO> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(ThreadCreateRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var callerId = ClaimsExtensions.GetCallerId(httpContext);
            var dto = new ThreadCreateRequestDTO(request.Title, request.Context, callerId);
            var response = await _client.GetResponse<ThreadCreateSuccess, StatusCodeResponse>(dto, cancellationToken);

            if (response.Is(out Response<ThreadCreateSuccess> thread)) return new RedirectResult($"/threads/{thread.Message.threadId}");
            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new ObjectResult(new { statusCode.Message.Message })
            {
                StatusCode = statusCode.Message.StatusCode
            };

            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        });
    }
}
