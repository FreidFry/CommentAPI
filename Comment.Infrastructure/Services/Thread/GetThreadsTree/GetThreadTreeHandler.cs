using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Thread.GetThreadsTree.Request;
using Comment.Infrastructure.Services.Thread.GetThreadsTree.Response;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Thread.GetThreadsTree
{
    public class GetThreadTreeHandler : IGetThreadTreeHandler
    {
        private readonly IRequestClient<ThreadsThreeRequest> _client;
        public GetThreadTreeHandler(IRequestClient<ThreadsThreeRequest> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(ThreadsThreeRequest dto, CancellationToken cancellationToken)
        {
            var response = await _client.GetResponse<ThreadsTreeResponse, StatusCodeResponse>(dto, cancellationToken);

            if (response.Is(out Response<ThreadsTreeResponse> tree)) return new OkObjectResult(tree.Message);
            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new StatusCodeResult(statusCode.Message.StatusCode);

            return new StatusCodeResult(500);
        }
    }
}
