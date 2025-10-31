using automobile_backend.Models.Entities;

namespace automobile_backend.Models.DTOs
{
    public class ServiceProgressDto
    {
        public int AppointmentId { get; set; }
        public string ServiceTitle { get; set; }
        public string CustomerName { get; set; }
        public AppointmentStatus Status { get; set; }
        public automobile_backend.Models.Entities.Type ServiceType { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public bool IsTimerActive { get; set; }
        public DateTime? CurrentTimerStartTime { get; set; }
        public decimal TotalTimeLogged { get; set; } // in hours
        public List<TimeLogDto> TimeLogs { get; set; } = new List<TimeLogDto>();
    }

    public class TimeLogDto
    {
        public int LogId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public decimal HoursLogged { get; set; }
        public bool IsActive { get; set; }
    }

    public class TimerActionDto
    {
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
    }

    public class UpdateStatusDto
    {
        public int AppointmentId { get; set; }
        public AppointmentStatus NewStatus { get; set; }
        public int UserId { get; set; }
        public string? Notes { get; set; }
    }

    public class TimerResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public TimeLogDto? ActiveTimeLog { get; set; }
        public decimal TotalTimeLogged { get; set; }
    }
}