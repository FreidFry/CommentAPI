using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Request;
using Comment.Infrastructure.Services.Comment.GetCommentsByThread.Response;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Comment.GetCommentsByThread
{
    public class GetCommentsByThreadHandler : IGetCommentsByThreadHandler
    {
        private readonly IRequestClient<CommentsByThreadRequestDTO> _client;
        public GetCommentsByThreadHandler(IRequestClient<CommentsByThreadRequestDTO> client)
        {
            _client = client;
        }

        public async Task<IActionResult> GetCommentsByThreadHandle(Guid threadId, CommentsByThreadRequest request, CancellationToken cancellationToken)
        {
            var dto = new CommentsByThreadRequestDTO(threadId, request);
            var response = await _client.GetResponse<CommentsByThreadRequestDTO, CommentsListResponse>(dto, cancellationToken);

            if (response.Is(out Response<CommentsListResponse> comments))
            {
                return new OkObjectResult(comments.Message);
            }

            return new StatusCodeResult(500);
        }
    }
}
