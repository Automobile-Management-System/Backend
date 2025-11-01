namespace automobile_backend.InterFaces.IServices
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoiceAsync(int appointmentId, int userId);

        Task<string?> GenerateAndUploadInvoiceAsync(int paymentId);
    }
}