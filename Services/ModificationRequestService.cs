using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class ModificationRequestService : IModificationRequestService
    {
        private readonly IModificationRequestRepository _requestRepository;

        public ModificationRequestService(IModificationRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task<IEnumerable<ModificationRequest>> GetAllModificationRequestsAsync()
        {
            return await _requestRepository.GetAllAsync();
        }
    }
}
