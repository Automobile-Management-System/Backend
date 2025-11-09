namespace automobile_backend.Models.DTOs
{
    public class CreateModificationRequestDto
    {
        public int CustomerId { get; set; }
        public int AppointmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Priority { get; set; } = "normal";
    }

    public class ReviewRequestDto
    {
        public string Action { get; set; } = string.Empty; // "approve" or "reject"
        public string? AdminResponse { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? RespondedBy { get; set; }
    }

    public class ModificationRequestDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int AppointmentId { get; set; }
        public string? ServiceType { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? EstimatedCost { get; set; }
        public string? AdminResponse { get; set; }
        public string? RespondedBy { get; set; }
        public DateTime? RespondedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ModificationRequestStatsDto
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int CompletedRequests { get; set; }
        public decimal TotalEstimatedCost { get; set; }
    }
}