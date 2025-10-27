using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities; // For PaymentMethod enum
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<InvoiceDto>> GetCustomerInvoicesAsync(int customerId)
        {
            // 1. Get the base query from the repository
            var appointmentsQuery = _paymentRepository.GetCustomerAppointmentsWithDetails(customerId);

            // 2. Project the query into the DTO and execute it
            var invoices = await appointmentsQuery
                // --- FIX #1 ---
                .OrderByDescending(a => a.StartDateTime) 
                .Select(a => new InvoiceDto
                {
                    AppointmentId = a.AppointmentId,
                    InvoiceNumber = $"INV-{a.AppointmentId:D8}", 
                    
                    ServiceName = string.Join(", ", a.AppointmentServices.Select(aps => aps.Service.ServiceName)),
                    
                    Status = (a.Payment != null) ? "paid" : "pending",
                    
                    Amount = (a.Payment != null) ? a.Payment.Amount : a.AppointmentServices.Sum(aps => aps.Service.Price),
                    
                    // --- FIX #2 ---
                    Date = (a.Payment != null) ? a.Payment.PaymentDateTime : a.StartDateTime,
                    
                    // --- FIX #3 ---
                    DueDate = (a.Payment == null) ? a.StartDateTime.AddDays(7) : null,
                    
                    PaymentMethod = a.Payment != null ? a.Payment.PaymentMethod.ToString() : null, 
                    
                    InvoiceLink = a.Payment != null ? a.Payment.InvoiceLink : null
                })
                .ToListAsync();

            return invoices;
        }
    }
}