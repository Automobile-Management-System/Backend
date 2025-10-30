using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.Services
{
    public interface IAdminpaymentService
    {
        Task<IEnumerable<AdminPaymentDetailDto>> GetAllPaymentsAsync();

    }
}