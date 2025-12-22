using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.DeleteThread.Request;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.DeleteThread
{
    public class DeleteThreadHandler : IDeleteThreadHandler
    {
        private readonly IRequestClient<DeleteThreadRequestDTO> _client;
        public DeleteThreadHandler(IRequestClient<DeleteThreadRequestDTO> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(DeleteThreadRequest request, HttpContext http, CancellationToken cancellationToken)
        {
            var callerId = ClaimsExtensions.GetCallerId(http);
            var dto = new DeleteThreadRequestDTO(request.ThreadId, callerId);
            var response = await _client.GetResponse<StatusCodeResponse, ValidatorErrorResponse>(dto, cancellationToken);

            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new StatusCodeResult(statusCode.Message.StatusCode);
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);

            return new StatusCodeResult(500);
        }
    }
}
