using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IChatbotService
    {
        Task<string> GetResponseAsync(string message);
    }
}
