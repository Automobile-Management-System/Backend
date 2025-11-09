// using automobile_backend.Models.Entities;

// namespace automobile_backend.Models.DTOs
// {
//     public class AppointmentResponseDto
//     {
//         public int AppointmentId { get; set; }
//         public DateTime DateTime { get; set; }
//         public AppointmentStatus Status { get; set; }
//         public int UserId { get; set; }
//         public string UserName { get; set; }

//         public int VehicleId { get; set; }
//         public string RegistrationNumber { get; set; } = string.Empty;

//         public List<ServiceDto> Services { get; set; } = new();

//         public static implicit operator AppointmentResponseDto(AppointmentResponseDto v)
//         {
//             throw new NotImplementedException();
//         }
//     }

//     public class ServiceDto
//     {
//         public int ServiceId { get; set; }
//         public string ServiceName { get; set; }
//         public decimal BasePrice { get; set; }
//     }
// }

using automobile_backend.Models.Entities;
using System;
using System.Collections.Generic;

namespace automobile_backend.Models.DTOs
{
    public class AppointmentResponseDto
    {
        public int AppointmentId { get; set; }
        public DateTime DateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public int VehicleId { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;

        public List<ServiceDto> Services { get; set; } = new();

        // Default constructor
        public AppointmentResponseDto() { }

        // Copy constructor for cloning
        public AppointmentResponseDto(AppointmentResponseDto other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            AppointmentId = other.AppointmentId;
            DateTime = other.DateTime;
            Status = other.Status;
            UserId = other.UserId;
            UserName = other.UserName;
            VehicleId = other.VehicleId;
            RegistrationNumber = other.RegistrationNumber;

            // Deep copy of services
            Services = new List<ServiceDto>();
            foreach (var service in other.Services)
            {
                Services.Add(new ServiceDto
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    BasePrice = service.BasePrice
                });
            }
        }
    }

    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
    }
}
