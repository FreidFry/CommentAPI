using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Notification.MarkAllRead.Request;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services.Notification.MarkAllRead
{
    public class AllNotificationmarkReadHandler : HandlerWrapper, IAllNotificationmarkReadHandler
    {
        private readonly IRequestClient<AllNotificationsMarkReadRequest> _client;

        public AllNotificationmarkReadHandler(IRequestClient<AllNotificationsMarkReadRequest> client, ILogger<AllNotificationmarkReadHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(HttpContext httpContext, CancellationToken cancellationToken) => await SafeExecute(async () =>
        {
            var userId = ClaimsExtensions.GetCallerId(httpContext);
            _logger.LogInformation("User {UserId} requested marking all notifications as read.", userId);
            var response = await _client.GetResponse<StatusCodeResponse>(new(userId), cancellationToken);

            if (response is Response<StatusCodeResponse> codeResponse)
            {
                var statusCode = codeResponse.Message.StatusCode;

                if (statusCode >= 200 && statusCode < 300)
                    _logger.LogInformation("Successfully marked all notifications as read for user {UserId}.", userId);
                else
                    _logger.LogWarning("Service returned non-success code {Code} for user {UserId} during mark-all-read.",
                        statusCode, userId);

                return new ObjectResult(new { codeResponse.Message.Message })
                {
                    StatusCode = codeResponse.Message.StatusCode
                };
            }
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "MarkAllNotificationsRead", new { UserId = ClaimsExtensions.GetCallerId(httpContext) });
    }
}
