using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Notification.MarkAllRead.Request;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Notification.MarkAllRead
{
    public class AllNotificationmarkReadHandler : IAllNotificationmarkReadHandler
    {
        private readonly IRequestClient<AllNotificationsMarkReadRequest> _client;

        public AllNotificationmarkReadHandler(IRequestClient<AllNotificationsMarkReadRequest> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var userId = ClaimsExtensions.GetCallerId(httpContext);
            var response = await _client.GetResponse<StatusCodeResponse>(new(userId), cancellationToken);

            if (response is Response<StatusCodeResponse> codeResponse) return new StatusCodeResult(codeResponse.Message.StatusCode);
            return new StatusCodeResult(500);
        }
    }
}
