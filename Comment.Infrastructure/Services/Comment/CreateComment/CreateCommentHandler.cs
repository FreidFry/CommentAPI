using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.CreateComment.Response;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Comment.CreateComment
{
    public class CreateCommentHandler : ICreateCommentHandler
    {
        private readonly IRequestClient<CommentCreateRequestDTO> _client;
        public CreateCommentHandler(IRequestClient<CommentCreateRequestDTO> client)
        {
            _client = client;
        }

        public async Task<IActionResult> CreateCommentHandleAsync(CommentCreateRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var callerId = GetCallerId(httpContext);
            if (callerId == null)
                return new UnauthorizedResult();
            var dto = new CommentCreateRequestDTO(callerId, request);
            var response = await _client.GetResponse<CommentCreateRequestDTO, CreateCommentSuccesResponse>(dto, cancellationToken);

            if (response.Is(out Response<CreateCommentSuccesResponse> success))
                return new OkResult();

            return new BadRequestResult();
        }
    }
}
