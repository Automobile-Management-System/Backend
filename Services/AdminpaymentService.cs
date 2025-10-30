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

        public async Task<IEnumerable<AdminPaymentDetailDto>> GetAllPaymentsAsync()
        {
            // You can add any business logic here (e.g., logging)
            return await _repository.GetAllPaymentsWithCustomerDetailsAsync();
        }

    }
}