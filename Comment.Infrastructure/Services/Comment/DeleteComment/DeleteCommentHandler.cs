using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Comment.DeleteComment
{
    public class DeleteCommentHandler : HandlerWrapper, IDeleteCommentHandler
    {
        private readonly IRequestClient<DeleteCommentRequestDTO> _client;
        public DeleteCommentHandler(IRequestClient<DeleteCommentRequestDTO> client)
        {
            _client = client;
        }
        public async Task<IActionResult> Handle(DeleteCommentRequest request, HttpContext http, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var callerId = GetCallerId(http);
            var dto = new DeleteCommentRequestDTO(request.CommentId, callerId);
            var response = await _client.GetResponse<StatusCodeResponse, ValidatorErrorResponse>(dto);

            if (response.Is(out Response<StatusCodeResponse> statusCodeResponse)) return new ObjectResult(new { statusCodeResponse.Message.Message })
            {
                StatusCode = statusCodeResponse.Message.StatusCode
            }; ;
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        });
    }
}
