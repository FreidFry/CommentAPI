using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response;
using Comment.Infrastructure.Services.Comment.GetReply.Request;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment.GetReply
{
    public class GetReplyHandler : HandlerWrapper, IGetReplyHandler
    {
        private readonly IRequestClient<GetReplyRequest> _client;
        public GetReplyHandler(IRequestClient<GetReplyRequest> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(GetReplyRequest request, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var response = await _client.GetResponse<CommentsListResponse, StatusCodeResponse>(request, cancellationToken);

            if (response.Is(out Response<CommentsListResponse> comment)) return new OkObjectResult(comment.Message);
            if (response.Is(out Response<StatusCodeResponse> statusCode)) return new ObjectResult(new { statusCode.Message.Message })
            {
                StatusCode = statusCode.Message.StatusCode
            };
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        });
    }
}
