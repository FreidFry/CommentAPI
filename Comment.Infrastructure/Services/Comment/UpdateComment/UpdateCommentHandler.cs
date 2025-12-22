using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Comment.Infrastructure.Services.Comment.UpdateComment.Request;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Comment.UpdateComment
{
    public class UpdateCommentHandler : IUpdateCommentHandler
    {
        private readonly IRequestClient<CommentUpdateRequestDTO> _client;
        private readonly IValidator<CommentUpdateRequest> _validator;
        public UpdateCommentHandler(IValidator<CommentUpdateRequest> validator, IRequestClient<CommentUpdateRequestDTO> client)
        {
            _validator = validator;
            _client = client;
        }

        public async Task<IActionResult> Handle(CommentUpdateRequest request, HttpContext http, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var callerId = GetCallerId(http);
            var dto = new CommentUpdateRequestDTO(callerId, request.CommentId, request.Content);
            var response = await _client.GetResponse<CommentViewModel, StatusCodeResponse, ValidatorErrorResponse>(dto);

            if (response.Is(out Response<CommentViewModel>? comment) && comment != null) return new OkObjectResult(comment.Message);
            if (response.Is(out Response<StatusCodeResponse> error)) return new StatusCodeResult(error.Message.StatusCode);
            if (response.Is(out Response<ValidatorErrorResponse> e)) return new BadRequestObjectResult(e.Message.msg);
            return new StatusCodeResult(500);
        }
    }
}
