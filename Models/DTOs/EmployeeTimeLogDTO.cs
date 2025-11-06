public class EmployeeTimeLogDTO
{
    public int LogId { get; set; }
    public string CustomerName { get; set; }
    public string VehicleRegNumber { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public decimal HoursLogged { get; set; }

    public List<string> CompletedServices { get; set; }
    public List<string> CompletedModifications { get; set; }
}
