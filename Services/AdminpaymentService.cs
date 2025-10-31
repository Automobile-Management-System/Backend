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

    }
}