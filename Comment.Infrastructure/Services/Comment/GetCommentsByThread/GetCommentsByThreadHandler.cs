using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Request;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread
{
    public class GetCommentsByThreadHandler : HandlerWrapper, IGetCommentsByThreadHandler
    {
        private readonly IRequestClient<CommentsByThreadRequestDTO> _client;
        public GetCommentsByThreadHandler(IRequestClient<CommentsByThreadRequestDTO> client, ILogger<GetCommentsByThreadHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(Guid threadId, CommentsByThreadRequest request, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var dto = new CommentsByThreadRequestDTO(threadId, request);
            var response = await _client.GetResponse<CommentsByThreadRequestDTO, CommentsListResponse>(dto, cancellationToken);

            if (response.Is(out Response<CommentsListResponse> comments))
                return new OkObjectResult(comments.Message);
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "GetCommentsByThread", new { threadId, request.SortBy });
    }
}
