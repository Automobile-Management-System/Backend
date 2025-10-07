using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    // This class acts as the join table for the many-to-many relationship
    // To set up a composite primary key (AppointmentId, ServiceId),
    // you would typically use Fluent API in your DbContext.
    // modelBuilder.Entity<AppointmentService>()
    //     .HasKey(aps => new { aps.AppointmentId, aps.ServiceId });

    public class AppointmentService
    {
        // Foreign Key for Appointment
        public int AppointmentId { get; set; }

        // Navigation property
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        // Foreign Key for Service
        public int ServiceId { get; set; }

        // Navigation property
        [ForeignKey("ServiceId")]
        public Service Service { get; set; }
    }
}