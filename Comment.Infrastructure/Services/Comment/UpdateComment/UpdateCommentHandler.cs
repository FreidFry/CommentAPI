using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.DTOs.Request;
using Comment.Infrastructure.Services.Comment.DTOs.Response;
using Comment.Infrastructure.Services.Comment.UpdateComment.Request;
using Comment.Infrastructure.Wrappers;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Comment.UpdateComment
{
    public class UpdateCommentHandler : HandlerWrapper, IUpdateCommentHandler
    {
        private readonly IRequestClient<CommentUpdateRequestDTO> _client;
        private readonly IValidator<CommentUpdateRequest> _validator;
        public UpdateCommentHandler(IValidator<CommentUpdateRequest> validator, IRequestClient<CommentUpdateRequestDTO> client, ILogger<UpdateCommentHandler> _logger) : base(_logger)
        {
            _validator = validator;
            _client = client;
        }

        public async Task<IActionResult> Handle(CommentUpdateRequest request, HttpContext http, CancellationToken cancellationToken)
       => await SafeExecute(async () =>
       {
           _logger.LogDebug("User is attempting to update comment {CommentId}", request.CommentId);
           var validationResult = await _validator.ValidateAsync(request, cancellationToken);
           if (!validationResult.IsValid)
           {
               _logger.LogWarning("Update validation failed for comment {CommentId}. Errors: {Errors}",
                   request.CommentId, validationResult.Errors.Select(e => e.ErrorMessage));
               return new BadRequestObjectResult(validationResult.Errors);
           }

           var callerId = GetCallerId(http);
           var dto = new CommentUpdateRequestDTO(callerId, request.CommentId, request.Content);
           var response = await _client.GetResponse<CommentViewModel, StatusCodeResponse, ValidatorErrorResponse>(dto);

           if (response.Is(out Response<CommentViewModel>? comment) && comment != null)
           {
               _logger.LogInformation("Comment {CommentId} successfully updated by user {UserId}",
                   request.CommentId, callerId);
               return new OkObjectResult(comment.Message);
           }
           if (response.Is(out Response<StatusCodeResponse> error))
           {
               _logger.LogWarning("Service refused update for comment {CommentId}. User: {UserId}, Status: {StatusCode}, Reason: {Reason}",
                   request.CommentId, callerId, error.Message.StatusCode, error.Message.Message);
               return new ObjectResult(new { error.Message.Message })
               {
                   StatusCode = error.Message.StatusCode
               };
           }
           if (response.Is(out Response<ValidatorErrorResponse> e))
           {
               _logger.LogWarning("Backend validator rejected update for comment {CommentId}: {Msg}",
                   request.CommentId, e.Message.msg);
               return new BadRequestObjectResult(e.Message.msg);
           }
           return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
       }, "UpdateComment", new { request.CommentId });
    }
}
