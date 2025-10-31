using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using automobile_backend.InterFaces.IServices;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class CloudStorageService : ICloudStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public CloudStorageService(IConfiguration configuration)
        {
            var keyPath = configuration["Firebase:ServiceAccountKeyPath"];
            if (string.IsNullOrEmpty(keyPath))
                throw new Exception("Firebase ServiceAccountKeyPath is not configured.");
            
            var credential = GoogleCredential.FromFile(keyPath);
            _storageClient = StorageClient.Create(credential);
            _bucketName = configuration["Firebase:BucketName"];
            if (string.IsNullOrEmpty(_bucketName))
                throw new Exception("Firebase BucketName is not configured.");
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType)
        {
            using (var memoryStream = new MemoryStream(fileBytes))
            {
                var options = new UploadObjectOptions
                {
                    // This makes the file publicly readable
                    PredefinedAcl = PredefinedObjectAcl.PublicRead 
                };

                await _storageClient.UploadObjectAsync(_bucketName, fileName, contentType, memoryStream, options);

                // Return the public URL
                return $"https://storage.googleapis.com/{_bucketName}/{fileName}";
            }
        }
    }
}