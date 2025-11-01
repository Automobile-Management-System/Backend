using automobile_backend.Models.Entities;

namespace automobile_backend.Models.DTOs
{
    public class SlotAvailabilityDto
    {
        public SlotsTime Slot { get; set; }
        public int Booked { get; set; }
        public int Capacity { get; set; } = 5;
        public int Remaining => Capacity - Booked;
        public bool IsAvailable => Remaining > 0;
    }
}