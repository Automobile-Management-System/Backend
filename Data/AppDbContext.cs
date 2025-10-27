namespace automobile_backend.Data
{
    // These 'using' statements are critical
    using automobile_backend.Models.Entities;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : DbContext // <- Correct class name
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Add DbSet for each of your entities
        public DbSet<User> Users { get; set; }
        public DbSet<CustomerVehicle> CustomerVehicles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentService> AppointmentServices { get; set; }
        public DbSet<EmployeeAppointment> EmployeeAppointments { get; set; }
        public DbSet<Payment> Payments { get; set; } // <- This will now work
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ModificationRequest> ModificationRequests { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User Configuration ---
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // --- AppointmentService (Many-to-Many Join Table) Configuration ---
            modelBuilder.Entity<AppointmentService>()
                .HasKey(aps => new { aps.AppointmentId, aps.ServiceId });

            modelBuilder.Entity<AppointmentService>()
                .HasOne(aps => aps.Appointment)
                .WithMany(a => a.AppointmentServices)
                .HasForeignKey(aps => aps.AppointmentId);

            modelBuilder.Entity<AppointmentService>()
                .HasOne(aps => aps.Service)
                .WithMany(s => s.AppointmentServices)
                .HasForeignKey(aps => aps.ServiceId);

            // --- One-to-One Relationship Configurations ---
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Payment)
                .WithOne(p => p.Appointment)
                .HasForeignKey<Payment>(p => p.AppointmentId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Review)
                .WithOne(r => r.Appointment)
                .HasForeignKey<Review>(r => r.AppointmentId);

            // --- Delete Behavior Configurations ---
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimeLog>()
               .HasOne(tl => tl.User)
               .WithMany()
               .HasForeignKey(tl => tl.UserId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimeLog>()
               .HasOne(tl => tl.Appointment)
               .WithMany(a => a.TimeLogs)
               .HasForeignKey(tl => tl.AppointmentId)
               .OnDelete(DeleteBehavior.Cascade);

            // --- EmployeeAppointment (Many-to-Many Join Table) Configuration ---
            modelBuilder.Entity<EmployeeAppointment>()
                .HasKey(ea => new { ea.AppointmentId, ea.UserId });

            modelBuilder.Entity<EmployeeAppointment>()
                .HasOne(ea => ea.Appointment)
                .WithMany(a => a.EmployeeAppointments)
                .HasForeignKey(ea => ea.AppointmentId);

            modelBuilder.Entity<EmployeeAppointment>()
                .HasOne(ea => ea.User)
                .WithMany()
                .HasForeignKey(ea => ea.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}