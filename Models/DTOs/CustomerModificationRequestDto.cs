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

        // Returns date in Sri Lanka time as string
        public string CreatedDateString
        {
            get
            {
                var slTime = TimeZoneInfo.ConvertTimeFromUtc(
                    CreatedDate.ToUniversalTime(),
                    TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time")
                );
                return slTime.ToString("dd MMM yyyy"); // e.g., 28 Oct 2025
            }
        }

        // Returns time in Sri Lanka time as string
        public string CreatedTimeString
        {
            get
            {
                var slTime = TimeZoneInfo.ConvertTimeFromUtc(
                    CreatedDate.ToUniversalTime(),
                    TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time")
                );
                return slTime.ToString("hh:mm tt"); // e.g., 09:15 AM
            }
        }

        public string RequestStatus { get; set; } = "Pending";
        public int AppointmentId { get; set; }
        public string AppointmentSummary { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
