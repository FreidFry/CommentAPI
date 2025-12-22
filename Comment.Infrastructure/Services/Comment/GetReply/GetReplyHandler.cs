using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response;
using Comment.Infrastructure.Services.Comment.GetReply.Request;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment.GetReply
{
    public class GetReplyHandler : IGetReplyHandler
    {
        private readonly IRequestClient<GetReplyRequest> _client;
        public GetReplyHandler(IRequestClient<GetReplyRequest> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(GetReplyRequest request, CancellationToken cancellationToken)
        {
            var response = await _client.GetResponse<CommentsListResponse, StatusCodeResponse>(request, cancellationToken);

            if (response.Is(out Response<CommentsListResponse> comment)) return new OkObjectResult(comment.Message);
            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new StatusCodeResult(statusCode.Message.StatusCode);

            return new StatusCodeResult(500);
        }
    }
}
