using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Entities = automobile_backend.Models.Entities;

namespace automobile_backend.Services
{
    public interface IAdminpaymentService
    {
        Task<(IEnumerable<AdminPaymentDetailDto> Items, int TotalCount)> GetAllPaymentsAsync(
            int pageNumber,
            int pageSize,
            string? search,
            PaymentStatus? status,
            PaymentMethod? paymentMethod);

        Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus newStatus);

        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetPendingCountAsync();
        Task<int> GetCompletedCountAsync();
        Task<int> GetFailedCountAsync();

        Task<(byte[] pdfBytes, string fileName)> GeneratePaymentsReportAsync(
            string? search,
            Entities.PaymentStatus? status,
            Entities.PaymentMethod? paymentMethod);
    }
}