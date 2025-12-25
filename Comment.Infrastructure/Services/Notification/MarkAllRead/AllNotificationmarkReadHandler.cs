using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Extensions;
using Comment.Infrastructure.Services.Notification.MarkAllRead.Request;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Notification.MarkAllRead
{
    public class AllNotificationmarkReadHandler : HandlerWrapper, IAllNotificationmarkReadHandler
    {
        private readonly IRequestClient<AllNotificationsMarkReadRequest> _client;

        public AllNotificationmarkReadHandler(IRequestClient<AllNotificationsMarkReadRequest> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(HttpContext httpContext, CancellationToken cancellationToken) => await SafeExecute(async () =>
        {
            var userId = ClaimsExtensions.GetCallerId(httpContext);
            var response = await _client.GetResponse<StatusCodeResponse>(new(userId), cancellationToken);

            if (response is Response<StatusCodeResponse> codeResponse) return new ObjectResult(new { codeResponse.Message.Message })
            {
                StatusCode = codeResponse.Message.StatusCode
            };
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        });
    }
}
