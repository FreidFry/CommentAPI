using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Notification.MarkRead.Request;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Notification.MarkRead
{
    public class NotificationMarkReadHandler : INotificationMarkReadHandler
    {
        private readonly IRequestClient<NotificationMarkReadRequest> _client;

        public NotificationMarkReadHandler(IRequestClient<NotificationMarkReadRequest> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(Guid id, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var userId = ClaimsExtensions.GetCallerId(httpContext);
            var request = new NotificationMarkReadRequest(id, userId);
            var response = await _client.GetResponse<StatusCodeResponse>(request, cancellationToken);

            if (response is Response<StatusCodeResponse> codeResponse) return new StatusCodeResult(codeResponse.Message.StatusCode);
            return new StatusCodeResult(500);
        }
    }
}
