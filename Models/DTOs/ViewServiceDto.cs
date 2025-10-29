namespace automobile_backend.Models.DTOs
{
    public class ViewServiceDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
    }
}
