using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
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
    public DbSet<Payment> Payments { get; set; }
    public DbSet<AddOn> AddOns { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ModificationRequest> ModificationRequests { get; set; }
    public DbSet<TimeLog> TimeLogs { get; set; }
    public DbSet<Report> Reports { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- User Configuration ---
        // Make Email unique to prevent duplicate accounts
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // --- AppointmentService (Many-to-Many Join Table) Configuration ---
        // Define the composite primary key
        modelBuilder.Entity<AppointmentService>()
            .HasKey(aps => new { aps.AppointmentId, aps.ServiceId });

        // Configure the many-to-many relationship between Appointment and Service
        modelBuilder.Entity<AppointmentService>()
            .HasOne(aps => aps.Appointment)
            .WithMany(a => a.AppointmentServices)
            .HasForeignKey(aps => aps.AppointmentId);

        modelBuilder.Entity<AppointmentService>()
            .HasOne(aps => aps.Service)
            .WithMany(s => s.AppointmentServices)
            .HasForeignKey(aps => aps.ServiceId);


        // --- One-to-One Relationship Configurations ---
        // Appointment <-> Payment
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Payment)
            .WithOne(p => p.Appointment)
            .HasForeignKey<Payment>(p => p.AppointmentId);

        // Appointment <-> Review
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Review)
            .WithOne(r => r.Appointment)
            .HasForeignKey<Review>(r => r.AppointmentId);


        // --- Delete Behavior Configurations ---
        // Prevent cascade delete from User to Appointment. 
        // If a user is deleted, you might want to keep their appointment history.
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.User)
            .WithMany() // Assuming User model will have an ICollection<Appointment>
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent cascade delete from User to TimeLog.
        // If a staff member's account is deleted, their logged time should likely remain.
        modelBuilder.Entity<TimeLog>()
           .HasOne(tl => tl.User)
           .WithMany() // Assuming User model will have an ICollection<TimeLog>
           .HasForeignKey(tl => tl.UserId)
           .OnDelete(DeleteBehavior.Restrict);

        // If an appointment is deleted, also delete related time logs.
        modelBuilder.Entity<TimeLog>()
           .HasOne(tl => tl.Appointment)
           .WithMany(a => a.TimeLogs)
           .HasForeignKey(tl => tl.AppointmentId)
           .OnDelete(DeleteBehavior.Cascade);
    }
}