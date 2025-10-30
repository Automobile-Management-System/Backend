using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace automobile_backend.Models.DTOs
{
    public class CreateServiceAppointmentDto
    {
        [Required]
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one service must be selected.")]
        public List<int> ServiceIds { get; set; }

        // Optional: You can add fields like vehicle id or notes later
        public string? Notes { get; set; }
    }
}
