using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IPaymentService
    {
        /// <summary>
        /// Gets a formatted list of all invoices (paid and pending) for a customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer (user).</param>
        /// <returns>A list of InvoiceDto objects.</returns>
        Task<IEnumerable<InvoiceDto>> GetCustomerInvoicesAsync(int customerId);
    }
}