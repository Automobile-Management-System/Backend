using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/modification-requests")]
    public class ModificationRequestController : ControllerBase
    {
        private readonly IModificationRequestService _requestService;

        public ModificationRequestController(IModificationRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _requestService.GetAllModificationRequestsAsync();
            return Ok(requests);
        }
    }
}
