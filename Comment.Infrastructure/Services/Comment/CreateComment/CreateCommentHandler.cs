using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.Comment.CreateComment.Request;
using Comment.Infrastructure.Services.Comment.CreateComment.Response;
using FluentValidation;
using MassTransit;
using MassTransit.MessageData.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis.Extensions.Core.Abstractions;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.Comment.CreateComment
{
    public class CreateCommentHandler : ICreateCommentHandler
    {
        private readonly IRequestClient<CommentCreateRequestDTO> _client;
        private readonly IRedisDatabase _redisDatabase;
        private readonly IValidator<CommentCreateRequest> _validator;
        public CreateCommentHandler(IRequestClient<CommentCreateRequestDTO> client, IRedisDatabase redisDatabase, IValidator<CommentCreateRequest> validator)
        {
            _client = client;
            _redisDatabase = redisDatabase;
            _validator = validator;
        }

        public async Task<IActionResult> CreateCommentHandleAsync(CommentCreateRequest request, HttpContext httpContext, CancellationToken cancellationToken)
        {
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

            if (response.Is(out Response<CreateCommentSuccesResponse> success))
            {
                return new OkResult();
            }

            if (response.Is(out Response<StatusCodeResponse> error))
            {
                return new BadRequestObjectResult(error.Message);
            }

            return new BadRequestResult();
        }
    }
}
