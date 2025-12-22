using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.UpdateThread.Request;
using Comment.Infrastructure.Services.Thread.UpdateThread.Response;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.UpdateThread
{
    public class UpdateThreadHandler : IUpdateThreadHandler
    {
        private readonly IRequestClient<UpdateThreadRequestDTO> _client;
        public UpdateThreadHandler(IRequestClient<UpdateThreadRequestDTO> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(UpdateThreadRequest request, HttpContext http, CancellationToken cancellationToken)
        {
            var callerId = ClaimsExtensions.GetCallerId(http);
            var dto = new UpdateThreadRequestDTO(request.ThreadId, request.Title, request.Context, callerId);
            var response = await _client.GetResponse<UpdateThreadSucces, StatusCodeResponse, ValidatorErrorResponse>(dto, cancellationToken);

            if (response.Is(out Response<UpdateThreadSucces> updateResponse)) return new OkObjectResult(updateResponse.Message);
            if (response.Is(out Response<StatusCodeResponse> stasusCode)) return new StatusCodeResult(stasusCode.Message.StatusCode);
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);

            return new StatusCodeResult(500);
        }
    }
}
