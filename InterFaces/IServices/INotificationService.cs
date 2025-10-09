using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface INotificationService
    {
        Task SendAppointmentReminderAsync(int appointmentId);
        Task SendStatusUpdateAsync(string entityType, int entityId, string newStatus);
        Task SendRealTimeUpdateAsync(string userId, object payload); // For WebSockets
    }
}
