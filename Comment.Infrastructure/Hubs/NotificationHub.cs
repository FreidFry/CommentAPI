using Comment.Core.Data;
using Microsoft.AspNetCore.SignalR;

namespace Comment.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
