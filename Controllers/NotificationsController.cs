using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Policy = "Staff")] // Re-enable when ready
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notifications;

        public NotificationsController(INotificationService notifications)
        {
            _notifications = notifications;
        }

        [HttpPost("appointments/{appointmentId:int}/completed")]
        // [AllowAnonymous]
        public async Task<IActionResult> SendAppointmentCompleted(int appointmentId)
        {
            try
            {
                await _notifications.SendStatusUpdateAsync("Appointment", appointmentId, "Completed");
                return Ok(new { message = "Completion email triggered.", appointmentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send completion email.", error = ex.Message });
            }
        }

        // NEW: trigger email for modification approval
        [HttpPost("appointments/{appointmentId:int}/modification-approved")]
        // [Authorize(Policy = "Staff")]
        public async Task<IActionResult> SendModificationApproved(int appointmentId)
        {
            try
            {
                await _notifications.SendStatusUpdateAsync("Appointment", appointmentId, "Approved");
                return Ok(new { message = "Approval email triggered.", appointmentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send approval email.", error = ex.Message });
            }
        }

        // NEW: trigger email for modification rejection
        [HttpPost("appointments/{appointmentId:int}/modification-rejected")]
        // [Authorize(Policy = "Staff")]
        public async Task<IActionResult> SendModificationRejected(int appointmentId)
        {
            try
            {
                await _notifications.SendStatusUpdateAsync("Appointment", appointmentId, "Rejected");
                return Ok(new { message = "Rejection email triggered.", appointmentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send rejection email.", error = ex.Message });
            }
        }
    }
}