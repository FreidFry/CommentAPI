using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Request;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.GetDetailedThread
{
    public class GetDetailedThreadHandler : IGetDetailedThreadHandler
    {
        private readonly IRequestClient<ThreadDetaliRequestDTO> _client;
        public GetDetailedThreadHandler(IRequestClient<ThreadDetaliRequestDTO> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(ThreadDetaliRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var callerId = ClaimsExtensions.GetCallerId(httpContext);
            var dto = new ThreadDetaliRequestDTO(request.ThreadId, callerId);
            var response = await _client.GetResponse<DetailedThreadResponse, StatusCodeResponse, ValidatorErrorResponse>(dto, cancellationToken);

            if (response.Is(out Response<DetailedThreadResponse> tree)) return new OkObjectResult(tree.Message);
            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new StatusCodeResult(statusCode.Message.StatusCode);
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);

            return new StatusCodeResult(500);
        }
    }
}
