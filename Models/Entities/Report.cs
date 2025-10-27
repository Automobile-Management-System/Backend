using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime GeneratedDate { get; set; }

        [Required]
        public string ReportLink { get; set; } = string.Empty;

        // Foreign Key for the User who generated the report
        public int UserId { get; set; }

        // Navigation property to the User
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}