using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Notification.GetHistory.Request;
using Comment.Infrastructure.Services.Notification.GetHistory.Response;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Notification.GetHistory
{
    public class GetNotificationHistoryHandler : IGetNotificationHistoryHandler
    {
        private readonly IRequestClient<GetNotificationRequest> _client;

        public GetNotificationHistoryHandler(IRequestClient<GetNotificationRequest> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var callerId = ClaimsExtensions.GetCallerId(httpContext);

            var response = await _client.GetResponse<GetNotificationResponse>(new(callerId), cancellationToken);

            if (response is Response<GetNotificationResponse> notifications) return new OkObjectResult(notifications.Message.Notifications);

            return new StatusCodeResult(500);
        }
    }
}
