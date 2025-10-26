using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{


    public class EmployeeAppointment
    {
        // Foreign Key for Appointment
        public int AppointmentId { get; set; }

        // Navigation property
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        // Foreign Key for Service
        public int UserId { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}