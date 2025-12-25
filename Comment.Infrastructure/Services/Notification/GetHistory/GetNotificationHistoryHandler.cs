using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Notification.GetHistory.Request;
using Comment.Infrastructure.Services.Notification.GetHistory.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Comment.Infrastructure.Services.Notification.GetHistory
{
    public class GetNotificationHistoryHandler : HandlerWrapper, IGetNotificationHistoryHandler
    {
        private readonly IRequestClient<GetNotificationRequest> _client;

        public GetNotificationHistoryHandler(IRequestClient<GetNotificationRequest> client, ILogger<GetNotificationHistoryHandler> _logger) : base(_logger)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
       => await SafeExecute(async () =>
       {
           var callerId = ClaimsExtensions.GetCallerId(httpContext);

           _logger.LogDebug("Fetching notification history for User {UserId}", callerId);
           var response = await _client.GetResponse<GetNotificationResponse>(new(callerId), cancellationToken);

           if (response is Response<GetNotificationResponse> notifications) return new OkObjectResult(notifications.Message.Notifications);

           return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
       }, "GetNotificationHistory", new { UserId = ClaimsExtensions.GetCallerId(httpContext) });
    }
}
