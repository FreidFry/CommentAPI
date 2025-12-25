using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Notification.MarkRead.Request;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services.Notification.MarkRead
{
    public class NotificationMarkReadHandler : HandlerWrapper, INotificationMarkReadHandler
    {
        private readonly IRequestClient<NotificationMarkReadRequest> _client;

        public NotificationMarkReadHandler(IRequestClient<NotificationMarkReadRequest> client, ILogger<NotificationMarkReadHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(Guid id, HttpContext httpContext, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var userId = ClaimsExtensions.GetCallerId(httpContext);
            var request = new NotificationMarkReadRequest(id, userId);
            _logger.LogDebug("User {UserId} is marking notification {NotificationId} as read.", userId, id);
            var response = await _client.GetResponse<StatusCodeResponse>(request, cancellationToken);

            if (response is Response<StatusCodeResponse> codeResponse)
            {
                var statusCode = codeResponse.Message.StatusCode;

                if (statusCode >= 200 && statusCode < 300)
                    _logger.LogTrace("Notification {NotificationId} marked as read for user {UserId}.", id, userId);
                else
                    _logger.LogWarning("Failed to mark notification {NotificationId} as read. Service returned {Code}: {Message}",
                        id, statusCode, codeResponse.Message);
                return new ObjectResult(new { codeResponse.Message.Message })
                {
                    StatusCode = codeResponse.Message.StatusCode
                };
            }
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        }, "MarkNotificationRead", new { NotificationId = id, UserId = ClaimsExtensions.GetCallerId(httpContext) });
    }
}
