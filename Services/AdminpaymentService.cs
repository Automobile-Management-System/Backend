using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using automobile_backend.Repositories;

namespace automobile_backend.Services
{
    public class AdminpaymentService : IAdminpaymentService
    {
        private readonly IAdminpaymentRepository _repository;

        public AdminpaymentService(IAdminpaymentRepository repository)
        {
            _repository = repository;
        }

        public async Task<(IEnumerable<AdminPaymentDetailDto> Items, int TotalCount)> GetAllPaymentsAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetAllPaymentsWithCustomerDetailsAsync(pageNumber, pageSize);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus newStatus)
        {
            // You can add any business logic here (e.g., logging)
            return await _repository.UpdatePaymentStatusAsync(paymentId, newStatus);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            // Business logic: Revenue is defined as the sum of *Completed* payments.
            return await _repository.GetTotalRevenueAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(PaymentStatus.Pending);
        }

        public async Task<int> GetCompletedCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(PaymentStatus.Completed);
        }

        public async Task<int> GetFailedCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(PaymentStatus.Failed);
        }

    }
}