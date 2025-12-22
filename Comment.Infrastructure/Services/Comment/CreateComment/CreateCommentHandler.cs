using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Capcha.Validate;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.CreateComment.Response;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis.Extensions.Core.Abstractions;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Comment.CreateComment
{
    public class CreateCommentHandler : ICreateCommentHandler
    {
        private readonly IRequestClient<CommentCreateRequestDTO> _client;
        private readonly IRedisDatabase _redisDatabase;
        private readonly IValidator<CommentCreateRequest> _validator;
        private readonly ICaptchaValidateHandler _captchaValidateHandler;
        public CreateCommentHandler(IRequestClient<CommentCreateRequestDTO> client, IRedisDatabase redisDatabase, IValidator<CommentCreateRequest> validator, ICaptchaValidateHandler captchaValidateHandler)
        {
            _client = client;
            _redisDatabase = redisDatabase;
            _validator = validator;
            _captchaValidateHandler = captchaValidateHandler;
        }

        public async Task<IActionResult> Handle(CommentCreateRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var CaptchaResponse = await _captchaValidateHandler.Handle(new(request.CaptchaId, request.CaptchaValue));
            if (!CaptchaResponse) return new BadRequestObjectResult("Captcha is not valid");

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var callerId = GetCallerId(httpContext);
            if (callerId == null)
                return new UnauthorizedResult();

            string? fileKey = null;

            if (request.FormFile != null)
            {
                using var ms = new MemoryStream();
                await request.FormFile.CopyToAsync(ms, cancellationToken);
                var fileBytes = ms.ToArray();

                fileKey = $"files/{Guid.NewGuid()}";

                await _redisDatabase.AddAsync(fileKey, fileBytes, TimeSpan.FromMinutes(10));
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

            if (response.Is(out Response<CreateCommentSuccesResponse> success)) return new StatusCodeResult(204);

            if (response.Is(out Response<StatusCodeResponse> error)) return new BadRequestObjectResult(error.Message.Message);

            return new BadRequestResult();
        }
    }
}
