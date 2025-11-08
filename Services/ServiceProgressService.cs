using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.Services
{
    public class ServiceProgressService : IServiceProgressService
    {
        private readonly IServiceProgressRepository _repository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly INotificationService _notifications; // NEW

        public ServiceProgressService(
            IServiceProgressRepository repository,
            IPaymentRepository paymentRepository,
            INotificationService notifications) // NEW
        {
            _repository = repository;
            _paymentRepository = paymentRepository;
            _notifications = notifications; // NEW
        }

        // --- MODIFIED: This is now a simple passthrough ---
        public async Task<IEnumerable<ServiceProgressDto>> GetEmployeeServiceProgressAsync(int employeeId)
        {
            // The repository now does all the heavy lifting! No more N+1 loops.
            return await _repository.GetEmployeeServiceProgressAsync(employeeId);
        }

        public async Task<ServiceProgressDto?> GetServiceProgressByIdAsync(int appointmentId)
        {
            var appointment = await _repository.GetAppointmentWithDetailsAsync(appointmentId);
            if (appointment == null) return null;

            // Note: We no longer need to get active timer or total time here,
            // but GetAppointmentWithDetailsAsync is still used by other methods.
            // We will build the DTO manually for this single case.

            var activeTimer = appointment.TimeLogs?.FirstOrDefault(tl => tl.IsActive);
            var totalTime = await GetTotalLoggedTimeAsync(appointmentId);

            var serviceProgress = new ServiceProgressDto
            {
                AppointmentId = appointment.AppointmentId,
                CustomerName = appointment.User?.FirstName + " " + appointment.User?.LastName,
                CustomerId = appointment.UserId,
                CustomerVehicleName = appointment.CustomerVehicle?.Brand + " " + appointment.CustomerVehicle?.Model,
                Status = appointment.Status,
                ServiceType = appointment.Type,
                AppointmentDateTime = appointment.DateTime,
                IsTimerActive = activeTimer != null,
                CurrentTimerStartTime = activeTimer?.StartDateTime,
                TotalTimeLogged = totalTime
                // TimeLogs list is no longer part of the DTO
            };

            if (appointment.Type == automobile_backend.Models.Entities.Type.Service)
            {
                serviceProgress.ServiceNames = appointment.AppointmentServices?
                    .Select(asv => asv.Service.ServiceName)
                    .ToList() ?? new List<string>();
            }
            else if (appointment.Type == automobile_backend.Models.Entities.Type.Modifications)
            {
                var modification = appointment.ModificationRequests?.FirstOrDefault();
                if (modification != null)
                {
                    serviceProgress.ModificationTitle = modification.Title;
                    serviceProgress.ModificationDescription = modification.Description;
                }
            }

            return serviceProgress;
        }

        public async Task<TimerResponseDto> StartTimerAsync(TimerActionDto timerAction)
        {
            try
            {
                var existingTimer = await _repository.GetActiveTimerAsync(timerAction.AppointmentId, timerAction.UserId);
                if (existingTimer != null)
                {
                    return new TimerResponseDto { Success = false, Message = "Timer is already running for this appointment." };
                }

                var appointment = await _repository.GetAppointmentWithDetailsAsync(timerAction.AppointmentId);
                if (appointment == null)
                {
                    return new TimerResponseDto { Success = false, Message = "Appointment not found." };
                }

                if (appointment.Status == AppointmentStatus.Upcoming)
                {
                    appointment.Status = AppointmentStatus.InProgress;
                    if (appointment.StartDateTime == default)
                    {
                        appointment.StartDateTime = DateTime.UtcNow;
                    }
                    await _repository.UpdateAppointmentAsync(appointment);
                }

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
                return new TimerResponseDto { Success = false, Message = $"Error starting timer: {ex.Message}" };
            }
        }

        public async Task<TimerResponseDto> PauseTimerAsync(TimerActionDto timerAction)
        {
            try
            {
                var activeTimer = await _repository.GetActiveTimerAsync(timerAction.AppointmentId, timerAction.UserId);
                if (activeTimer == null)
                {
                    return new TimerResponseDto { Success = false, Message = "No active timer found for this appointment." };
                }

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
                return new TimerResponseDto { Success = false, Message = $"Error pausing timer: {ex.Message}" };
            }
        }

        public async Task<TimerResponseDto> StopTimerAsync(TimerActionDto timerAction)
        {
            try
            {
                var activeTimer = await _repository.GetActiveTimerAsync(timerAction.AppointmentId, timerAction.UserId);
                if (activeTimer == null)
                {
                    return new TimerResponseDto { Success = false, Message = "No active timer found for this appointment." };
                }

                var timeElapsed = DateTime.UtcNow - activeTimer.StartDateTime;
                activeTimer.EndDateTime = DateTime.UtcNow;
                activeTimer.HoursLogged = (decimal)timeElapsed.TotalHours;
                activeTimer.IsActive = false;

                await _repository.UpdateTimeLogAsync(activeTimer);

                var appointment = await _repository.GetAppointmentWithDetailsAsync(timerAction.AppointmentId);
                if (appointment != null)
                {
                    appointment.Status = AppointmentStatus.Completed;
                    appointment.EndDateTime = DateTime.UtcNow;
                    await _repository.UpdateAppointmentAsync(appointment);
                    await CreatePaymentRecordAsync(appointment);

                    // NEW: send completion email
                    await TrySendCompletionEmailAsync(appointment.AppointmentId);
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
                return new TimerResponseDto { Success = false, Message = $"Error stopping timer: {ex.Message}" };
            }
        }

        public async Task<bool> UpdateServiceStatusAsync(UpdateStatusDto updateStatus)
        {
            try
            {
                var appointment = await _repository.GetAppointmentWithDetailsAsync(updateStatus.AppointmentId);
                if (appointment == null) return false;

                appointment.Status = updateStatus.NewStatus;

                if (updateStatus.NewStatus == AppointmentStatus.InProgress && appointment.StartDateTime == default)
                {
                    appointment.StartDateTime = DateTime.UtcNow;
                }

                if (updateStatus.NewStatus == AppointmentStatus.Completed)
                {
                    appointment.EndDateTime = DateTime.UtcNow;

                    var activeTimer = await _repository.GetActiveTimerAsync(updateStatus.AppointmentId, updateStatus.UserId);
                    if (activeTimer != null)
                    {
                        var timeElapsed = DateTime.UtcNow - activeTimer.StartDateTime;
                        activeTimer.EndDateTime = DateTime.UtcNow;
                        activeTimer.HoursLogged = (decimal)timeElapsed.TotalHours;
                        activeTimer.IsActive = false;
                        await _repository.UpdateTimeLogAsync(activeTimer);
                    }

                    await CreatePaymentRecordAsync(appointment);

                    // NEW: send completion email
                    await TrySendCompletionEmailAsync(appointment.AppointmentId);
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

        private async Task CreatePaymentRecordAsync(Appointment appointment)
        {
            try
            {
                var existingPayment = await _paymentRepository.GetByAppointmentIdAsync(appointment.AppointmentId);
                if (existingPayment != null) return;

                var payment = new Payment
                {
                    AppointmentId = appointment.AppointmentId,
                    Amount = appointment.Amount,
                    Status = PaymentStatus.Pending,
                    PaymentMethod = PaymentMethod.CreditCard,
                    PaymentDateTime = DateTime.UtcNow,
                    InvoiceLink = null
                };

                await _paymentRepository.CreateAsync(payment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating payment record: {ex.Message}");
            }
        }

        // NEW helper
        private async Task TrySendCompletionEmailAsync(int appointmentId)
        {
            try
            {
                await _notifications.SendStatusUpdateAsync("Appointment", appointmentId, "Completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed sending completion email for appointment {appointmentId}: {ex.Message}");
            }
        }
    }
}