using System;

namespace automobile_backend.Models.DTOs
{
    public class CustomerModificationRequestDto
    {
        public int ModificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public DateTime CreatedDate { get; set; }

        // Formatted date/time strings for frontend
        public string CreatedDateString
        {
            get
            {
                var slTime = TimeZoneInfo.ConvertTimeFromUtc(
                    CreatedDate.ToUniversalTime(),
                    TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time")
                );
                return slTime.ToString("dd MMM yyyy");
            }
        }

        public string CreatedTimeString
        {
            get
            {
                var slTime = TimeZoneInfo.ConvertTimeFromUtc(
                    CreatedDate.ToUniversalTime(),
                    TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time")
                );
                return slTime.ToString("hh:mm tt");
            }
        }

        public string RequestStatus { get; set; } = "Pending"; // from Appointment.Status
        public int AppointmentId { get; set; } = 0;
        public int UserId { get; set; } // filled automatically from claims
    }
}
