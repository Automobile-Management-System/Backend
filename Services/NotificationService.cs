using automobile_backend.InterFaces.IServices;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace automobile_backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext db, IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _db = db;
            _configuration = configuration;
            _logger = logger;
        }

        // Not used in your scenario, kept for interface compliance
        public async Task SendAppointmentReminderAsync(int appointmentId)
        {
            await Task.CompletedTask;
        }

        // Called from your endpoint: SendStatusUpdateAsync("Appointment", id, "Completed")
        public async Task SendStatusUpdateAsync(string entityType, int entityId, string newStatus)
        {
            try
            {
                if (!string.Equals(entityType, "Appointment", StringComparison.OrdinalIgnoreCase))
                    return;

                if (!string.Equals(newStatus, "Completed", StringComparison.OrdinalIgnoreCase))
                    return;

                var appt = await _db.Appointments
                    .Include(a => a.User)
                    .Include(a => a.CustomerVehicle)
                    .Include(a => a.AppointmentServices).ThenInclude(aps => aps.Service)
                    .FirstOrDefaultAsync(a => a.AppointmentId == entityId);

                if (appt == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found. Skipping email.", entityId);
                    return;
                }

                var to = appt.User?.Email;
                if (string.IsNullOrWhiteSpace(to))
                {
                    _logger.LogWarning("No customer email found for Appointment {AppointmentId}.", entityId);
                    return;
                }

                var subject = $"Completed: Your {(appt.Type == Models.Entities.Type.Service ? "service" : "modification")} appointment (#{appt.AppointmentId})";
                var body = BuildCompletionBody(appt);

                await SendEmailAsync(to, subject, body, isHtml: true);
                _logger.LogInformation("Completion email sent to {Email} for Appointment {AppointmentId}.", to, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending completion email for Appointment {AppointmentId}.", entityId);
            }
        }

        public async Task SendRealTimeUpdateAsync(string userId, object payload)
        {
            await Task.CompletedTask;
        }

        private static string BuildCompletionBody(Models.Entities.Appointment appt)
        {
            var services = appt.AppointmentServices?
                .Select(s => s.Service?.ServiceName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList() ?? new();

            var vehicle = $"{WebUtility.HtmlEncode(appt.CustomerVehicle?.Brand)} {WebUtility.HtmlEncode(appt.CustomerVehicle?.Model)} ({WebUtility.HtmlEncode(appt.CustomerVehicle?.RegistrationNumber)})";
            var serviceList = services.Count > 0 ? string.Join(", ", services) : "N/A";

            var sb = new StringBuilder();
            sb.Append($@"<div style=""font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#222"">
<h2>Appointment Completed</h2>
<p>Hi {WebUtility.HtmlEncode(appt.User?.FirstName ?? "Customer")},</p>
<p>Your {(appt.Type == Models.Entities.Type.Service ? "service" : "modification")} appointment has been completed.</p>
<ul>
  <li><strong>Appointment #:</strong> {appt.AppointmentId}</li>
  <li><strong>Date/Time:</strong> {appt.DateTime:yyyy-MM-dd HH:mm}</li>
  <li><strong>Vehicle:</strong> {vehicle}</li>
  <li><strong>Services:</strong> {WebUtility.HtmlEncode(serviceList)}</li>
  <li><strong>Amount:</strong> LKR {appt.Amount:N2}</li>
</ul>
<p>If payment is pending, you can complete it from your dashboard.</p>
<p>Thank you for choosing AutoServe 360.</p>
</div>");
            return sb.ToString();
        }

        private async Task SendEmailAsync(string to, string subject, string body, bool isHtml)
        {
            var host = _configuration["Smtp:Host"];
            var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
            var user = _configuration["Smtp:Username"];
            var pass = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"] ?? user;
            var fromName = _configuration["Smtp:FromName"] ?? "AutoServe 360";
            var enableSsl = bool.TryParse(_configuration["Smtp:EnableSsl"], out var ssl) ? ssl : true;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                _logger.LogWarning("SMTP settings missing. Email to {Recipient} not sent.", to);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(new MailAddress(to));

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(user, pass),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            await client.SendMailAsync(message);
        }
    }
}
