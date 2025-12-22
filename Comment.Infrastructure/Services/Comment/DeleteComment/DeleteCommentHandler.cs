using static Comment.Infrastructure.Extensions.ClaimsExtensions;
using Comment.Infrastructure.Services.Comment.DeleteComment.Request;
using Microsoft.AspNetCore.Http;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Comment.Infrastructure.CommonDTOs;

namespace Comment.Infrastructure.Services.Comment.DeleteComment
{
    public class DeleteCommentHandler : IDeleteCommentHandler
    {
        private readonly IRequestClient<DeleteCommentRequestDTO> _client;
        public DeleteCommentHandler(IRequestClient<DeleteCommentRequestDTO> client)
        {
            _client = client;
        }
        public async Task<IActionResult> Handle(DeleteCommentRequest request, HttpContext http, CancellationToken cancellationToken)
        {
            var callerId = GetCallerId(http);
            var dto = new DeleteCommentRequestDTO(request.CommentId, callerId);
            var response = await _client.GetResponse<StatusCodeResponse, ValidatorErrorResponse>(dto);

            if (response.Is(out Response<StatusCodeResponse> statusCodeResponse)) return new StatusCodeResult(statusCodeResponse.Message.StatusCode);
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);

            return new StatusCodeResult(500);
        }
    }
}
