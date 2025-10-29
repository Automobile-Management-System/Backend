using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IServices
{
    public interface IServiceProgressService
    {
        Task<IEnumerable<ServiceProgressDto>> GetEmployeeServiceProgressAsync(int employeeId);
        Task<ServiceProgressDto?> GetServiceProgressByIdAsync(int appointmentId);
        Task<TimerResponseDto> StartTimerAsync(TimerActionDto timerAction);
        Task<TimerResponseDto> PauseTimerAsync(TimerActionDto timerAction);
        Task<TimerResponseDto> StopTimerAsync(TimerActionDto timerAction);
        Task<bool> UpdateServiceStatusAsync(UpdateStatusDto updateStatus);
        Task<TimeLogDto?> GetActiveTimerAsync(int appointmentId, int userId);
        Task<decimal> GetTotalLoggedTimeAsync(int appointmentId);
    }
}