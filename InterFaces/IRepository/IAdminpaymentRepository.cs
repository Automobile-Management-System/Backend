using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.Repositories
{
    public interface IAdminpaymentRepository
    {
        Task<(IEnumerable<AdminPaymentDetailDto> Items, int TotalCount)> GetAllPaymentsWithCustomerDetailsAsync(int pageNumber, int pageSize);

        Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus newStatus);

        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetPaymentCountByStatusAsync(PaymentStatus status);

    }
}