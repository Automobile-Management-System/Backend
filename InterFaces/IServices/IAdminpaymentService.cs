using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.Services
{
    public interface IAdminpaymentService
    {
        Task<(IEnumerable<AdminPaymentDetailDto> Items, int TotalCount)> GetAllPaymentsAsync(int pageNumber, int pageSize);

        Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus newStatus);

        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetPendingCountAsync();
        Task<int> GetCompletedCountAsync();
        Task<int> GetFailedCountAsync();
    }
}