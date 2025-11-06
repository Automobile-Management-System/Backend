// automobile_backend/Hubs/AdminNotificationHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace automobile_backend.Hubs
{
    [Authorize(Roles = "Admin")]
    public class AdminNotificationHub : Hub
    {
        public const string AdminGroup = "Admins";

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);
            await base.OnDisconnectedAsync(exception);
        }
    }
}