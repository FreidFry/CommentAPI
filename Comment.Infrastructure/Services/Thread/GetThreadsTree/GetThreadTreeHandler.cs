using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Thread.GetThreadsTree.Request;
using Comment.Infrastructure.Services.Thread.GetThreadsTree.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services.Thread.GetThreadsTree
{
    public class GetThreadTreeHandler : HandlerWrapper, IGetThreadTreeHandler
    {
        private readonly IRequestClient<ThreadsThreeRequest> _client;
        public GetThreadTreeHandler(IRequestClient<ThreadsThreeRequest> client, ILogger<GetThreadTreeHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(ThreadsThreeRequest dto, CancellationToken cancellationToken)
       => await SafeExecute(async () =>
       {
           var response = await _client.GetResponse<ThreadsTreeResponse, StatusCodeResponse>(dto, cancellationToken);

           if (response.Is(out Response<ThreadsTreeResponse> tree)) return new OkObjectResult(tree.Message);
           if (response.Is(out Response<StatusCodeResponse> statusCode))
           {
               _logger.LogWarning("Failed to retrieve threads tree. Service returned {Code}: {Message}",
                   statusCode.Message.StatusCode, statusCode.Message.Message);
               return new ObjectResult(new { statusCode.Message.Message })
               {
                   StatusCode = statusCode.Message.StatusCode
               };
           }

           return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
       }, "GetThreadsTree", dto);
    }
}
