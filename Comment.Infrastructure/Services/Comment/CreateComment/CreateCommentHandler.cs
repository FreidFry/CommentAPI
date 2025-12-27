using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Capcha.Validate;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.CreateComment.Response;
using Comment.Infrastructure.Wrappers;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Comment.CreateComment
{
    public class CreateCommentHandler : HandlerWrapper, ICreateCommentHandler
    {
        private readonly IRequestClient<CommentCreateRequestDTO> _client;
        private readonly IRedisDatabase _redisDatabase;
        private readonly IValidator<CommentCreateRequest> _validator;
        private readonly ICaptchaValidateHandler _captchaValidateHandler;
        public CreateCommentHandler(IRequestClient<CommentCreateRequestDTO> client, IRedisDatabase redisDatabase, IValidator<CommentCreateRequest> validator, ICaptchaValidateHandler captchaValidateHandler, ILogger<CreateCommentHandler> _logger) : base(_logger)
        {
            _client = client;
            _redisDatabase = redisDatabase;
            _validator = validator;
            _captchaValidateHandler = captchaValidateHandler;
        }

        public async Task<IActionResult> Handle(CommentCreateRequest request, HttpContext httpContext, CancellationToken cancellationToken)
            => await SafeExecute(async () =>
            {
                //var CaptchaResponse = await _captchaValidateHandler.Handle(new(request.CaptchaId, request.CaptchaValue));
                //if (!CaptchaResponse)
                //{
                //    _logger.LogWarning("Captcha validation failed for CaptchaId: {CaptchaId}", request.CaptchaId);
                //    return new BadRequestObjectResult("Captcha is not valid");
                //}

                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validation failed for comment in Thread {ThreadId}. Errors: {Errors}",
                    request.ThreadId, validationResult.Errors.Select(e => e.ErrorMessage));
                    return new BadRequestObjectResult(validationResult.Errors);
                }

                var callerId = GetCallerId(httpContext);
                if (callerId == null)
                {
                    _logger.LogWarning("Unauthorized attempt to create comment in Thread {ThreadId}", request.ThreadId);
                    return new UnauthorizedResult();
                }

                string? fileKey = null;

                if (request.FormFile != null)
                {
                    using var ms = new MemoryStream();
                    await request.FormFile.CopyToAsync(ms, cancellationToken);
                    var fileBytes = ms.ToArray();

                    fileKey = $"files/{Guid.NewGuid()}";

                    await _redisDatabase.AddAsync(fileKey, fileBytes, TimeSpan.FromMinutes(10));
                    _logger.LogDebug("File uploaded to Redis temporary storage. Key: {FileKey}, Size: {Size} bytes", fileKey, fileBytes.Length);
                }

                var dto = new CommentCreateRequestDTO(
                    callerId.Value,
                    request.Content,
                    request.ThreadId,
                    request.ParentCommentId,
                    fileKey,
                    request.FormFile?.ContentType
                );

                var response = await _client.GetResponse<CreateCommentSuccesResponse, StatusCodeResponse>(dto, cancellationToken);

                if (response.Is(out Response<CreateCommentSuccesResponse> success))
                {
                    _logger.LogInformation("User {UserId} successfully created comment in Thread {ThreadId}", callerId, request.ThreadId);
                    return new ObjectResult(new())
                    {
                        StatusCode = 204
                    };
                }

                if (response.Is(out Response<StatusCodeResponse> error))
                {
                    _logger.LogError("Backend service returned error for comment creation: {ErrorMessage} (Status: {StatusCode})",
                    error.Message.Message, error.Message.StatusCode);
                    return new ObjectResult(new { error.Message.Message })
                    {
                        StatusCode = error.Message.StatusCode
                    };
                }
                return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
            }, "CreateComment", new { request.ThreadId, request.ParentCommentId });
    }
}
