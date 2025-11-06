using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Policy = "Staff")] // Temporarily disable for testing if needed
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notifications;

        public NotificationsController(INotificationService notifications)
        {
            _notifications = notifications;
        }

        [HttpPost("appointments/{appointmentId:int}/completed")]
        // [AllowAnonymous] // Uncomment for quick testing (remember to re-secure)
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
    }
}