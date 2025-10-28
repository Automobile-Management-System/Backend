using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.Services
{
    public class ServiceProgressService : IServiceProgressService
    {
        private readonly IServiceProgressRepository _repository;

        public ServiceProgressService(IServiceProgressRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ServiceProgressDto>> GetEmployeeServiceProgressAsync(int employeeId)
        {
            var appointments = await _repository.GetEmployeeAppointmentsAsync(employeeId);
            var serviceProgressList = new List<ServiceProgressDto>();

            foreach (var appointment in appointments)
            {
                var activeTimer = await GetActiveTimerAsync(appointment.AppointmentId, employeeId);
                var totalTime = await GetTotalLoggedTimeAsync(appointment.AppointmentId);

                var serviceProgress = new ServiceProgressDto
                {
                    AppointmentId = appointment.AppointmentId,
                    ServiceTitle = GetServiceTitle(appointment),
                    CustomerName = appointment.User?.FirstName + " " + appointment.User?.LastName,
                    Status = appointment.Status,
                    ServiceType = appointment.Type,
                    AppointmentDateTime = appointment.DateTime,
                    IsTimerActive = activeTimer != null,
                    CurrentTimerStartTime = activeTimer?.StartDateTime,
                    TotalTimeLogged = totalTime,
                    TimeLogs = appointment.TimeLogs?.Select(tl => new TimeLogDto
                    {
                        LogId = tl.LogId,
                        StartDateTime = tl.StartDateTime,
                        EndDateTime = tl.EndDateTime,
                        HoursLogged = tl.HoursLogged,
                        IsActive = tl.IsActive
                    }).ToList() ?? new List<TimeLogDto>()
                };

                serviceProgressList.Add(serviceProgress);
            }

            return serviceProgressList;
        }

        public async Task<ServiceProgressDto?> GetServiceProgressByIdAsync(int appointmentId)
        {
            var appointment = await _repository.GetAppointmentWithDetailsAsync(appointmentId);
            if (appointment == null) return null;

            var activeTimer = appointment.TimeLogs?.FirstOrDefault(tl => tl.IsActive);
            var totalTime = await GetTotalLoggedTimeAsync(appointmentId);

            return new ServiceProgressDto
            {
                AppointmentId = appointment.AppointmentId,
                ServiceTitle = GetServiceTitle(appointment),
                CustomerName = appointment.User?.FirstName + " " + appointment.User?.LastName,
                Status = appointment.Status,
                ServiceType = appointment.Type,
                AppointmentDateTime = appointment.DateTime,
                IsTimerActive = activeTimer != null,
                CurrentTimerStartTime = activeTimer?.StartDateTime,
                TotalTimeLogged = totalTime,
                TimeLogs = appointment.TimeLogs?.Select(tl => new TimeLogDto
                {
                    LogId = tl.LogId,
                    StartDateTime = tl.StartDateTime,
                    EndDateTime = tl.EndDateTime,
                    HoursLogged = tl.HoursLogged,
                    IsActive = tl.IsActive
                }).ToList() ?? new List<TimeLogDto>()
            };
        }

        public async Task<TimerResponseDto> StartTimerAsync(TimerActionDto timerAction)
        {
            try
            {
                // Check if there's already an active timer for this appointment and user
                var existingTimer = await _repository.GetActiveTimerAsync(timerAction.AppointmentId, timerAction.UserId);
                if (existingTimer != null)
                {
                    return new TimerResponseDto
                    {
                        Success = false,
                        Message = "Timer is already running for this appointment.",
                        ActiveTimeLog = new TimeLogDto
                        {
                            LogId = existingTimer.LogId,
                            StartDateTime = existingTimer.StartDateTime,
                            EndDateTime = existingTimer.EndDateTime,
                            HoursLogged = existingTimer.HoursLogged,
                            IsActive = existingTimer.IsActive
                        }
                    };
                }

                // Create new time log with active timer
                var timeLog = new TimeLog
                {
                    AppointmentId = timerAction.AppointmentId,
                    UserId = timerAction.UserId,
                    StartDateTime = DateTime.UtcNow,
                    IsActive = true,
                    HoursLogged = 0
                };

                var createdTimeLog = await _repository.CreateTimeLogAsync(timeLog);
                var totalTime = await GetTotalLoggedTimeAsync(timerAction.AppointmentId);

                return new TimerResponseDto
                {
                    Success = true,
                    Message = "Timer started successfully.",
                    ActiveTimeLog = new TimeLogDto
                    {
                        LogId = createdTimeLog.LogId,
                        StartDateTime = createdTimeLog.StartDateTime,
                        EndDateTime = createdTimeLog.EndDateTime,
                        HoursLogged = createdTimeLog.HoursLogged,
                        IsActive = createdTimeLog.IsActive
                    },
                    TotalTimeLogged = totalTime
                };
            }
            catch (Exception ex)
            {
                return new TimerResponseDto
                {
                    Success = false,
                    Message = $"Error starting timer: {ex.Message}"
                };
            }
        }

        public async Task<TimerResponseDto> PauseTimerAsync(TimerActionDto timerAction)
        {
            try
            {
                var activeTimer = await _repository.GetActiveTimerAsync(timerAction.AppointmentId, timerAction.UserId);
                if (activeTimer == null)
                {
                    return new TimerResponseDto
                    {
                        Success = false,
                        Message = "No active timer found for this appointment."
                    };
                }

                // Calculate hours logged
                var timeElapsed = DateTime.UtcNow - activeTimer.StartDateTime;
                activeTimer.EndDateTime = DateTime.UtcNow;
                activeTimer.HoursLogged = (decimal)timeElapsed.TotalHours;
                activeTimer.IsActive = false;

                await _repository.UpdateTimeLogAsync(activeTimer);
                var totalTime = await GetTotalLoggedTimeAsync(timerAction.AppointmentId);

                return new TimerResponseDto
                {
                    Success = true,
                    Message = "Timer paused successfully.",
                    ActiveTimeLog = new TimeLogDto
                    {
                        LogId = activeTimer.LogId,
                        StartDateTime = activeTimer.StartDateTime,
                        EndDateTime = activeTimer.EndDateTime,
                        HoursLogged = activeTimer.HoursLogged,
                        IsActive = activeTimer.IsActive
                    },
                    TotalTimeLogged = totalTime
                };
            }
            catch (Exception ex)
            {
                return new TimerResponseDto
                {
                    Success = false,
                    Message = $"Error pausing timer: {ex.Message}"
                };
            }
        }

        public async Task<TimerResponseDto> StopTimerAsync(TimerActionDto timerAction)
        {
            try
            {
                var activeTimer = await _repository.GetActiveTimerAsync(timerAction.AppointmentId, timerAction.UserId);
                if (activeTimer == null)
                {
                    return new TimerResponseDto
                    {
                        Success = false,
                        Message = "No active timer found for this appointment."
                    };
                }

                // Calculate hours logged and stop timer
                var timeElapsed = DateTime.UtcNow - activeTimer.StartDateTime;
                activeTimer.EndDateTime = DateTime.UtcNow;
                activeTimer.HoursLogged = (decimal)timeElapsed.TotalHours;
                activeTimer.IsActive = false;

                await _repository.UpdateTimeLogAsync(activeTimer);

                // Update appointment status to completed if stopping timer
                var appointment = await _repository.GetAppointmentWithDetailsAsync(timerAction.AppointmentId);
                if (appointment != null)
                {
                    appointment.Status = AppointmentStatus.Completed;
                    appointment.EndDateTime = DateTime.UtcNow;
                    await _repository.UpdateAppointmentAsync(appointment);
                }

                var totalTime = await GetTotalLoggedTimeAsync(timerAction.AppointmentId);

                return new TimerResponseDto
                {
                    Success = true,
                    Message = "Timer stopped and service completed successfully.",
                    ActiveTimeLog = new TimeLogDto
                    {
                        LogId = activeTimer.LogId,
                        StartDateTime = activeTimer.StartDateTime,
                        EndDateTime = activeTimer.EndDateTime,
                        HoursLogged = activeTimer.HoursLogged,
                        IsActive = activeTimer.IsActive
                    },
                    TotalTimeLogged = totalTime
                };
            }
            catch (Exception ex)
            {
                return new TimerResponseDto
                {
                    Success = false,
                    Message = $"Error stopping timer: {ex.Message}"
                };
            }
        }

        public async Task<bool> UpdateServiceStatusAsync(UpdateStatusDto updateStatus)
        {
            try
            {
                var appointment = await _repository.GetAppointmentWithDetailsAsync(updateStatus.AppointmentId);
                if (appointment == null) return false;

                appointment.Status = updateStatus.NewStatus;

                // If setting to InProgress, set start time
                if (updateStatus.NewStatus == AppointmentStatus.InProgress && appointment.StartDateTime == default)
                {
                    appointment.StartDateTime = DateTime.UtcNow;
                }

                // If setting to Completed, set end time
                if (updateStatus.NewStatus == AppointmentStatus.Completed)
                {
                    appointment.EndDateTime = DateTime.UtcNow;

                    // Stop any active timers
                    var activeTimer = await _repository.GetActiveTimerAsync(updateStatus.AppointmentId, updateStatus.UserId);
                    if (activeTimer != null)
                    {
                        var timeElapsed = DateTime.UtcNow - activeTimer.StartDateTime;
                        activeTimer.EndDateTime = DateTime.UtcNow;
                        activeTimer.HoursLogged = (decimal)timeElapsed.TotalHours;
                        activeTimer.IsActive = false;
                        await _repository.UpdateTimeLogAsync(activeTimer);
                    }
                }

                await _repository.UpdateAppointmentAsync(appointment);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<TimeLogDto?> GetActiveTimerAsync(int appointmentId, int userId)
        {
            var activeTimer = await _repository.GetActiveTimerAsync(appointmentId, userId);
            if (activeTimer == null) return null;

            return new TimeLogDto
            {
                LogId = activeTimer.LogId,
                StartDateTime = activeTimer.StartDateTime,
                EndDateTime = activeTimer.EndDateTime,
                HoursLogged = activeTimer.HoursLogged,
                IsActive = activeTimer.IsActive
            };
        }

        public async Task<decimal> GetTotalLoggedTimeAsync(int appointmentId)
        {
            return await _repository.GetTotalLoggedTimeAsync(appointmentId);
        }

        private string GetServiceTitle(Appointment appointment)
        {
            var serviceType = appointment.Type == automobile_backend.Models.Entities.Type.Service ? "Service" : "Modifications";
            var vehicleInfo = appointment.CustomerVehicle?.Brand + " " + appointment.CustomerVehicle?.Model;
            return $"{serviceType} - {vehicleInfo}";
        }
    }
}