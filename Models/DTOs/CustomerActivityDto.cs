namespace automobile_backend.Models.DTOs
{
    public class CustomerActivityDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public double AverageAppointmentsPerCustomer { get; set; }
        public double AverageRating { get; set; }
    }
}