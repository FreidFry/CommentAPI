using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Thread.DTOs.Request;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Request;
using Comment.Infrastructure.Services.Thread.GetDetailedThread.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.GetDetailedThread
{
    public class GetDetailedThreadHandler : HandlerWrapper, IGetDetailedThreadHandler
    {
        private readonly IRequestClient<ThreadDetaliRequestDTO> _client;
        public GetDetailedThreadHandler(IRequestClient<ThreadDetaliRequestDTO> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(ThreadDetaliRequest request, HttpContext httpContext, CancellationToken cancellationToken)
       => await SafeExecute(async () =>
       {
           var callerId = ClaimsExtensions.GetCallerId(httpContext);
           var dto = new ThreadDetaliRequestDTO(request.ThreadId, callerId);
           var response = await _client.GetResponse<DetailedThreadResponse, ValidatorErrorResponse, JsonResponse>(dto, cancellationToken);

           if (response.Is(out Response<DetailedThreadResponse> tree)) return new OkObjectResult(tree.Message);
           if (response.Is(out Response<JsonResponse> json)) return new OkObjectResult(json.Message.json);
           if (response.Is(out Response<StatusCodeResponse> StatusCode)) return new ObjectResult(new { StatusCode.Message.Message })
           {
               StatusCode = StatusCode.Message.StatusCode
           };

           return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
       });
    }
}
