using System.Net.WebSockets;

namespace automobile_backend.Services
{
    public interface IWebSocketService
    {
        Task HandleWebSocketAsync(WebSocket webSocket, string userId);
        Task SendMessageToUserAsync(string userId, string message);
        Task SendAppointmentNotificationAsync(string customerId, string appointmentId, string status, string message);
        Task BroadcastAppointmentUpdateAsync(string appointmentId, string status, string message);
        bool IsUserConnected(string userId);
    }
}
