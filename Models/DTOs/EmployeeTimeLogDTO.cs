namespace automobile_backend.Models.DTO
{
    public class EmployeeTimeLogDTO
    {
        public int LogId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public decimal HoursLogged { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new();
        public List<string> Modifications { get; set; } = new();
    }
}
