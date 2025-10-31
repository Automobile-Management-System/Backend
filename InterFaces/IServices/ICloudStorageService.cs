namespace automobile_backend.InterFaces.IServices
{
    public interface ICloudStorageService
    {
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType);
    }
}