using System;
using System.Collections.Generic;

namespace automobile_backend.Models.DTOs
{
    public class WeeklyRevenueDto
    {
       public List<string> Days { get; set; } = new List<string>(); // ["Mon", "Tue", "Wed", ...]
      public List<decimal> RevenueList { get; set; } = new List<decimal>(); // Revenue for each day
        public string Currency { get; set; } = "LKR"; // Currency indicator
    }

    public class WeeklyAppointmentsDto
    {
  public List<string> Days { get; set; } = new List<string>(); // ["Mon", "Tue", "Wed", ...]
     public List<int> Appointments { get; set; } = new List<int>(); // Appointment count for each day
    }

    public class RecentUserDto
    {
      public int UserId { get; set; }
   public string FullName { get; set; } = string.Empty;
   public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
 public string? ProfilePicture { get; set; }
     public DateTime RegisteredDate { get; set; }
    }
}