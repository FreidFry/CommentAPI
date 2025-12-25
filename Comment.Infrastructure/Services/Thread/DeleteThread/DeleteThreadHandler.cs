using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.DeleteThread.Request;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.DeleteThread
{
    public class DeleteThreadHandler : HandlerWrapper, IDeleteThreadHandler
    {
        private readonly IRequestClient<DeleteThreadRequestDTO> _client;
        public DeleteThreadHandler(IRequestClient<DeleteThreadRequestDTO> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(DeleteThreadRequest request, HttpContext http, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var callerId = ClaimsExtensions.GetCallerId(http);
            var dto = new DeleteThreadRequestDTO(request.ThreadId, callerId);
            var response = await _client.GetResponse<StatusCodeResponse, ValidatorErrorResponse>(dto, cancellationToken);

            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new ObjectResult(new { statusCode.Message.Message })
            {
                StatusCode = statusCode.Message.StatusCode
            };
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);

            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        });
    }
}
