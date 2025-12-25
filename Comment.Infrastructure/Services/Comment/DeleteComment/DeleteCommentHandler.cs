using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Comment.DeleteComment
{
    public class DeleteCommentHandler : HandlerWrapper, IDeleteCommentHandler
    {
        private readonly IRequestClient<DeleteCommentRequestDTO> _client;
        public DeleteCommentHandler(IRequestClient<DeleteCommentRequestDTO> client, ILogger<DeleteCommentHandler> _logger) : base (_logger)
        {
            _client = client;
        }
        public async Task<IActionResult> Handle(DeleteCommentRequest request, HttpContext http, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var callerId = GetCallerId(http);
            var dto = new DeleteCommentRequestDTO(request.CommentId, callerId);
            _logger.LogDebug("Sending delete request for comment {CommentId} by user {UserId}", request.CommentId, callerId);
            var response = await _client.GetResponse<StatusCodeResponse, ValidatorErrorResponse>(dto);

            if (response.Is(out Response<StatusCodeResponse> statusCodeResponse))
            {
                var code = statusCodeResponse.Message.StatusCode;
                if (code >= 200 && code < 300)
                    _logger.LogInformation("Comment {CommentId} successfully deleted by user {UserId}", request.CommentId, callerId);
                else
                    _logger.LogWarning("Service refused to delete comment {CommentId}. Status: {Status}, Message: {Msg}",
                        request.CommentId, code, statusCodeResponse.Message.Message);

                return new ObjectResult(new { statusCodeResponse.Message.Message })
                {
                    StatusCode = statusCodeResponse.Message.StatusCode
                };
            }
            if (response.Is(out Response<ValidatorErrorResponse> e))
            {
                _logger.LogWarning("Validation failed for deleting comment {CommentId}: {Errors}", request.CommentId, e.Message.msg);
                return new BadRequestObjectResult(e.Message.msg);
            }
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "DeleteComment", new { request.CommentId });
    }
}
