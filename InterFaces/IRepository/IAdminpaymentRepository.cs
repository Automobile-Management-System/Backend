using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.Repositories
{
    public interface IAdminpaymentRepository
    {
        Task<IEnumerable<AdminPaymentDetailDto>> GetAllPaymentsWithCustomerDetailsAsync();

    }
}