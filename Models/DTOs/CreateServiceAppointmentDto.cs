using automobile_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace automobile_backend.Models.DTOs
{
    public class CreateServiceAppointmentDto
    {
        public DateTime AppointmentDateTime { get; set; }
        public SlotsTime SlotsTime { get; set; }
        public List<int> ServiceIds { get; set; }
        public string? Notes { get; set; }
        public int VehicleId { get; set; } // Add this property to fix CS1061
    }
}
